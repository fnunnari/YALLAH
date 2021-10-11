/*
Created by Fabrizios Seminar Group, 2021

This script allows you to let a character breathe and controll the breathing speed

How to use:
1. Select your MBLabCharacter in the Unity Scene, choose the breathing_animation_controller as Animation Controller (Animator > Controller)
2. Add this script to your MBLabCharacter and select the character as "Anim"
3. There is a chance that the breathing_animation_controller needs some additional setup, you might have to add the Animations to the states, in this case:
3.1 Go to the Animator View: then click on the sbreath state and add the IdleSlowBreath Animation as Motion, then click on the A-Pose state and add the APose Animation as Motion (e.g. WomanAPose)

For examples on how to use this script look at the FaceController.cs script
*/

using UnityEngine;
using System.Collections;

public class breath_controller : MonoBehaviour
{
    public Animator anim;   //the animator that this script controls (should be the "breathing_animation_controller)
    private float breathspeed = 1.16f; // = 16*13/180, should be a reasonable standard breathing rate, see the comment at "setBreathspeedperMin" for a in depth explanation
    private float currentSpeed;
    private float smoothDampVelocity;

    public float breathTransitionTime = 0.2F;

    void Start()
    {
        this.anim = GetComponent<Animator>();
        currentSpeed = breathspeed;
        anim.speed = breathspeed;
    }

    void Update()
    {
        if(currentSpeed != breathspeed){
            float speed = Mathf.SmoothDamp(currentSpeed, breathspeed, ref smoothDampVelocity, this.breathTransitionTime);
            anim.speed = speed;
            currentSpeed = speed;
        }
    }

    
    //use this to set how many times the character should breath per Minute (can be a non-integer number like 17.5)
    /*
    hints:
    Standard breathing values are between 12 and 18 breaths per minute (https://www.elsevier.com/de-de/connect/pflege/zahlen-zur-lunge)
    Males around 15.98 breaths per minute and females around 17.62 breaths per minute (Wilhelm et al., 2017)
    Anger increases the breaths per minute and Fear reduces them (Stemmler, 1989)

    Code explanation: the animation takes around 13 seconds and during the animation the character breathes 3 times,
    this is why we have to multiply the number of breaths per minute that we want with 13 / 60 * 3

    Note that there will be a transtition phase (so the change in breathing frequenzy doesn't happen immediately), 
    but you can change how long this transition is with "setTransitionTime"
    */
    public void setBreathspeedperMin(float breaths)
    {
        breathspeed = breaths * (float)(13.0f / 180.0f);
        anim.SetTrigger("breathe"); //starts the breathing animation, if it is not already playing
    }
    public void setBreathspeed(float speed)
    {
        breathspeed = speed;
        anim.SetTrigger("breathe"); //starts the breathing animation, if it is not already playing
    }
    public float getBreathspeed()
    {
        return (breathspeed);
    }
    //manually start the breathing
    public void goBreath()
    {
        anim.SetTrigger("breathe");
    }
    //go to the idle state
    public void goIdle()
    {
        anim.SetTrigger("goIdle");
    }
    //go to the APose state
    public void goAPose()
    {
        anim.SetTrigger("goAPose");
    }
    
    public void stopAnim()
    {
      breathspeed = 0f;
      anim.speed = breathspeed;
    }

    //set how long the transition will take, takes seconds as input
    public void setTransitionTime(float time)
    {
        breathTransitionTime = time;
    }
    public float getTransitionTime()
    {
        return (breathTransitionTime);
    }
}
