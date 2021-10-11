/*
Created by Fabrizios Seminar Group, 2021

We used this script to create short videos that showcase the emotions shame, anger and fear, with different intensities of involuntary movements, namely none, normal, emotional and cartoonish
we mapped the expressions on buttons (q w e r   a s d f   y x c v)
we also added some other helpful controls to "n" for normal, "b" for the default position without breathing, "m" for the A pose

You can use this code as example on how to animate a character with our scripts and reuse some of the code here to do it yourself

How to use:
1. Add this Script to the MBLab Character Node (e.g. MBLabFemale)
2. When you start the sketch and all the other scripts are also in place (blush_controller.cs, BlinkController.cs, breath_controller.cs and FaceController.cs), you should be able to use the buttons as mentioned above
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoScript : MonoBehaviour {

    private breath_controller breath;
    private blush_controller blush;
    private BlinkController blink;
    //private FacialExpressionsController face;
    private FaceController faceControl;

    private float expressionTransitionTime = 0.5f;
    private float blushTransitionTime = 2f;

    void Start () {
        breath = GetComponent<breath_controller>();
        blush = GetComponentInChildren<blush_controller>();
        blink = GetComponentInChildren<BlinkController>();
        faceControl = GetComponentInChildren<FaceController>();
    }

    void Update () {
        if (Input.GetKeyDown("n")) //n for normal behavior
        {
            Debug.Log("normal");
            normalBehavior();
        }
        if (Input.GetKeyDown("b")) //b for default position/to stop breathing
        {
            Debug.Log("default");
            defaultPosition();
        }
        if (Input.GetKeyDown("m")) //m for A pose
        {
            Debug.Log("aPose");
            aPose();
        }

        //no involuntary movements expressions

        //none - shameful
        if (Input.GetKeyDown("q"))
        {
            Debug.Log("none- shameful");
            string[] strInput = { "fe_embarrassed01"};
            int[] intInput = { 100 };
            noMovements(strInput, intInput, expressionTransitionTime);
        }

        //none - angry
        if (Input.GetKeyDown("a"))
        {
            Debug.Log("none- angry");
            string[] strInput = { "fe_angry01" };
            int[] intInput = { 100 };
            noMovements(strInput, intInput, expressionTransitionTime);
        }

        //none - afraid
        if (Input.GetKeyDown("y"))
        {
            Debug.Log("none- afraid");
            string[] strInput = { "fe_scared01", "fe_shocked01", "Expressions_browsMidVert_max" };
            int[] intInput = { 100, 50, 100 };
            noMovements(strInput, intInput, expressionTransitionTime);
        }

        //normal/neutral involuntary movements

        //neutral - shameful
        if (Input.GetKeyDown("w"))
        {
            Debug.Log("neutral- shameful");
            string[] strInput = { "fe_embarrassed01" };
            int[] intInput = { 100 };
            neutral(strInput, intInput, expressionTransitionTime);
        }

        //neutral - angry
        if (Input.GetKeyDown("s"))
        {
            Debug.Log("neutral- angry");
            string[] strInput = { "fe_angry01" };
            int[] intInput = { 100 };
            neutral(strInput, intInput, expressionTransitionTime);
        }

        //neutral - afraid
        if (Input.GetKeyDown("x"))
        {
            Debug.Log("neutral- afraid");
            string[] strInput = { "fe_scared01", "fe_shocked01", "Expressions_browsMidVert_max" };
            int[] intInput = { 100, 50, 100 };
            neutral(strInput, intInput, expressionTransitionTime);
        }

        //emotional involuntary movements

        //emotional - shameful
        if (Input.GetKeyDown("e"))
        {
            Debug.Log("emotional- shameful");
            string[] strInput = { "fe_embarrassed01" };
            int[] intInput = { 100 };
            animationScript(strInput, intInput, expressionTransitionTime,/* breathsPerMinute = */ 20f,/* blinkPerMinute = */ 30f,/* blushfactor = */ 0.4f, blushTransitionTime);
        }

        //emotional - angry
        if (Input.GetKeyDown("d"))
        {
            Debug.Log("emotional- angry");
            string[] strInput = { "fe_angry01" };
            int[] intInput = { 100 };
            animationScript(strInput, intInput, expressionTransitionTime,/* breathsPerMinute = */ 30f,/* blinkPerMinute = */ 30f,/* blushfactor = */ 0.4f, blushTransitionTime);
        }

        //emotional - afraid
        if (Input.GetKeyDown("c"))
        {
            Debug.Log("emotional- afraid");
            string[] strInput = { "fe_scared01", "fe_shocked01", "Expressions_browsMidVert_max" };
            int[] intInput = { 100, 50, 100 };
            animationScript(strInput, intInput, expressionTransitionTime,/* breathsPerMinute = */ 25f,/* blinkPerMinute = */ 21f,/* blushfactor = */ -0.4f, blushTransitionTime);
        }

        //cartoonish involuntary movements

        //cartoonish - shameful
        if (Input.GetKeyDown("r"))
        {
            Debug.Log("cartoonish- shameful");
            string[] strInput = { "fe_embarrassed01" };
            int[] intInput = { 100 };
            animationScript(strInput, intInput, expressionTransitionTime,/* breathsPerMinute = */ 45f,/* blinkPerMinute = */ 45f,/* blushfactor = */ 0.8f, blushTransitionTime);
        }

        //cartoonish - angry
        if (Input.GetKeyDown("f"))
        {
            Debug.Log("cartoonish- angry");
            string[] strInput = { "fe_angry01" };
            int[] intInput = { 100 };
            animationScript(strInput, intInput, expressionTransitionTime,/* breathsPerMinute = */ 45f,/* blinkPerMinute = */ 45f,/* blushfactor = */ 0.8f, blushTransitionTime);
        }

        //cartoonish - afraid
        if (Input.GetKeyDown("v"))
        {
            Debug.Log("cartoonish- afraid");
            string[] strInput = { "fe_scared01", "fe_shocked01", "Expressions_browsMidVert_max" };
            int[] intInput = { 100, 50, 100 };
            animationScript(strInput, intInput, expressionTransitionTime,/* breathsPerMinute = */ 45f,/* blinkPerMinute = */ 45f,/* blushfactor = */ -0.6f, blushTransitionTime);
        }

        if (Input.GetKeyDown(".")) // . to list the animation strings
        {
            Debug.Log("normal");
            for (int i = 0; i < faceControl.ListFacialExpressions().Length; i++)
            {
                Debug.Log(i + ":  " + faceControl.ListFacialExpressions()[i]);
            }
        }

    }

    void animationScript(string[] facialexpressions, int[] faceweights, float facetransitiontime, float breathspeed, float blinkspeed, float blushfactor,  float blushtransitiontime)
    {
        breath.setBreathspeedperMin(breathspeed); //breaths per minute (17 normal)
        blush.setTargetBlushFactor(blushfactor);
        blush.setTransitionTime(blushtransitiontime);
        blink.InitBlinkController(true, 60/blinkspeed, 0.25f);
        blink.setBlinksPerMinute(blinkspeed); //blinks per minute (normal 14)
        faceControl.SetCurrentFacialExpression(facialexpressions, faceweights);
        faceControl.SetExpressionTransitionTime(facetransitiontime);

    }
    void neutral(string[] facialexpressions, int[] faceweights, float facetransitiontime)
    {
        breath.setBreathspeedperMin(17.6f); //breaths per minute (17 normal)
        blush.setTargetBlushFactor(0);
        blush.setTransitionTime(0);
        blink.InitBlinkController(true, 60 / 14, 0.25f);
        blink.setBlinksPerMinute(14.7f); //blinks per minute (normal 14)
        faceControl.SetCurrentFacialExpression(facialexpressions, faceweights);
        faceControl.SetExpressionTransitionTime(facetransitiontime);
    }

    void noMovements(string[] facialexpressions, int[] faceweights, float facetransitiontime)
    {
        //breath.setBreathspeedperMin(17.6f); //breaths per minute (17 normal)
        blush.setTargetBlushFactor(0);
        blush.setTransitionTime(0);
        blink.InitBlinkController(false, 60 / 14, 0.25f);
        //blink.setBlinksPerMinute(14.7f); //blinks per minute (normal 14)
        faceControl.SetCurrentFacialExpression(facialexpressions, faceweights);
        faceControl.SetExpressionTransitionTime(facetransitiontime);
    }


    void normalBehavior()
    {
        breath.setBreathspeed(1.0f);
        blush.setTargetBlushFactor(0.0f);
        blush.setBlushFactor(0.0f);
        blink.InitBlinkController(true, 4f,0.25f);
        faceControl.ClearFacialExpression();
    }
    void defaultPosition()
    {
        //breath.setAnimSpeed(1000.0f);
       // breath.goAPose(); //no idle animation there right now, this is a workaround
        
       // breath.setBreathspeed(1f);
        breath.setBreathspeed(0.0f);
        blush.setTargetBlushFactor(0.0f);
        blush.setBlushFactor(0.0f);
        blink.InitBlinkController(false, 4f,0.25f);
        faceControl.ClearFacialExpression();
    }
    void aPose()
    {
        //breath.setAnimSpeed(10.0f);
        breath.goAPose();
        blush.setTargetBlushFactor(0.0f);
        blush.setBlushFactor(0.0f);
        blink.InitBlinkController(false, 4f, 0.25f);
        faceControl.ClearFacialExpression();
    }

}
