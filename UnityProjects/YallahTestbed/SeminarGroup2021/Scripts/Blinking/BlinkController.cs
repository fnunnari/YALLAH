/*
Created by Fabrizios Seminar Group, 2021

This script allows you to let a character blink and controll the blinking speed

How to use:
1. Add this script to the mesh (e.g. MBLabFemaleMesh)

For examples on how to use this script look at the FaceController.cs script
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaxeSpeedTest;

public class BlinkController : MonoBehaviour
{
    SkinnedMeshRenderer skinnymeshrenderer;
    Mesh skinnymesh;

    int amountBlendShapes;
    float weight;
    bool waiting, opening, closing;
    bool blinking;
    float secondstonextblink;

    [Range(1f, 60f)]
    public float averageIntervall = 4f;

    [Range(0f, 0.9f)]
    public float intervallWidth = 0.25f;


    BlinkStatus blinkStatus;
    ControlStatus control;


    float time;
    float lastblinktime;

    //blendShapes: left eye 19, right eye 25

    void Awake()
    {
        skinnymeshrenderer = GetComponent<SkinnedMeshRenderer>();
        skinnymesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    void Start()
    {
        amountBlendShapes = skinnymesh.blendShapeCount;
        weight = 0f;
        blinking = false;
        control = ControlStatus.TIME;
        SetGlobalStatus(BlinkStatus.WAITING, false, false, true);

        time = Time.deltaTime;
        lastblinktime = time;
        secondstonextblink = Random.Range(averageIntervall - (averageIntervall * intervallWidth), averageIntervall + (averageIntervall * intervallWidth));
    }

    public void InitBlinkController(bool automaticBlinking, float averageIntervall, float intervallWidth)
    {
        this.control = automaticBlinking ? ControlStatus.TIME : ControlStatus.MANUAL;
        this.averageIntervall = averageIntervall;
        this.intervallWidth = intervallWidth;
    }

    void Update()
    {
        time = Time.time - lastblinktime;

        if (control == ControlStatus.TIME && time >= secondstonextblink)
        {
            Blink(averageIntervall, intervallWidth);
            lastblinktime = Time.time;
        }

        if (amountBlendShapes > 0)
        {
            switch (blinkStatus)
            {
                case BlinkStatus.WAITING:
                    break;
                case BlinkStatus.OPENING:
                    if (weight > 0f && opening)
                    {
                        ChangeBlendShapeWeight(19, weight);
                        ChangeBlendShapeWeight(25, weight);
                        weight = weight - 10f;
                    }
                    else
                    {
                        SetGlobalStatus(BlinkStatus.WAITING, false, false, true);
                        blinking = false;
                    }
                    break;
                case BlinkStatus.CLOSING:
                    if (weight < 100f && closing)
                    {
                        ChangeBlendShapeWeight(19, weight);
                        ChangeBlendShapeWeight(25, weight);
                        weight = weight + 10f;
                    }
                    else
                    {
                        SetGlobalStatus(BlinkStatus.OPENING, true, false, false);
                    }
                    break;
            }
        }
    }

    public void Blink(float averageIntervall, float intervallWidth)
    {
        if (control == ControlStatus.TIME)
        {
            secondstonextblink = Random.Range(averageIntervall - (averageIntervall * intervallWidth), averageIntervall + (averageIntervall * intervallWidth));
            
        }

        if (!blinking)
        {
            SetGlobalStatus(BlinkStatus.CLOSING, false, true, false);
            blinking = true;
        }
    }

    public void setAverageBlinkingIntervall(float intervall)
    {
        averageIntervall = intervall;
    }

    public void setBlinksPerMinute(float blinksPerMinute)
    {
        if (blinksPerMinute == 0)
        {
            Debug.Log("Zero Blinks per Minute not allowed.");
            return;
        }
        setAverageBlinkingIntervall(60 / blinksPerMinute);
    }

    private void SetGlobalStatus(BlinkStatus status, bool open, bool close, bool wait)
    {
        blinkStatus = status;
        opening = open;
        closing = close;
        waiting = wait;
    }

    private void ChangeBlendShapeWeight(int blendShapeIndex, float weight)
    {
        skinnymeshrenderer.SetBlendShapeWeight(blendShapeIndex, weight);
    }

    enum ControlStatus
    {
        TIME,
        MANUAL,
    }

    enum BlinkSpeed
    {
        SUPERSLOW,
        SLOW,
        NORMAL,
        FAST,
        SUPERFAST
    }
}
