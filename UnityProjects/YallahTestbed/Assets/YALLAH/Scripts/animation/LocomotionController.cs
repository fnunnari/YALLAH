using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionController : MonoBehaviour {

    [Tooltip("Where the avatar wants to walk to.")]
	public Vector3 targetPosition = new Vector3(0,0,0);

    [Tooltip("Force at each frame the Y coordinate to 0. Useful if you have animations drifting on the vertical axis.")]
	public bool forceZeroY = true;

	// If the distance between the avatar and the target goes below this value, the fwd factor will decrease to 0.
    [Tooltip("If the distance between the avatar and the target goes below this value, the avatar will stop walking.")]
	public float distanceThreshold = 0.5f ;

	// If the angle between the avatar fwd vector and the target goes below this value, the rot factor will decrease to 0.
    [Tooltip("If the angle between the avatar fwd vector and the target goes below this value, the avatar will stop rotating.")]
	public float rotationThresholdDegs = 5.0f;

    // The actual threshold used to tart/stop the rotation.
    // When going below the user-define rot threshold, this threshold is set to a higher value.
    // If the highr value is reached, this histeresis threshold will be again set to the user-defined.
    // This avoids instabilities at the threshold level.
    // See: https://en.wikipedia.org/wiki/Hysteresis#Control_systems
    private float rotHistheresiThresholdDegs;


	private static float rotDampMaxSpeed = 5.0f;
	private static float fwdDampMaxSpeed = 5.0f;

	// Reference to the animator, on which we will set the value of the parameters and the IK info.
	private Animator anim ;

	// The current rotation factor [-1,1]
	private float rotVal = 0.0f;
	// velocity for the smooth damp
	private float rotValVelocity = 0.0f;


	// the current fwd factor [0,1]
	private float fwdVal = 0.0f;
	// velocity for smooth damp
	private float fwdValVelocity = 0.0f;
    // The layer containing the locomotion state machine.
	private int locomotionLayerIdx = -1 ;


	#if UNITY_EDITOR
	[Header("Test:")]
    [Tooltip("Orders the character to walk to the Target Position")]
	public bool forceStart ;
    public bool forceStop ;
	#endif



	// Use this for initialization
	void Start () {
		// Take the reference to the animator on the same GameObject
		this.anim = GetComponent<Animator> ();

		this.locomotionLayerIdx = this.anim.GetLayerIndex ("Locomotion Layer");
		Debug.Assert (this.locomotionLayerIdx != -1);

        this.rotHistheresiThresholdDegs = this.rotationThresholdDegs;

		/*
        //
        // Override the animations, so that we can be able to set the animation clips at run-time.

        // I needed to move them in the global animation controller, where there is already an Animator Override configuration.
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(this.anim.runtimeAnimatorController);
		this.anim.runtimeAnimatorController = animatorOverrideController;
		animatorOverrideController ["DUMMY_WALK_CYCLE_ANIMATION"] = this.walkCycleAnimationClip;
		animatorOverrideController ["DUMMY_TURN_RIGHT_ANIMATION"] = this.turnRightAnimationClip;
		animatorOverrideController ["DUMMY_TURN_LEFT_ANIMATION"] = this.turnLeftAnimationClip;
		*/

	}


	public void WalkTo (Vector3 target_position) {
		this.targetPosition = target_position;
		this.anim.SetTrigger ("locomotion_start");
	}


    public bool IsWalking() {
        AnimatorStateInfo state_info = this.anim.GetCurrentAnimatorStateInfo(this.locomotionLayerIdx) ;
        return state_info.IsName ("WalkBlendTree");
    }

    public void StopWalking() {
        if(IsWalking()) {
            this.anim.SetTrigger("locomotion_stop");
        }        
    }

	// Update is called once per frame
	void Update () {

		#if UNITY_EDITOR
		if (this.forceStart) {
			this.WalkTo (this.targetPosition);
			this.forceStart = false;
		}
        if(this.forceStop) {
            this.StopWalking();
            this.forceStop = false;
        }
		#endif


        if (! this.IsWalking()) {
			this.anim.ResetTrigger ("locomotion_stop");
			this.fwdVal = 0;
            return;                         // <-- BEWARE: Jumps out!!!
		}

		//
		//
		Vector3 current_position = this.gameObject.transform.position;
		// Animation accumulates a vertical offset that we ahve to force at 0.
		if (this.forceZeroY) {
			current_position.y = 0.0f;
			gameObject.transform.position = current_position;
		}

		Vector3 current_fwd_vector = (this.gameObject.transform.rotation * Vector3.forward).normalized;

		Vector3 vec_to_target = (targetPosition - current_position).normalized;
		float distance_to_target = (targetPosition - current_position).magnitude;

        // The dot product is 1 when the vectors are aligned, 0 when at 90 degrees, -1 when opposites.
		float dot = Vector3.Dot (current_fwd_vector, vec_to_target);
        // We use the vertical (y) coordinate of the cross product to understand weather to go left or right.
		Vector3 cross = Vector3.Cross (current_fwd_vector, vec_to_target);
		//Debug.Log ("dot=" + dot + "\tcross=" + cross);

		//
		// ROTATION
		float new_rot_val = 0.0f;
		bool rot_reached = false;

		// calc the dot product value equivalent to the threshold in degrees.
        // Essentially it maps the [0,90] degrees range to [1,0].
		// float rot_thr_dot_val = Mathf.Cos (Mathf.Deg2Rad * this.rotationThresholdDegs);
        float rot_thr_dot_val = Mathf.Cos (Mathf.Deg2Rad * this.rotHistheresiThresholdDegs);
        // If the dot product if less than the threshold, we need to rotate and re-align (which would bring the dor towards 1.0)
		if (dot < rot_thr_dot_val) {
            // If we enter the histererirs zone, lower the threshold so that we go down to the lower value.
            this.rotHistheresiThresholdDegs = this.rotationThresholdDegs;

			new_rot_val = 1.0f;

			// test if the rotation has to be done left or right
			if (cross.y < 0)
				new_rot_val *= -1.0f;
		} else {
			rot_reached = true;
            // If we reach the lower threshold, set the new limit to a higher value, to avoid jumps.
            this.rotHistheresiThresholdDegs = this.rotationThresholdDegs * 2f;
			// Debug.Log ("ROT Reached!");
		}
        // Debug.Log("HistThrsdegs=" + this.rotHistheresiThresholdDegs);

		this.rotVal = Mathf.SmoothDamp (this.rotVal, new_rot_val, ref this.rotValVelocity, Time.deltaTime, LocomotionController.rotDampMaxSpeed);
		//this.rotVal = new_rot_val;

		//
		// WALK
		float new_fwd_val = 0.0f;
		bool distance_reached = false;

		if (distance_to_target > this.distanceThreshold) {
			// lower the threshold so we can start walking even before the rotation stops
			if (dot >= rot_thr_dot_val * 0.9f)
				new_fwd_val = dot; //1.0f;
		} else {
			distance_reached = true;
			// Debug.Log ("Distance Reached!");
			//new_fwd_val = 1.0f;
		}

		//this.fwdVal = Mathf.SmoothDamp (this.fwdVal, new_fwd_val, ref this.fwdValVelocity, Time.deltaTime, LocomotionController.fwdDampMaxSpeed);
		this.fwdVal = Mathf.SmoothDamp (this.fwdVal, new_fwd_val, ref this.fwdValVelocity, 0.2f, LocomotionController.fwdDampMaxSpeed);
		//this.fwdVal = new_fwd_val ;

		//
		// Values to the animator
		// Control left/right rotation
		this.anim.SetFloat ("rot_factor", this.rotVal);
		// Control walk/run
		this.anim.SetFloat ("fwd_factor", this.fwdVal);


		if(distance_reached) {
			// Debug.Log ("Triger Stop");
			this.anim.SetTrigger ("locomotion_stop");
		}
	}

}
