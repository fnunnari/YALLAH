// #define DEBUG_GEOM

using System;
using UnityEngine;


namespace HaxeSpeedTest {

	public class EyeHeadGazeLogic
	{
		private const string EYES_RIGHT = "Expressions_eyesHoriz_min";
		private const string EYES_LEFT = "Expressions_eyesHoriz_max";
		private const string EYES_DOWN = "Expressions_eyesVert_min";
		private const string EYES_UP = "Expressions_eyesVert_max";

		public static Vector3 HORIZ_PLANE_NORMAL = Vector3.up;


		/** Returns the name of the four blendSHapes used to drive the eyes. In order:
		 * EYES_RIGHT, EYES_LEFT, EYES_DOWN, EYES_UP.
		 */
		public static string[] VISEMES = new string[] { EYES_RIGHT, EYES_LEFT, EYES_DOWN, EYES_UP };

		/** Returns the number of Visemes which this module expects to handle.*/
		public static int get_viseme_count()
		{
			return VISEMES.Length;
		}


		// Determines in degree at which point the neck should rotate
		// In more extreme angles the neck will rotate otherwise only the eyes
		private const float EYES_MAX_YAW = 30.0f;
		private const float EYES_MAX_PITCH = 12.0f;

		// Neck rotation limits (degs)
		// https://www.livestrong.com/article/95456-normal-neck-range-motion/
		private const float NECK_MAX_PITCH = 45.0f;  // Neck extension (45..70)
		private const float NECK_MIN_PITCH = -40.0f;  // Neck flexion ( 40..60)
		private const float NECK_MAX_YAW = 60.0f;  // Neck rotation (60..80)
		private const float NECK_MIN_YAW = -60.0f;  // Neck rotation
		// Unused (for the moment)
		//	private const float NECK_MAX_ROLL = 45.0f;  // Neck lateral bending
		//	private const float NECK_MIN_ROLL = -45.0f;  // Neck lateral bending

		/** The speed or rotation (deg/sec) of the eyes during fixation. */
		public float eyesRotSpeed = 1000.0f;

		/** Current eyes pitch (degs, vertical). */
		private float eyesPitch = 0.0f ;
		/** Current eyes yaw (degs, horizontal). */
		private float eyesYaw = 0.0f;

		/** Whether to enable or not the neck rotation. */
		public bool enableNeckRotation = true;

		/**  The time (secs) the neck needs to re-align when the target is beyond eyes limits. */
		public float neckRotTime = 0.1f;

		/** The maximum rotation speed (deg/sec) of the neck.
		 * Here: https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4616101/
		 * They consider slow/high rotation speed for the neck as 90 - 180 deg/sec
		 */
		public float neckRotMaxSpeed = 135.0f;

		/** Current neck pitch (degs). */
		private float neckYaw = 0.0f;
		/** Current neck yaw (degs). */
		private float neckPitch = 0.0f;

		// How strongly the neck tends to reset ita rotation when the target is in the rotation range of the eyes
		public float neckRestTendencyStrenght = 0.25f;

		/** The target point to fixate. */
		public Nullable<Vector3> eyeGazeTargetPoint = null;

		private Quaternion aPoseLeftEyeRotation;
		private Quaternion aPoseRightEyeRotation;
		private Quaternion aPoseNeckRotation;
        private Quaternion aPoseNeckLocalRotation = new Quaternion();

        public Vector3 aPoseRightDirection;
        public Vector3 aPoseFwdDirection;


        //
        // CONSTRUCTOR
        //
        public EyeHeadGazeLogic (Vector3 aPoseLeftEyePosition, Vector3 aPoseRightEyePosition,
                                 Quaternion aPoseLeftEyeRotation, Quaternion aPoseRightEyeRotation,
                                 Quaternion aPoseNeckRotation,
                                 Quaternion aPoseNeckLocalRotation)
		{
            this.aPoseLeftEyeRotation = aPoseLeftEyeRotation;
			this.aPoseRightEyeRotation = aPoseRightEyeRotation;
			this.aPoseNeckRotation = aPoseNeckRotation;
            this.aPoseNeckLocalRotation.Set(aPoseNeckLocalRotation.x,
                                            aPoseNeckLocalRotation.y,
                                            aPoseNeckLocalRotation.z,
                                            aPoseNeckLocalRotation.w);

            //
            // Compute reference axes
            this.aPoseRightDirection = aPoseRightEyePosition - aPoseLeftEyePosition;
            // This angle should be 90 degrees
            float check_angle = Vector3.Angle(this.aPoseRightDirection, HORIZ_PLANE_NORMAL);
            Debug.Log("Angle between eyes traverse and vertical vector: " + check_angle);
            if (check_angle != 90.0f)
            {
                Debug.LogWarning("The angle between the eyes traverse and vertical vector is not 90. This might cause errors in the eye/head tracking.");
            }
                 
            this.aPoseFwdDirection = Vector3.Cross(this.aPoseRightDirection, HORIZ_PLANE_NORMAL);

            Debug.Log("aPoseRightDirection=" + aPoseRightDirection);
            Debug.Log("aPoseFwdDirection=" + aPoseFwdDirection);

		}


        //
        // PUBLIC Methods
        //

		/** Brings beck neck and eyes at the default position. */
		//public void ResetEyeGaze() {
		//	this.eyesPitch = 0.0f;
		//	this.eyesYaw = 0.0f;
		//	this.neckPitch = 0.0f;
		//	this.neckYaw = 0.0f;
		//}

		/** Starts moving eyes and head to watch a spcified position.
		 */
		public void LookAtPoint(Vector3 targetPoint){
			//Debug.Log (eyeGazeTargetPoint + "    " + currentEyeGazePoint); 
			this.eyeGazeTargetPoint = targetPoint;
		}


        //
		// PRIVATE Methods
		//

		private static float SmoothtInc(float delta, float time, float max_speed, float dt) {
			if (Math.Abs(delta) < 0.0001f) {  // Otherwise we generate numerical jumps
				return 0.0f;
			}

			float speed = delta / time;
			if (speed > max_speed) {
				speed = max_speed;
			} else if (speed < -max_speed) {
				speed = -max_speed;
			}
			float inc = speed * dt;
			if (Mathf.Abs (inc) > Mathf.Abs (delta)) {
				Debug.Log ("RotSmoothtInc: incrementing too much!!!");
			}
			return inc;
		}

		private static float LinearInc(float delta, float speed, float dt) {
			float inc = speed * dt;
			inc = Mathf.Min (inc, Mathf.Abs (delta));
			inc *= Mathf.Sign (delta);
			return inc;
		}

        //
        // UPDATE (main refresh routine)
        //

        //sets the eyes to look at the target
        // target_point is the absolute point in space that we want to wantch at.
        public void UpdateEyesRotation(Vector3 leftEyePosition, Vector3 rightEyePosition,
            Quaternion leftEyeRotation, Quaternion rightEyeRotation,
            ref float[] outBlendShapeWeights,
            ref Quaternion inOutNeckRotation,
            float dt)
        {

            //
            // If the target is null, we reset all target angles to 0.
            float newEyesPitch = 0.0f;
            float newEyesYaw = 0.0f;

            if (this.eyeGazeTargetPoint.HasValue) {

                Vector3 abs_target_point = this.eyeGazeTargetPoint.Value;

                //
                // Calculate the target in "eye-apose" space, i.e., relatively to the eye (or their midpoint) when the character stands in A-Pose.
                // It means that the target_point must be transformed as if it positioned relatively to the character standing in the default A-Pose.

                //
                // Decomposing the eye rotation matrix
                // The current rotation C can be decomposed in initial rotation I and an additional delta D in global space.
                //   C = D * I
                // (That is different from decomposing in initial plus local transform C = I * D_loc)
                // Hence,
                //   D = C * Iinv
                // In order to transform the absolute target in the original global space before rotation, we need the inverse of D.
                // In general (AB)inv = Binv * Ainv
                // Hence:
                //  Dinv = I * Cinv
                //


                //
                // This will be splitted in two when the two eyes will be driven independently.

                Vector3 eyeMidPoint = new Vector3(
                    (leftEyePosition.x + rightEyePosition.x) / 2.0f,
                    (leftEyePosition.y + rightEyePosition.y) / 2.0f,
                    (leftEyePosition.z + rightEyePosition.z) / 2.0f
                );

    #if DEBUG_GEOM
                Debug.DrawLine(leftEyePosition, rightEyePosition, Color.blue, 0.1f, false);
    #endif

                Vector3 targetPointInvDelta = this.aPoseLeftEyeRotation * Quaternion.Inverse(leftEyeRotation) * abs_target_point;

                Vector3 eyeMidPointInvDelta = this.aPoseLeftEyeRotation * Quaternion.Inverse(leftEyeRotation) * eyeMidPoint;
                Vector3 targetPointEyeSpace = targetPointInvDelta - eyeMidPointInvDelta;


                // Calculate the targetPoint projected on the horizontal plane which passes through the eyesMidPoint
                float hproj_dist = targetPointEyeSpace.x * HORIZ_PLANE_NORMAL.x
                    + targetPointEyeSpace.y * HORIZ_PLANE_NORMAL.y
                    + targetPointEyeSpace.z * HORIZ_PLANE_NORMAL.z;
                Vector3 targetHplaneProjection = targetPointEyeSpace - (HORIZ_PLANE_NORMAL * hproj_dist);

    #if DEBUG_GEOM
                Debug.DrawLine(Vector3.zero, targetPointEyeSpace, Color.red);
                Debug.DrawLine(Vector3.zero, targetHplaneProjection, Utiliy.darker(Color.red));
    #endif


                //
                // Calculate the pitch: the vertical orientation of the eyes.

                // Calc the angle (in degrees) between the vector pointing to the target and its projection on the vertical plane
                float vertAngleBetween = Vector3.Angle(targetPointEyeSpace, targetHplaneProjection);
                newEyesPitch = Vector3.Dot(targetPointEyeSpace, HORIZ_PLANE_NORMAL) >= 0 ? vertAngleBetween : -vertAngleBetween;

                //
                // Calculate the yaw: the horizontal orientation of the eyes

                // Calc the angle (in degrees) between the forward vector the the target in eye-space.
                float horizAngleBetween = Vector3.Angle(this.aPoseFwdDirection, targetHplaneProjection);
                newEyesYaw = Vector3.Dot(targetHplaneProjection, this.aPoseRightDirection) <= 0 ? horizAngleBetween : -horizAngleBetween;

            }

            //
			// Increment the current eyes yaw/pitch according to eyes movement speed
			float deltaEyesPitch = newEyesPitch - this.eyesPitch;
			this.eyesPitch += LinearInc (deltaEyesPitch, this.eyesRotSpeed, dt);

			float deltaEyesYaw = newEyesYaw - this.eyesYaw;
			this.eyesYaw += LinearInc (deltaEyesYaw, this.eyesRotSpeed, dt);

#if DEBUG_GEOM
            {
                // Test, reproject the line of sight direction using the calculated angles
                // Vector3 rebuiltTarget = Quaternion.AngleAxis(-newEyesYaw, HORIZ_PLANE_NORMAL) * FORWARD;
                // WRONG!!! Vector3 rebuiltTarget = Quaternion.AngleAxis(-newEyesPitch, RIGHT) * Quaternion.AngleAxis(-newEyesYaw, HORIZ_PLANE_NORMAL) * FORWARD;
                //Vector3 rebuiltTarget = Quaternion.AngleAxis(-newEyesYaw, HORIZ_PLANE_NORMAL) * Quaternion.AngleAxis(-newEyesPitch, RIGHT) * FORWARD;
                Vector3 rebuiltTarget = Quaternion.AngleAxis(-newEyesYaw, HORIZ_PLANE_NORMAL) * Quaternion.AngleAxis(-newEyesPitch, this.aPoseRightDirection) * this.aPoseFwdDirection;
                Debug.DrawRay(Vector3.zero, rebuiltTarget, Color.green);
            }
#endif
            //
            // Activate the yaw/pitch rotations for the neck.
            if (enableNeckRotation) {

				float deltaPitch = 0.0f;
                if (this.eyesPitch > EYES_MAX_PITCH) {
                    deltaPitch = this.eyesPitch - EYES_MAX_PITCH;
                    this.eyesPitch = EYES_MAX_PITCH;
                } else if (this.eyesPitch < -EYES_MAX_PITCH) {
                    deltaPitch = this.eyesPitch + EYES_MAX_PITCH;
                    this.eyesPitch = -EYES_MAX_PITCH;
				}

				this.neckPitch += SmoothtInc (deltaPitch, neckRotTime, neckRotMaxSpeed, dt);
                this.neckPitch += SmoothtInc(-this.neckPitch, neckRotTime / neckRestTendencyStrenght, neckRotMaxSpeed * neckRestTendencyStrenght, dt) ;

                if (this.neckPitch > NECK_MAX_PITCH) {
                    this.neckPitch = NECK_MAX_PITCH ;
				} else if(neckPitch < NECK_MIN_PITCH) {
                    this.neckPitch = NECK_MIN_PITCH ;
				}

				// Debug.Log("Delta pitch/Neck pitch: "+deltaPitch+"\t"+neckPitch) ;


				float deltaYaw = 0.0f;
                if (this.eyesYaw >= EYES_MAX_YAW) {
                    deltaYaw = this.eyesYaw - EYES_MAX_YAW;
					this.eyesYaw = EYES_MAX_YAW;
                } else if (this.eyesYaw <= -EYES_MAX_YAW) {
                    deltaYaw = this.eyesYaw + EYES_MAX_YAW;
					this.eyesYaw = -EYES_MAX_YAW;
				}

				this.neckYaw += SmoothtInc(deltaYaw, neckRotTime, neckRotMaxSpeed, dt) ;
				this.neckYaw += SmoothtInc(-this.neckYaw, neckRotTime / neckRestTendencyStrenght, neckRotMaxSpeed * neckRestTendencyStrenght, dt) ;

				if (neckYaw > NECK_MAX_YAW) {
					this.neckYaw = NECK_MAX_YAW ;
				} else if(neckYaw < NECK_MIN_YAW) {
					this.neckYaw = NECK_MIN_YAW ;
				}

                // Debug.Log("Delta yaw/Neck yaw: "+deltaYaw+"\t"+neckYaw) ;

#if DEBUG_GEOM
                {
                    float totalYaw = this.neckYaw + this.eyesYaw;
                    float totalPitch = this.neckPitch + this.eyesPitch;
                    // Test, reproject the line of sight direction using the calculated angles
                    Vector3 rebuiltTarget = Quaternion.AngleAxis(-totalYaw, HORIZ_PLANE_NORMAL) * Quaternion.AngleAxis(-totalPitch, this.aPoseRightDirection) * this.aPoseFwdDirection;
                    Debug.DrawRay(Vector3.zero, rebuiltTarget, Color.white);
                }
#endif

                //
                // Computer the new quaternion for the Neck.

                // G = P * L --> P = G * L^-1
                Quaternion neckAPoseParentRotation = this.aPoseNeckRotation * Quaternion.Inverse(this.aPoseNeckLocalRotation);

                // I have to compute the quaternion representing the rotation of the two angles.
                // yaw * pitch * oroginal pose
                Quaternion neckRotQuat = Quaternion.AngleAxis(- this.neckYaw, HORIZ_PLANE_NORMAL) * Quaternion.AngleAxis(- this.neckPitch, this.aPoseRightDirection);
                // this rotation must be applied to a rotation describing the neck straight in the current charactr orientation.

                // Test left/right
                // Quaternion neckLocalRotQuat = Quaternion.Inverse(neckAPoseParentRotation) * Quaternion.AngleAxis(45, HORIZ_PLANE_NORMAL) * neckAPoseParentRotation;
                // Test up/down
                // Quaternion neckLocalRotQuat = Quaternion.Inverse(neckAPoseParentRotation) * Quaternion.AngleAxis(45, this.aPoseRightDirection) * neckAPoseParentRotation;

                Quaternion neckLocalRotQuat = Quaternion.Inverse(neckAPoseParentRotation) * neckRotQuat * neckAPoseParentRotation;

                //Debug.Log("aposenecklocalrot=" + this.aPoseNeckLocalRotation);
                inOutNeckRotation = neckLocalRotQuat * this.aPoseNeckLocalRotation;


			}


			//
			// Calculate the proportion from the pitch angle to BlendShape percentage
			// weightEyesPitch : 50% = pitch : 45deg
			float weightEyesPitch = 0.5f * this.eyesPitch / 45.0f;

			// Calculate the proportion from the yaw angle to the BlendShape percentage
			// weightEyesYaw : 50% = yaw : 52deg
			float weightEyesYaw = 0.5f * this.eyesYaw / 52.0f;

			//Debug.Log("weightEyesDown: " + weightEyesPitch + " weightEyesLeft: " + weightEyesYaw);

			//Blendshapes are registered int the DefaultExecutionOrder: EYES_RIGHT, EYES_LEFT, EYES_DOWN, EYES_UP
			if (weightEyesPitch >= 0.0f) {
				outBlendShapeWeights[3] = weightEyesPitch;
				outBlendShapeWeights[2] = 0.0f;
			} else {
				outBlendShapeWeights[2] = -weightEyesPitch;
				outBlendShapeWeights[3] = 0.0f;
			}

			if (weightEyesYaw >= 0.0f) {
				outBlendShapeWeights[0] = weightEyesYaw;
				outBlendShapeWeights[1] = 0.0f;
			} else {
				outBlendShapeWeights[1] = -weightEyesYaw;
				outBlendShapeWeights[0] = 0.0f;
			}

		} // end update

	}
}
