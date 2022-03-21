using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using System.Runtime.InteropServices;  // Provides DllImport
using System.Linq;

public class NativeAPIBridge : MonoBehaviour {

	public GameObject avatarMesh;
	public GameObject mainCamera;


	// The references to all the controllers we need.
	private static AnimationController _animationController;
	private static MaryTTSController _maryTTSController;
	private static FacialExpressionsController _facialExpressionController ;
	//private static EyeGazeController _eyeGazeController ;
	private static EyeHeadGazeController _eyeGazeController ;
	private static CameraCinemaControl _cameraController;


	//
	// Here we list all of the function types we need
	// These must match the native C types. Hence, no bools or other complex types are supported.
	delegate void fnType_V();
	delegate void fnType_Vi(int i);
	delegate void fnType_Vf(float f);
	delegate void fnType_Vfff(float f1, float f2, float f3);
	delegate void fnType_Vs(string s);
	delegate int fnType_I();
	delegate int fnType_Ii(int i);
	delegate float fnType_F();
	delegate string fnType_S() ;
	delegate string fnType_Ss(string s) ;

	void Awake () {

		// Execute only if we are running on WebGL
		if (Application.platform != RuntimePlatform.WebGLPlayer) {
			return;
		}

		Assert.IsNotNull(this.avatarMesh);
		Assert.IsNotNull(this.mainCamera);

		//
		// Get the reference to the object's components.
		_animationController = this.avatarMesh.GetComponentInParent<AnimationController>() ;
		Assert.IsNotNull (_animationController) ;
		_maryTTSController = this.avatarMesh.GetComponent<MaryTTSController> ();
		Assert.IsNotNull (_maryTTSController);
		_facialExpressionController = this.avatarMesh.GetComponent<FacialExpressionsController> ();
		Assert.IsNotNull (_facialExpressionController);
		_eyeGazeController = this.avatarMesh.GetComponent<EyeHeadGazeController> ();
		Assert.IsNotNull (_eyeGazeController);

		//New?
		_cameraController = this.mainCamera.GetComponent<CameraCinemaControl> ();
		Assert.IsNotNull (_cameraController);

		//
		// Set the callbacks
		Debug.Log("Setting anim callbacks");
		set_anim_callbacks(napi_PlayAnimationClip, napi_IsAnimationClipPlaying, napi_GetPlayingAnimationClip,
			napi_ListAvailableAnimationClips, napi_EnableAmbientAnimation, napi_DisableAmbientAnimation);

		Debug.Log("Setting MaryTTS callbacks");
		set_marytts_callbacks (napi_MaryTTSspeak, napi_IsMaryTTSspeaking);

		Debug.Log("Setting Facial Expression callbacks");
		set_facial_expression_callbacks (napi_SetCurrentFacialExpression, napi_GetCurrentFacialExpression,
			napi_ClearFacialExpression,	napi_ListFacialExpressions,
			napi_SetExpressionTransitionTime, napi_GetExpressionTransitionTime);

		Debug.Log("Setting Eye Gaze callbacks");
		set_eyegaze_callbacks (napi_LookAtPoint, napi_LookAtObject);

		Debug.Log("Setting Camera Movement callbacks");
		set_camera_callbacks (napi_GoToNextShot, napi_GoToPreviousShot,
			napi_GetCurrentShot, napi_SetCurrentShot,
			napi_SetShotFineTunePercentage, napi_GetShotFineTunePercentage,
			napi_ListCameraShots);

		Debug.Log("Callbacks all set");
	}

	//
	// UTILITY FUNCTIONS
	//
	private static string StringArrayToJSON(string[] lst ) {
		string[] quoted_lst = new string[lst.Length];
		for (int i = 0; i < lst.Length; i++) {
			quoted_lst[i] = "\"" + lst [i] + "\"";
		}
		string joined_quoted_names = string.Join (",", quoted_lst);
		return "[" + joined_quoted_names + "]";
	}


	//
	// AnimationController
	//

	//	void PlayAnimationClip(string)
	//	int IsAnimationClipPlaying()
	//	string GetPlayingAnimationClip()
	//	string[] ListAvailableAnimationClips
	//	void EnableAmbientAnimation()
	//	void DisableAmbientAnimation()

	[MonoPInvokeCallback (typeof (fnType_Vs))]
	private static void napi_PlayAnimationClip(string clip_name) {
		_animationController.PlayAnimationClip(clip_name);
	}

	[MonoPInvokeCallback (typeof (fnType_I))]
	private static int napi_IsAnimationClipPlaying() {
		return _animationController.IsAnimationClipPlaying() ? 1 : 0;
	}

	[MonoPInvokeCallback (typeof (fnType_S))]
	private static string napi_GetPlayingAnimationClip() {
		return _animationController.GetPlayingAnimationClipName ();
	}

	[MonoPInvokeCallback (typeof (fnType_S))]
	private static string napi_ListAvailableAnimationClips() {

		return StringArrayToJSON (_animationController.ListAnimationClips ());

//		string[] clip_names = _animationController.ListAnimationClips ();
//		string[] quoted_clip_names = new string[clip_names.Length];
//		for (int i = 0; i < clip_names.Length; i++) {
//			quoted_clip_names[i] = "\"" + clip_names [i] + "\"";
//		}
//		string joined_quoted_names = string.Join (",", quoted_clip_names);
//		return "[" + joined_quoted_names + "]";

	}

	[MonoPInvokeCallback (typeof (fnType_V))]
	private static void napi_EnableAmbientAnimation() {
		_animationController.EnableAmbientAnimation();
	}

	[MonoPInvokeCallback (typeof (fnType_V))]
	private static void napi_DisableAmbientAnimation() {
		_animationController.DisableAmbientAnimation();
	}

	[DllImport("__Internal")]
	private static extern void set_anim_callbacks(
		fnType_Vs play_fn,
		fnType_I is_playing_fn,
		fnType_S get_playing_fn,
		fnType_S list_fn,
		fnType_V enable_ambient_fn,
		fnType_V disable_ambient_fn
	);



	//
	// MaryTTSController
	//

	//	void  MaryTTSspeak(string)
	//	int IsMaryTTSspeaking()

	[MonoPInvokeCallback (typeof (fnType_Vs))]
	private static void napi_MaryTTSspeak(string s)
	{
		_maryTTSController.MaryTTSspeak (s);
	}

	[MonoPInvokeCallback (typeof (fnType_I))]
	private static int napi_IsMaryTTSspeaking()
	{
		// Converts bool return type to int
		return _maryTTSController.IsMaryTTSspeaking () ? 1 : 0;
	}
		
	[DllImport("__Internal")]
	private static extern void set_marytts_callbacks(
		fnType_Vs speak_fn,
		fnType_I is_speaking_fn
	);


	//
	// FACIAL EXPRESSION CONTROLLER
	//
//	void SetCurrentFacialExpression(string expression_name)
//	string GetCurrentFacialExpression()
//	void ClearFacialExpression()
//	string[] ListFacialExpressions()
//	SetExpressionTransitionTime(float transition_time_secs)
//	float GetExpressionTransitionTime()

	[MonoPInvokeCallback (typeof (fnType_Vs))]
	private static void napi_SetCurrentFacialExpression(string expression_name) {
		_facialExpressionController.SetCurrentFacialExpression (expression_name);
	}

	[MonoPInvokeCallback (typeof (fnType_S))]
	private static string napi_GetCurrentFacialExpression() {
		return _facialExpressionController.GetCurrentFacialExpression ();
	}

	[MonoPInvokeCallback (typeof (fnType_V))]
	private static void napi_ClearFacialExpression() {
		_facialExpressionController.ClearFacialExpression ();
	}

	[MonoPInvokeCallback (typeof (fnType_S))]
	private static string napi_ListFacialExpressions() {
		return StringArrayToJSON( _facialExpressionController.ListFacialExpressions () );
	}

	[MonoPInvokeCallback (typeof (fnType_Vf))]
	private static void napi_SetExpressionTransitionTime(float transition_time_secs) {
		_facialExpressionController.SetExpressionTransitionTime (transition_time_secs);
	}

	[MonoPInvokeCallback (typeof (fnType_F))]
	private static float napi_GetExpressionTransitionTime() {
		return _facialExpressionController.GetExpressionTransitionTime ();
	}

	[DllImport("__Internal")]
	private static extern void set_facial_expression_callbacks(
		fnType_Vs set_expression_fn,
		fnType_S get_expression_fn,
		fnType_V clear_expression_fn,
		fnType_S list_epxresisons_fn,
		fnType_Vf set_expr_trans_time_fn,
		fnType_F get_expr_trans_time_fn
	);

	//
	// EYE GAZE CONTROL
	//
//	void LookAtPoint(float x, float y, float z)
//	void LookAtObject(string target_object_name)
	[MonoPInvokeCallback (typeof (fnType_Vfff))]
	private static void napi_LookAtPoint(float x, float y, float z) {
		_eyeGazeController.LookAtPoint (new Vector3 (x, y, z));
	}

	[MonoPInvokeCallback (typeof (fnType_Vs))]
	private static void napi_LookAtObject(string target_object_name) {
		_eyeGazeController.LookAtObject (target_object_name);
	}

	[DllImport("__Internal")]
	private static extern void set_eyegaze_callbacks (
		fnType_Vfff look_at_point_fn,
		fnType_Vs look_at_object_fn
	);

	//
	//Camera controller
	//
	[MonoPInvokeCallback (typeof (fnType_V))]
	private static void napi_GoToNextShot() {
		_cameraController.GoToNextShot ();
	}

	[MonoPInvokeCallback (typeof (fnType_V))]
	private static void napi_GoToPreviousShot() {
		_cameraController.GoToPreviousShot ();
	}

	[MonoPInvokeCallback (typeof (fnType_S))]
	private static string napi_GetCurrentShot() {
		return _cameraController.GetCurrentShot ();
	}

	[MonoPInvokeCallback (typeof (fnType_Vs))]
	private static void napi_SetCurrentShot(string shot_name) {
		_cameraController.SetCurrentShot (shot_name);
	}

	[MonoPInvokeCallback (typeof (fnType_Vf))]
	private static void napi_SetShotFineTunePercentage(float percent) {
		_cameraController.SetShotFineTunePercentage(percent);
	}

	[MonoPInvokeCallback (typeof (fnType_F))]
	private static float napi_GetShotFineTunePercentage() {
		return _cameraController.GetShotFineTunePercentage();
	}

	[MonoPInvokeCallback (typeof (fnType_S))]
	private static string napi_ListCameraShots() {
		return StringArrayToJSON( _cameraController.ListCameraShots () );
	}

	//SetCurrentShot(string shot_name)
	//New?
	[DllImport("__Internal")]
	private static extern void set_camera_callbacks (
		//fnType_F get_shot_fine_tune_percent_fn
		fnType_V set_next_camera_shot_fn,
		fnType_V set_previous_camera_shot_fn,
		fnType_S get_camera_shot_name_fn,
		fnType_Vs set_camera_shot_fn,
		fnType_Vf set_shot_fine_tune_percentage,
		fnType_F get_shot_fine_tune_percentage,
		fnType_S list_camera_shots_fn

	);
}
