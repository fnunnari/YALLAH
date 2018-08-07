
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectInputLocomotionController : MonoBehaviour {

	// public fields to visualize the value of the Joystick/Keyboard input

	#if UNITY_EDITOR
	[Header("Debug:")]
	public float vert_val_debug ;
	public float horiz_val_debug;
	#endif

	// Reference to the animator, on wwhich we will set the value of the parameters and the IK info.
	private Animator anim ;

	// Use this for initialization
	void Start () {
		// Take the reference to the animator on the same GameObject
		this.anim = GetComponent<Animator> ();

        this.anim.SetTrigger("locomotion_start");
	}

	// Update is called once per frame
	void Update () {

		// Control walk/run
		float vert_val = Input.GetAxis ("Vertical");
		anim.SetFloat ("fwd_factor", vert_val);

		// Control left/right rotation
		float horiz_val = Input.GetAxis ("Horizontal");
		anim.SetFloat ("rot_factor", horiz_val);

		#if UNITY_EDITOR
		this.vert_val_debug = vert_val;
		this.horiz_val_debug = horiz_val;
		#endif


		//		// Trigger the waving animation
		//		bool has_to_wave = Input.GetButtonDown ("Submit");
		//		if (has_to_wave) {
		//			anim.SetTrigger ("say_hello");
		//		}
	}

	//	// Invoked at each frame only by layers whose IK Pass is checked.
	//	void OnAnimatorIK(int layerIndex) {
	//
	//		// Debug.Log ("ik on layer " + layerIndex);
	//		if (layerIndex == 1) {
	//			Vector3 ik_target_pos = this.ik_target.transform.position;
	//			anim.SetIKPosition (AvatarIKGoal.LeftHand, ik_target_pos);
	//
	//
	//			float delta_ik_influence = this.ikInfluenceSpeed * Time.deltaTime;
	//			if(Input.GetKey(KeyCode.LeftShift)) {
	//				this.ikInfluence += delta_ik_influence ;
	//				if(this.ikInfluence>1.0f) this.ikInfluence = 1.0f ;
	//			} else {
	//				this.ikInfluence -= delta_ik_influence ;
	//				if(this.ikInfluence<0.0f) this.ikInfluence = 0.0f ;
	//			}
	//
	//			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, this.ikInfluence);
	//			// Example forcing the ikInfluence Touch as
	//			// anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.5f);
	//		}
	//
	//	}

}
