#define USE_HAXE_LOGIC

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


using glm;


#if USE_HAXE_LOGIC
using haxe.root;
#else
using HaxeSpeedTest ;
#endif

/**
 * EyeGazeBehaviour controls the eye gaze of an avatar.
 * The eye-head-coordination is taken from:
 * https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4370616/
 * "Most of gaze shifts without head movements (90%) were within amplitudes of ±30° horizontally or ±12° vertically"
 * 
 * History:
 * - v1 This version works good without neck movement - FN 20180125
 * - v2 Works perfectly also with neck rotation - FN 20180126
 * - v3 cleaned up and added neck rotation limits - FN 20180126
 * - v4 Splitted the Controller from the core logic - FN 20180205
 * - v5 Using the Haxe generated Logic - FN
 * - v6 Fixed concurrence with animation playback. Moved updates to LateUpdate() - FN 20180405
 */
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class EyeHeadGazeController : MonoBehaviour
{

    public Transform leftEye;
    public Transform rightEye;
    public Transform neck;

    private EyeHeadGazeLogic logic;

    //	[Tooltip("The speed or rotation (deg/sec) of the eyes during fixation.")]
    //	public float eyesRotSpeed;
    //
    //	//[Tooltip("Whether to enable or not the neck rotation.")]
    //	public bool enableNeckRotation ;
    ////	{
    ////		get { return this.logic.enableNeckRotation;}
    ////		set { this.logic.enableNeckRotation = value;}
    ////	}
    //
    //	[Tooltip("The time (secs) the neck needs to re-align when the target is beyond eyes limits")]
    //	public float neckRotTime ;
    //
    //	/** The maximum speed (deg/sec) of rotation of the neck.
    //	 * Here: https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4616101/
    //	 * They consider slow/high rotation speed for the neck as 90 - 180 deg/sec
    //	 */
    //	[Tooltip("Maximum rotaion speed for the neck (deg/sec)")]
    //	public float neckRotMaxSpeed;
    //
    //	[Tooltip("How strongly the neck tends to reset ita rotation when the target is in the rotation range of the eyes.")]
    //	[Range(0.01f, 1.0f)]
    //	public float neckRestTendencyStrenght;


    // Reference to the Mesh renderer. Needed to pilot the BlendShapes in real-time.
    private SkinnedMeshRenderer skinnedMeshRenderer;

    // This is the absolute initial rotation of the neck when the character stands in its initial APose.
    private Quaternion aPoseNeckRotation;

    // The list of the IDs of the blendshapes required by the logic.
    private List<int> blendShapeIDs;


#if USE_HAXE_LOGIC
    private Quat toQuat(Quaternion q)
    {
        return new Quat(q.x, q.y, q.z, q.w);
    }

    private Quaternion toQuaternion(Quat q)
    {
        return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
    }

    private Vec3 toVec3(Vector3 v)
    {
        return new Vec3(v.x, v.y, v.z);
    }
#endif


    void Awake()
    {
        Assert.IsNotNull(leftEye);
        Assert.IsNotNull(rightEye);
        Assert.IsNotNull(neck);

        this.skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        Assert.IsNotNull(this.skinnedMeshRenderer);

        // Get the IDs of each of the blendshapes
        this.blendShapeIDs = new List<int>();
        for (int i = 0; i < EyeHeadGazeLogic.get_viseme_count(); i++)
        {
            string bs_name = (string)(EyeHeadGazeLogic.VISEMES[i]);
            int bsID = this.skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(bs_name);
            Assert.AreNotEqual(bsID, -1);
            this.blendShapeIDs.Add(bsID);
        }

        // This is the absolute initial rotation of the eye(s) (for the moment the left one) when the character stands in its initial APose.
        Quaternion aPoseLeftEyeRotation = this.leftEye.rotation;
        Quaternion aPoseRightEyeRotation = this.rightEye.rotation;
        // As above, but for the neck.
        //this.aPoseNeckLocalRotation = this.neck.localRotation;
        this.aPoseNeckRotation = this.neck.rotation;

        // Initializes the "current rotation" as the one in aPose.
        this.neckCurrentRotation = this.aPoseNeckRotation;

        // Set the reference axes for the character in its initial position
        EyeHeadGazeLogic.SetReferenceAxes(
#if USE_HAXE_LOGIC
            toVec3(-Vector3.forward),
            toVec3(Vector3.up),
            toVec3(-Vector3.right)
#else
			-Vector3.forward,
			Vector3.up,
			-Vector3.right
#endif
        );

        // Initialize the animator
        this.logic = new EyeHeadGazeLogic(
#if USE_HAXE_LOGIC
            toQuat(aPoseLeftEyeRotation),
            toQuat(aPoseRightEyeRotation),
            toQuat(aPoseNeckRotation));
#else
			aPoseLeftEyeRotation,
			aPoseRightEyeRotation,
			aPoseNeckRotation) ;
#endif


        this.logic.enableNeckRotation = true;

    }


    //
    // PUBLIC API
    //

    public void NeckRotation(bool enable)
    {
        this.logic.enableNeckRotation = enable;
    }

    public bool IsNeckRotationEnabled()
    {
        return this.logic.enableNeckRotation;
    }

    public void StopFixating()
    {
        Debug.Log("Nulling target point");
        this.logic.eyeGazeTargetPoint = null;
    }
    public void StopLooking()
    {
        this.logic.eyeGazeTargetPoint = null;
    }

    /** Brings beck neck and eyes at the default position. */
    //	public void ResetEyeGaze() {
    //		Debug.Log ("Resetting eye gaze");
    //		this.logic.ResetEyeGaze ();
    //
    //		foreach (int bsID in this.blendShapeIDs) {
    //			this.skinnedMeshRenderer.SetBlendShapeWeight(bsID, 0.0f);
    //		}
    //
    //		this.neck.localRotation = this.aPoseNeckLocalRotation;
    //	}


    /** Starts moving eyes and head to watch a spcified position.
	 *  Note: t looks for the current position of the object but doesn't follow the object*/
    public void LookAtPoint(Vector3 targetPoint)
    {
        found_object = null;
        //found_object.transform.position = targetPoint;
        // Debug.Log ("Looking at " + targetPoint);
#if USE_HAXE_LOGIC
        this.logic.LookAtPoint(toVec3(targetPoint));
#else
		this.logic.LookAtPoint(targetPoint);
#endif
    }

    private GameObject found_object;

    /** Look at the object with the given name.
	 *  If the object doesn't exist, nothing happens.
	 *  Note: t looks for the current position of the object but doesn't follow the object*/
    public void LookAtObject(string target_obj)
    {
        found_object = GameObject.Find(target_obj);
        Vector3 targetPoint = found_object.transform.position;
        if (found_object != null)
        {
        #if USE_HAXE_LOGIC
                    this.logic.LookAtPoint(toVec3(targetPoint));
        #else
		        this.logic.LookAtPoint(targetPoint);
        #endif
            //this.LookAtPoint(found_object.transform.position);
            //print(found_object.transform.position);
        }
    }


    //
    // Behaviour Inherited Methods
    //

    // Buffers to get the result of the computation of the underlieing logic.
#if USE_HAXE_LOGIC
    private double[] eyeBlendShapes = new double[EyeHeadGazeLogic.get_viseme_count()];
#else
	private float[] eyeBlendShapes = new float[EyeHeadGazeLogic.get_viseme_count()] ;
#endif

    // This is the absolute initial rotation of the neck when the character stands in its initial APose.
    private Quaternion neckCurrentRotation;


    void LateUpdate()
    {
        // Debug.Log ("eye/head gaze update");

        // Sometimes is not true because of numerical errors
        // Assert.AreEqual(leftEye.transform.rotation, rightEye.transform.rotation);

        // Set the neck rotation here, because as consequence it will update the absolute position of the eye-bones.
        this.neck.rotation = this.neckCurrentRotation;

#if USE_HAXE_LOGIC
        Quat neckRot = toQuat(this.neckCurrentRotation);
#else
		Quaternion neckRot = this.neckCurrentRotation ;
#endif

        if(found_object != null)
        {
            this.LookAtObject(found_object.transform.name);
        }

        // Debug.Log ("NeckRot Before: " + neckRot);

        // Invoke the animator
        this.logic.UpdateEyesRotation(
#if USE_HAXE_LOGIC
            toVec3(leftEye.position), toVec3(rightEye.position),
            toQuat(leftEye.rotation), toQuat(rightEye.rotation),
            eyeBlendShapes,
            neckRot,
#else
			leftEye.position, rightEye.position,
			leftEye.rotation, rightEye.rotation,
			ref eyeBlendShapes,
			ref neckRot,
#endif
            Time.deltaTime);

        //Debug.Log ("Weights: " + eyeBlendShapes[0] +"\t"+ eyeBlendShapes[1]+"\t"+ eyeBlendShapes[2]+"\t"+ eyeBlendShapes[3]);

        // Update the blendshapes
        for (int i = 0; i < this.eyeBlendShapes.Length; i++)
        {
            int bsID = this.blendShapeIDs[i];
            float w = (float)this.eyeBlendShapes[i];
            this.skinnedMeshRenderer.SetBlendShapeWeight(bsID, w * 100.0f);
        }

        // Update the neck rotation
        // Debug.Log ("NeckRot After: " + neckRot);

#if USE_HAXE_LOGIC
        this.neck.rotation = toQuaternion(neckRot);
        this.neckCurrentRotation = toQuaternion(neckRot);
#else
		this.neck.rotation = neckRot;
		this.neckCurrentRotation = neckRot ;
#endif

    }

}
