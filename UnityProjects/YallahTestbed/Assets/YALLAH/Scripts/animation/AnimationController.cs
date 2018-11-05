//A class for controlling animations of the character 
//Written by Kiarash Tamaddon, November 2017

//This class uses an AnimatorOverrideController to run an animation in the "ACTION" state
//The animations should de defined in the inspector under "animationClips"

//How to use:
//1. add this script to the character avatar (The GameObject containing both the skeleton and the mesh)
//2. add the animator "CharAnimationController" to the character.
//2. add the animation "DEFAULT ACTION" to the "ACTION" state
//4. enter the number of animations in the serialized field in the inspector 
//5. add animations that are to be run in the serialized field of the inspector  

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;


public class AnimationController : MonoBehaviour
{
	// Name of the states we must see in the Animator
	private string IDLE_STATE_NAME = "IdleState";
	private string CURRENT_ACTION_STATE_NAME = "CurrentActionState" ;
	private string AMBIENT_STATE_NAME = "AmbientState";

	// Name of the AnimationClips set in the Animator's states.
	private string DUMMY_STATIC_POSE_ANIMATION_NAME = "DUMMY_STATIC_POSE_ANIMATION";
	private string DUMMY_CURRENT_ANIMATION_NAME = "DUMMY_CURRENT_ANIMATION";
	private string DUMMY_IDLE_AMBIENT_ANIMATION_NAME = "DUMMY_IDLE_AMBIENT_ANIMATION";


	// Whether the ambient animation should be activated at start
	public bool enableAmbientAtStart ;

	// The animation clip used for the ambient motion
	public AnimationClip idleAmbientAnimationClip;

	// The animation clip used for th idle state
	public AnimationClip staticPoseAnimationClip;

	// The list of animation clips to be used for playback
	public AnimationClip[] animationClips;

	[Range(0.0f,2f)]
	public float animationTransitionTime = 0.4f;

	// The animator "Char Animation Controller" that must be added by the user
	private Animator animator;

	// Used to set at run-time the animation clip in the states
	private AnimatorOverrideController animatorOverrideController;

	// When the current animation has started playing
	private float animationClipStartTime;
	// The duration of the animation currently playing
	private float animationClipLength;

	// Wheter to switch to the ambianet animation when the current animation finishes.
	private bool useAmbientAnimation;

    #if UNITY_EDITOR
    [Header("Test:")]
    public bool playRandomAnim = false;
    #endif


	[Header("Locomotion Clips:")]

	// The animation clip used for walking forward
	public AnimationClip walkCycleAnimationClip;

	// The animation clip used to turn right
	public AnimationClip turnRightAnimationClip;

	// The animation clip used to turn left
	public AnimationClip turnLeftAnimationClip;


	public void Start()
	{
		// Tweak the animator so that we can be able to set the animation clips at run-time.
		animator = GetComponent<Animator>();
		// Debug.Log("Override gesture") ;
		animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animator.runtimeAnimatorController = animatorOverrideController;

		// Set the animations for the idle mode and the "ambient".
		animatorOverrideController [DUMMY_STATIC_POSE_ANIMATION_NAME] = this.staticPoseAnimationClip;
		animatorOverrideController [DUMMY_IDLE_AMBIENT_ANIMATION_NAME] = this.idleAmbientAnimationClip;

		animatorOverrideController ["DUMMY_WALK_CYCLE_ANIMATION"] = this.walkCycleAnimationClip;
		animatorOverrideController ["DUMMY_TURN_RIGHT_ANIMATION"] = this.turnRightAnimationClip;
		animatorOverrideController ["DUMMY_TURN_LEFT_ANIMATION"] = this.turnLeftAnimationClip;

		//this.ambientAnimationClip.SetCurve ("Anna/root/pelvis/spine01/spine02/spine03/neck", typeof(Transform), "m_localPosition.x", null);

		animationClipLength = -1f;
		useAmbientAnimation = true;

		if (this.enableAmbientAtStart)
		{
			this.EnableAmbientAnimation() ;
		}
		else
		{
			this.DisableAmbientAnimation() ;
		}

	}



	public void Update() {

		#if UNITY_EDITOR
		if(this.playRandomAnim) {
			int n_anims = this.animationClips.Length;
			if (n_anims > 0) {
				int rnd_anim_idx = Random.Range (0, n_anims);
				this.PlayAnimationClip (rnd_anim_idx);
			}

			this.playRandomAnim = false;
		}
		#endif

		// If the current animation reaches the end, we fade back to either standing or the ambiant animation.
		if ((Time.time - animationClipStartTime) > animationClipLength && animationClipLength > 0)
		{
			animationClipLength = -1f;
			//Debug.Log ("Animation name:" + animatorOverrideController ["DEFAULT ACTION"].name);
			if (useAmbientAnimation) {
				animator.CrossFadeInFixedTime (AMBIENT_STATE_NAME, animationTransitionTime);
			}
			else
			{
				animator.CrossFadeInFixedTime (IDLE_STATE_NAME, animationTransitionTime);

			}
			//print (animationTransitionTime);
		}
		//print (GetPlayingAnimationClipName ());
		//Debug.Log("Playing: "+this.IsAnimationClipPlaying());
	}


	public string[] ListAnimationClips()
	{
		List<string> clip_names = new List<string>();

		foreach (AnimationClip anim_clip in animationClips) 
		{
			clip_names.Add(anim_clip.name) ;
		}

		return clip_names.ToArray() ;
	}


	public void PlayAnimationClip(string animationName)
	{

		for (int anim_num = 0; anim_num < this.animationClips.Length; anim_num++) {
			if (animationName == this.animationClips [anim_num].name) {
				this.PlayAnimationClip (anim_num);
				break;
			}
		}

	}

	// Play the specified animation number.
	public void PlayAnimationClip(int animationNo)
	{

		if (! this.IsAnimationClipPlaying() ) {
			if (animationNo < animationClips.Length)
				// replaces (overrides) the default animation with the animation whose number is passed to this function
				animatorOverrideController [DUMMY_CURRENT_ANIMATION_NAME] = animationClips [animationNo];
			else
				Assert.AreNotEqual(animationNo, animationClips.Length);

			//Debug.Log ("Animation name:" + animatorOverrideController ["DEFAULT ACTION"].name);
			animator.CrossFadeInFixedTime (CURRENT_ACTION_STATE_NAME, animationTransitionTime);

			//store data for transition at the end of play
			animationClipStartTime = Time.time;
			animationClipLength = animationClips [animationNo].length;
			// print ("current time " + Time.time +"   time= " + animationClips [animationNo].length);


		}
	}

	// activate the ambient animation
	public void EnableAmbientAnimation()
	{
		useAmbientAnimation = true;
		animator.CrossFadeInFixedTime (AMBIENT_STATE_NAME, 0.3f);
	}

	// deactivate the ambient animation
	public void DisableAmbientAnimation()
	{
		useAmbientAnimation = false;
		animator.CrossFadeInFixedTime (IDLE_STATE_NAME, 0.3f);
	}

    public bool IsAmbientAnimationEnabled()
    {
        return useAmbientAnimation == true;
    }

	public void SetAnimationTransitionTime (float transitionTime)
	{
		animationTransitionTime = transitionTime;
	}

	public float GetAnimationTransitionTime ()
	{
		return animationTransitionTime;
	}

	//returns true if an animation is running in the ACTION state
	public bool IsAnimationClipPlaying()
	{
		return (animator.GetCurrentAnimatorStateInfo (0).IsName (CURRENT_ACTION_STATE_NAME));
	}

	/** Get the name of the animation clip currently playing.
	 * Returns null if not animation is playing.
	 */
	public string GetPlayingAnimationClipName()
	{
		if(animator.GetCurrentAnimatorStateInfo(0).IsName(CURRENT_ACTION_STATE_NAME))
			return(animatorOverrideController [DUMMY_CURRENT_ANIMATION_NAME].name);
		else
			return null;		
	}
}