﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class TestAllFeatures : MonoBehaviour {


    [Tooltip("Please, drag here the avatar (parent of the body mesh)")]
    public GameObject avatar;

    [Tooltip("Please, drag here the body mesh (child of the avatar)")]
    public GameObject body;



    [Header("Gaze:")]
    public bool enableGaze;
    [Tooltip("The character will stare at this Game Object.")]
    public GameObject gazeTarget;
    [Tooltip("The gaze target will orbit around the head of the character.")]
    public bool animateGazeTarget = false;
    private EyeHeadGazeController gazescript;


    [Header("Text-to-Speech:")]
    public bool enableTTS;
    [Tooltip("The character will say something every x seconds")]
    public float speakIntervalSecs = 5.0f;
    private float lastSpeakStart = 0.0f;
    private MaryTTSController ttsController;


    [Header("Facial Expression:")]
    public bool enableFacialExpression;
    [Tooltip("The will randomly walk somewhere every x seconds")]
    public float facialExpressionChangeIntervalSecs = 8.0f;
    public float facialExpressionDurationSecs = 4.0f;
    private float lastFacialExprStart = 0.0f;
    private FacialExpressionsController facialExpressionsController;


    [Header("Locomotion:")]
    public bool enableLocomotion;
    [Tooltip("The will randomly walk somewhere every x seconds")]
    public float walkIntervalSecs = 10.0f;
    private float lastWalkStart = 0.0f;
    private LocomotionController locomotionController;


    // Use this for initialization
    void Start()
    {
        if (this.avatar == null)
        {
            Debug.LogError("The 'avatar' object has not been specified");
            Debug.Break();
        }

        if (this.body == null)
        {
            Debug.LogError("The 'body' object has not been specified");
            Debug.Break();
        }

        this.gazescript = this.body.GetComponent<EyeHeadGazeController>();
        this.ttsController = this.body.GetComponent<MaryTTSController>();
        this.facialExpressionsController = this.body.GetComponent<FacialExpressionsController>();
        this.locomotionController = this.avatar.GetComponent<LocomotionController>();


        // Look at the target
        if (this.gazeTarget != null)
        {
            this.gazescript.LookAtObject(this.gazeTarget.name);
        }
    }


    // Update is called once per frame
    void Update() {
        float now = Time.time ;

        //
        // Move Eye gaze target
        if (this.enableGaze)
        {
            if (this.animateGazeTarget)
            {
                // Sinusoidal orbit around the character's head.
                Vector3 gaze_position = new Vector3(Mathf.Sin(now * 2.0f) * 1.0f,
                                                     1.5f + Mathf.Sin(now * 3.0f) * 1.0f,
                                                     Mathf.Sin(now * 4.0f) * 0.7f);
                gaze_position += gameObject.transform.position;
                // print ("Looking at " + gaze_position);
                this.gazeTarget.transform.position = gaze_position;
            }
        }


        //
        // Repeat a sentence
        if (this.enableTTS)
        {
            if (now - this.lastSpeakStart > this.speakIntervalSecs)
            {
                this.ttsController.MaryTTSspeak("The quick brown fox jumps over the lazy dog.");

                this.lastSpeakStart = now;
            }
        }


        //
        // Walk here and there
        if(this.enableLocomotion) {
            if(now - this.lastWalkStart > this.walkIntervalSecs)
            {
                Vector3 target_position = new Vector3(UnityEngine.Random.Range(-4, 4),
                                                      0.0f,
                                                      UnityEngine.Random.Range(-4, 4));
                Debug.Log("Walking to " + target_position);
                this.locomotionController.WalkTo(target_position);

                this.lastWalkStart = now;
            }
        }


        //
        // Manage Facial Expression
        if (this.enableFacialExpression)
        {
            if (now - this.lastFacialExprStart > this.facialExpressionChangeIntervalSecs)
            {
                string[] facial_expressions = this.facialExpressionsController.ListFacialExpressions();
                // expression 0 is the defaul expression. Let's randomize the others
                int expr_num = UnityEngine.Random.Range(1, facial_expressions.Length);
                string expr_name = facial_expressions[expr_num];
                Debug.Log("Setting facial expression to '" + expr_name + "'");
                this.facialExpressionsController.SetCurrentFacialExpression(expr_name);

                this.lastFacialExprStart = now;
            }
            else if (this.facialExpressionsController.GetCurrentFacialExpression() != "Normal")
            {
                if (now - this.lastFacialExprStart > facialExpressionDurationSecs)
                {
                    this.facialExpressionsController.ClearFacialExpression();
                }
            }
        }


    }

}
