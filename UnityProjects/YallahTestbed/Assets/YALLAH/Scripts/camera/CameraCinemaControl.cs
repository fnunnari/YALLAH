/** Class for adjusting the camera's position according to the intended Shot
 * Original Implementation: Kiarash Tamaddon, September 2017
 *
 * How the calculations for the position of the camera are made:
 * The Shot is calculated according to the height of different part of the body
 * Between the standard shots, we chose five standard shots which are "FULL_SHOT", "MEDIUM_SHOT", "MEDIUM_CLOSE_UP", "FULL_CLOSE_UP" and "EXTREME_CLOSE_UP".
 * The user can switch the shots on the GUI.
 * The script repositions the camera for each shot. 
 * For repositioning the camera, We need to calculate distance of the camera to the character and the height of camera. 
 * The camera is always horizontally aligned. 
 * 
 * As you may see in the picture, height of the frame is a sum of the part of the body that is in the frame, plus the bottom and top margins of the frame:
 * `Frame_Height = (Bottom_Margin + Top_Margin) * Frame_Height  + Height`
 * Distance of the camera to the charachter is calculated using tangent of the camera's FOV:
 * `Distance = Frame_Height / Tan(Camera_FOV * 0.5)`
 * As the camera is horizontally aligned, it should be positioned symmetrically in the middle of the frame. Thus height of camera is the average of heights of top and bottom of the frame:
 * `Camera_Height = (height of the Frame_Bottom + height of the Frame_Top) * 0.5`
 * as a result, we have:
 *`Camera_Height = [(Body_Top + Top_Margin * Height) + (Body_Button - Button_Margin * Height)] / 2`
 */

/** How to use: 
 * 1. Add this code as component of the active camera.
 * 2. In the GUI inspector (as a component of the active camera) set the Target Character.
 * 
 * Limitations:
 * - The camera rotation must be 0,0,0
 * 
 * Known Bugs:
 * - Doesn't work with wide camera FOV angles (e.g., 60deg leads to NaNs). Tested with 20deg.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCinemaControl : MonoBehaviour {

	[Tooltip("Reference to the Avatar to frame")]
	public GameObject targetCharacter;

	public enum Shot { FULL_SHOT=0 , MEDIUM_SHOT, MEDIUM_CLOSE_UP, FULL_CLOSE_UP, EXTREME_CLOSE_UP };


	[Header(" ")]  // a space in the inspector pannel
	[Tooltip("The shot currently pursued by the camera.")]
	public Shot activeShot;

	[Header(" ")]  // a space in the inspector pannel
	[Tooltip("The 'lowest' shot used when cacling through shots via GoToNext/PreviousShot() API.")]
	public Shot minShot ;
	[Tooltip("The 'upper' shot used when cacling through shots via GoToNext/PreviousShot() API.")]
	public Shot maxShot;

	[Header(" ")] // a space in the inspector pannel
	[Tooltip("Fine tunes the camera distance +/- %")]
	[Range(-90, 600)]
	public float distanceFineTune = 0;


	/** Maps a shot type to a time (in secs) needed to travel about 80% of the distance to the target.
        Needed because some shots lead to the camera being very close to the body, hence faster movements are needed. */
	private static Dictionary<Shot, float> shotTimeLookupTable = new Dictionary<Shot, float>
	{
		{Shot.FULL_SHOT, 0.6f},
		{Shot.MEDIUM_SHOT,  0.5f},
		{Shot.MEDIUM_CLOSE_UP,  0.4f},
		{Shot.FULL_CLOSE_UP, 0.3f},
		{Shot.EXTREME_CLOSE_UP, 0.1f}
	};


	/** The list of bones needed to compute the distances for the various shots. */
	private enum TargetBonesEnum { head_top, toes_R, spine01, breast_R, clavicle_R, eye_R, calf_R, neck, root }

	/** The reference to the GameObjects representing the targetbones. */
	private GameObject[] _targetBoneObjects;


	/** Caches the distance of the camera from the character for each of the shots,
	 * thus avoiding heavy recomputations on shot switches. */
	private float[] _initialCameraDistances = new float[System.Enum.GetNames(typeof(Shot)).Length];


	/** The camera position we want to reach. Used in Update() */
	private Vector3 targetCameraPosition = new Vector3 (0,0,0);

	/** The camera inclination (X angle) we want to reach. Used in Update() */
	private float targetCameraAngle = 0.0f;


	/** 
	 * Support structure describing the parameters of a Shot.
	 **/
	private struct CameraFramingInfo
	{
		public float bottom_margin;  //ratio of the bottom margin to the full height of the picture
		public float top_margin;  //ratio of the top margin to the full height of the picture
		public Vector3 Bottom_body;  //height of lowest part of the character's body which is in the Shot
		public Vector3 Top_body;  //height of highest part of the character's body which is in the Shot		
		public float camera_x;  //if the camera is to the left or right

		public float heightCoefficient; // TODO description

		public CameraFramingInfo(float bottom_margin, float top_margin, Vector3 Bottom_body, Vector3 Top_body, float camera_x)
		{
			this.bottom_margin = bottom_margin;
			this.top_margin = top_margin;
			this.Bottom_body = Bottom_body;
			this.Top_body = Top_body;
			this.camera_x = camera_x;

			this.heightCoefficient = (this.Top_body.y - this.Bottom_body.y) / (1 - (this.top_margin + this.bottom_margin));

		}
	}


	void Start (){
		// Fills an array with the reference to the GameObjects of the needed bones,
		// thus avoiding expensive searches at run-time
		_targetBoneObjects = new GameObject[System.Enum.GetNames(typeof(TargetBonesEnum)).Length];
		_childFinderRecursive(targetCharacter);

		// Initialize/cache all camera distances.
		_initializeDistances();

		// Initialize camera position and inclination.
		_computeCameraParams(this.activeShot, ref targetCameraPosition, ref targetCameraAngle);
		transform.position = targetCameraPosition;
		transform.rotation = Quaternion.Euler(new Vector3(targetCameraAngle, 0, 0));
	}


	/** Used by the SmoothDamp to compute the camera position. */
	private Vector3 _cameraVelocity = Vector3.zero;

	/** Used by the smooth damp of the camera inclination. */
	private float _cameraAngleVelocity = 0;

	/**
	 * In this update cycle, the position and rotation of the camera according to the currently selected shot.
	 **/
	void Update(){
		// Compute new target position and inclination
		_computeCameraParams (this.activeShot, ref this.targetCameraPosition, ref this.targetCameraAngle);

		float smooth_time = shotTimeLookupTable[this.activeShot];

		// damp to the target position
		transform.position = Vector3.SmoothDamp(transform.position, this.targetCameraPosition, ref this._cameraVelocity, smooth_time, maxSpeed: 2.0f, deltaTime: Time.fixedDeltaTime);

		// damp to the target camera inclination
		float newXangle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.x, this.targetCameraAngle, ref this._cameraAngleVelocity, smooth_time * 2, maxSpeed: 45.0f, deltaTime: Time.fixedDeltaTime);
		transform.rotation = Quaternion.Euler(new Vector3(newXangle, 0, 0)) ;
	}


	/**
	 * Support method to initialize a vector with the distances between the camera and the character associated with each Shot type.
	 * Called during initialization.
	 * Prevents heavy computations at runtime.
	 **/
	private void _initializeDistances()
    {
		for (int i = 0; i < System.Enum.GetNames(typeof(Shot)).Length; i++)
		{
			Shot shot = (Shot)System.Enum.ToObject(typeof(Shot), i);

			CameraFramingInfo framingInfo = _newFramingInfo(shot);


			//cam_frame_calc(shot, true, ref targetCameraPosition, ref currentCameraAngle);

			//calculating the initial camera distance: 
			//the conditions:
			//1. We keep height of the camera to the height of the right eye
			//2. For each shot we set the height of the lowest and highest point of the camera frame in the plane of the character
			//3. Now knowing the camera FOV, we can calculate the distance of camera and its angle for each frame
			//
			//the parameters are as follows. It is a matter of solving two triangles to have the position and orientation of the camera:
			//	h1: distance between top of the frame point and the eye
			//	h2: distance between the bottom of the frame and the eye
			//	bCoeeficient & bCoeeficient: coefficients of the quadratic equation 
			//
			//
			//
			//							top of the frame on the char's plane
			//				          * . 
			//					   *	.
			// 	                *	    .
			//	            *			h1
			// 	         *				.
			//	      * )alpha2			.
			//Camera..............d......height of the char's right eye
			//		* )alpha1			.
			//		  *					.
			//		    *				.
			//			  *				.
			//				*			h2
			//				  *			.
			//					*		.
			//					  *		.
			//						*	.
			//						  * .
			//						    bottom of of the frame on the char's plane
			//
			// alpha1 + alpha2 = FOV
			//tg(alpha1 + alpha2) = (h1/d + h2/d) / (a + (h1*h2/d^2) = tg(FOV)
			//d^2 - [(h1 + h2) / tg(FOV) * d] + [h1 * h2] = 0

			float frameTop = (framingInfo.Top_body.y + framingInfo.top_margin * framingInfo.heightCoefficient);
			float camera_height_ = _targetBoneObjects[(int)TargetBonesEnum.eye_R].transform.position.y;
			float h1 = frameTop - camera_height_;
			float frameBottom = (framingInfo.Bottom_body.y + framingInfo.bottom_margin * framingInfo.heightCoefficient);
			float h2 = camera_height_ - frameBottom;
			float bCoeeficient = -1 * (h1 + h2) / (Mathf.Tan(Camera.main.fieldOfView * Mathf.PI / 180));
			float cCoefficient = h1 * h2;

			_initialCameraDistances[(int)shot] = (float)(-0.5 * (-bCoeeficient + Mathf.Sqrt(bCoeeficient * bCoeeficient - 4 * cCoefficient)));

		}


	}


	/** 
	 * Instantiate a new CameraFramingInfo according to the current position of the bones.
	 * Uses the cached _targetBoneObjects.
	 **/
	private CameraFramingInfo _newFramingInfo(Shot current_shot)
    {
		CameraFramingInfo outInfo;

		Vector3 head_top = _targetBoneObjects[(int)TargetBonesEnum.head_top].transform.position;
		switch (current_shot)
		{
			case Shot.FULL_SHOT:
				outInfo = new CameraFramingInfo(1f / 12f, 1f / 12f, _targetBoneObjects[(int)TargetBonesEnum.toes_R].transform.position, head_top, _targetBoneObjects[(int)TargetBonesEnum.root].transform.position.x);
				break;

			case Shot.MEDIUM_SHOT:
				Vector3 lowBody = (_targetBoneObjects[(int)TargetBonesEnum.calf_R].transform.position + _targetBoneObjects[(int)TargetBonesEnum.spine01].transform.position) / 2;
				outInfo = new CameraFramingInfo(0, 1f / 12f, lowBody, head_top, _targetBoneObjects[(int)TargetBonesEnum.spine01].transform.position.x);
				break;

			case Shot.MEDIUM_CLOSE_UP:
				lowBody = _targetBoneObjects[(int)TargetBonesEnum.breast_R].transform.position;
				Vector3 offset = new Vector3(0, 0.05f, 0);
				outInfo = new CameraFramingInfo(0, 1f / 12f, lowBody + offset, head_top + offset / 2, _targetBoneObjects[(int)TargetBonesEnum.spine01].transform.position.x);
				break;

			case Shot.FULL_CLOSE_UP:
				lowBody = _targetBoneObjects[(int)TargetBonesEnum.neck].transform.position;
				outInfo = new CameraFramingInfo(0, 1f / 12f, lowBody, head_top, _targetBoneObjects[(int)TargetBonesEnum.neck].transform.position.x);
				break;

			case Shot.EXTREME_CLOSE_UP:
				Vector3 dist = _targetBoneObjects[(int)TargetBonesEnum.head_top].transform.position - _targetBoneObjects[(int)TargetBonesEnum.eye_R].transform.position;
				outInfo = new CameraFramingInfo(0, 0, _targetBoneObjects[(int)TargetBonesEnum.eye_R].transform.position - dist / 4, _targetBoneObjects[(int)TargetBonesEnum.eye_R].transform.position + dist / 4, _targetBoneObjects[(int)TargetBonesEnum.eye_R].transform.position.x);
				break;

			default:
				outInfo = new CameraFramingInfo(1f / 12f, 1f / 12f, _targetBoneObjects[(int)TargetBonesEnum.toes_R].transform.position, head_top, 0);
				Debug.LogError("Invalid value for camera shot: " + current_shot);
				break;
		}

		return outInfo;
	}


	/** 
	 * Given the shot type, this method computes the desired camera position and inclination.
	 * It reads on-the-fly the position of the reference bones. Hence, for the same shot, the results will be different if the character is moved or animated.
	 **/
    private void _computeCameraParams(Shot current_shot, ref Vector3 newTargetPosition, ref float newAngle){

		CameraFramingInfo framing_info = _newFramingInfo(current_shot);

		//
		// Compute the camera position, according to the current position of the eye
		float cameraZ = ((framing_info.Top_body.z + framing_info.Bottom_body.z) / 2 + _initialCameraDistances [(int)current_shot]) * (1 + distanceFineTune / 100);
		// set height of camera to right eye's height 
		float camera_height = _targetBoneObjects [(int)TargetBonesEnum.eye_R].transform.position.y;

		newTargetPosition = new Vector3(framing_info.camera_x, camera_height, cameraZ);

		//
		// Calculating the inclination of the camera (rotation around the x-axis)
		float target_frame_top = (framing_info.Top_body.y + framing_info.top_margin * framing_info.heightCoefficient); //expected highest point that you can see in the frame on the vertical surface passing the character

		newAngle = (float)((Mathf.Atan(((target_frame_top - transform.position.y) / (transform.position.z - _targetBoneObjects[(int)TargetBonesEnum.eye_R].transform.position.z))) * 180 / Mathf.PI) + (Camera.main.fieldOfView / 2));

	}


	/**
	 * Using the enumeration of the target bones,
	 * fills the _targetBoneObjects with the references to the corresponding GameObjects.
	 * Such objects are recursively searched among the children of the given parentObject.
	 * The order between bone nemae and references is preserved.
	 **/
	private void _childFinderRecursive(GameObject parentGameObject){
		for (int child_index = 0; child_index <= System.Enum.GetNames(typeof(TargetBonesEnum)).Length; child_index++)
		{
			if (parentGameObject.name == System.Enum.GetName(typeof(TargetBonesEnum), child_index))
			{
				_targetBoneObjects[child_index] = parentGameObject;
			}
		}

		foreach (Transform childTransform in parentGameObject.transform)
		{
			var childGameObject = childTransform.gameObject;
			_childFinderRecursive(childGameObject);  // Recursion!
		}
	}


	/**
	 * Public API: Go to the previous shot in the list. Used by GUI controls.
	 **/
	public void GoToPreviousShot(){
		int current_shot_idx = (int)this.activeShot ;
		int min_shot_idx = (int)this.minShot;
		int max_shot_idx = (int)this.maxShot;
		current_shot_idx -= 1;
		if (current_shot_idx < min_shot_idx) current_shot_idx = max_shot_idx;
		this.activeShot = (Shot)System.Enum.ToObject(typeof(Shot), current_shot_idx);
	}

	/**
	 * Public API: Go to the next shot in the list. Used by GUI controls.
	 **/
	public void GoToNextShot(){
		int current_shot_idx = (int)this.activeShot ;
		int min_shot_idx = (int)this.minShot;
		int max_shot_idx = (int)this.maxShot;
		current_shot_idx += 1;
		if (current_shot_idx > max_shot_idx) current_shot_idx = min_shot_idx;
		this.activeShot = (Shot)System.Enum.ToObject(typeof(Shot), current_shot_idx);
	}

	/**
	 * Public API: return the current shot as string.
	 **/
	public string GetCurrentShot () {
		int current_shot_idx = (int)this.activeShot ;
		string text = "" + System.Enum.GetNames (typeof(Shot)) [current_shot_idx];
		return text;
	}


	/**
	 * Public API: set the current shot as string.
	 **/
	public void SetCurrentShot(string shot_name){
		this.activeShot = (Shot)System.Enum.Parse(typeof(Shot), shot_name);
	}

	/**
	 * Public API: set the current shot fine tuning.
	 **/
	public void SetShotFineTunePercentage(float percent){
		distanceFineTune = percent;
	}

	/**
	 * Public API: get the current shot fine tuning.
	 **/
	public float GetShotFineTunePercentage(){
		return distanceFineTune;
	}


	/**
	 * Public API: returns the list of available shots as array of strings.
	 **/
	public string[] ListCameraShots() {
		List<string> out_list = new List<string>();

		for(int i = 0; i< System.Enum.GetNames (typeof(Shot)).Length ; i++) 
		{
			out_list.Add("" + System.Enum.GetNames (typeof(Shot)) [i]) ;
		}
		return out_list.ToArray() ;
	}
}
