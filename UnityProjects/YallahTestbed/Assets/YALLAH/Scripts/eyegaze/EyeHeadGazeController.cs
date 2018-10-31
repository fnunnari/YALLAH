using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/*
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
public class EyeHeadGazeController : MonoBehaviour {

	public Transform leftEye;
	public Transform rightEye;
	public Transform neck;

	private EyeHeadGazeLogic logic;

    private GameObject found_object;
    //	[Tooltip("The speed or rotation (deg/sec) of the eyes during fixation.")]
    //	public float eyesRotSpeed;

    [Tooltip("Whether to enable or not the neck rotation.")]
    public bool enableNeckRotation = true;

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

	// The list of the IDs of the blendshapes required by the logic.
	private List<int> blendShapeIDs;

    // Buffer to get the result of the computation of the blendshape weights.
    private float[] eyeBlendShapes = new float[EyeHeadGazeLogic.get_viseme_count()];



	void Awake() {
		Assert.IsNotNull(leftEye);
		Assert.IsNotNull(rightEye);
		Assert.IsNotNull(neck);

		this.skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		Assert.IsNotNull (this.skinnedMeshRenderer);

		// Get the IDs of each of the blendshapes
		this.blendShapeIDs = new List<int> ();
		for(int i=0 ; i < EyeHeadGazeLogic.get_viseme_count() ; i++) {
			string bs_name = (string)(EyeHeadGazeLogic.VISEMES [i]);
			int bsID = this.skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(bs_name);
			Assert.AreNotEqual (bsID, -1);
			this.blendShapeIDs.Add (bsID);
		}

        //Debug.Log(gameObject.name);
        //Debug.Log(this.neck.rotation.ToString("F4"));
        //Debug.Log(this.neck.localRotation.ToString("F4"));

		// Initialize the animator
		this.logic = new EyeHeadGazeLogic(
            this.leftEye.position,
            this.rightEye.position,
            this.leftEye.rotation,
            this.rightEye.rotation,
            this.neck.rotation,
            this.neck.localRotation
        ) ;

        // Initialize local neck rot cache.
        this.lastNeckLocalRot = this.neck.localRotation;

	}
		

	//
	// PUBLIC API
	//

	public void SetEnableNeckRotation(bool enable) {
		this.enableNeckRotation = enable;
	}

	public bool IsNeckRotationEnabled() {
		return this.enableNeckRotation;
	}

	public void StopLooking() {
		this.logic.eyeGazeTargetPoint = null;
	}


	/** Starts moving eyes and head to watch a spcified position.
	 */
	public void LookAtPoint(Vector3 targetPoint){
        // Debug.Log ("Looking at " + targetPoint);
        found_object = null;
        this.logic.LookAtPoint(targetPoint);

	}


    /** Starts moving eyes and head to watch a spcified position.
     */
    public void LookAtPoint(float x, float y, float z)
    {
        this.LookAtPoint(new Vector3(x, y, z));
    }


	/** Look at the object with the given name.
	 *  If the object doesn't exist, nothing happens.
	 */
	public void LookAtObject(string target_obj){
		found_object = GameObject.Find (target_obj);
		if(found_object != null) {
            this.logic.LookAtPoint(found_object.transform.position);
		}
	}


    //
    // Behaviour Inherited Methods
    //

    /** This is used as cache of the last computation of the neck local rotation.
     * It is needed to bring back the neck to its last rotation if
     * some animators are altering its rotation.
     */
    private Quaternion lastNeckLocalRot;

	void LateUpdate() {
        // Debug.Log ("eye/head gaze update");

        if (found_object != null)
        {
            this.LookAtObject(found_object.transform.name);
        }
   
        //
        // Copy the preferences from the Unity panel to teh logic.
        this.logic.enableNeckRotation = this.enableNeckRotation;

        // Local variable, will be updated with the new rotation.
		//Quaternion neckRot = this.neck.rotation ;
        Quaternion neckRot = new Quaternion();
        // Debug.Log ("NeckRot Before: " + neckRot);
        //Debug.Log("NeckRot Before: " + this.neck.rotation);

        this.neck.localRotation = this.lastNeckLocalRot;

        //
        // Invoke the animator
		this.logic.UpdateEyesRotation (
			leftEye.position, rightEye.position,
			leftEye.rotation, rightEye.rotation,
			ref eyeBlendShapes,
			ref neckRot,
			Time.deltaTime);


		// Update the blendshapes
        //  Debug.Log ("Weights: " + eyeBlendShapes[0] +"\t"+ eyeBlendShapes[1]+"\t"+ eyeBlendShapes[2]+"\t"+ eyeBlendShapes[3]);
		for (int i=0 ; i<this.eyeBlendShapes.Length ; i++) {
			int bsID = this.blendShapeIDs[i] ;
			float w = (float)this.eyeBlendShapes[i];
			this.skinnedMeshRenderer.SetBlendShapeWeight(bsID, w * 100.0f);
		}
			
		// Update the neck rotation
		// Debug.Log ("NeckRot After: " + neckRot);
        if(this.IsNeckRotationEnabled()) {
            this.lastNeckLocalRot = neckRot;
            this.neck.localRotation = neckRot;
        }

	}

}
