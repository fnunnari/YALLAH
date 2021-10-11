/*
Created by Fabrizios Seminar Group, 2021

This script allows you to let a character blush or become pale.

In the folder that this script is in, there are example textures for "MBLab Caucasian Woman", for other characters you will have to create blush and pale textures yourself

How to use:
1. Select our special blushshader (blushShader.shader) as shader for the skin material of your Character (e.g. MBlabFemaleMesh > MBlab_human_skin)
2. Select the Albedo Textures, as "Albedo (RGB)" texture (that is the first one) the normal MBlab texture, the "Albedo_Blush" texture (second one) needs to be a modified version of the first texture that is blushing heavily
    the "Albedo_Pale" Texture (the third one), needs to be a modified version of the first texture that is very pale.
    You can also add an Bumpmap (the forth textureslot)
    (Optioinal) Add the bump map (e.g. human_female_bump) as 4th texture, this is optional and we chose not to do it because we think it looks worse
3. Add this script to the mesh

For examples on how to use this script look at the FaceController.cs script
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blush_controller : MonoBehaviour {
    [Range(0f, 1f)]
    private float blend = 0f;
    Renderer m_Renderer;
    private float targetblush = 0f;
    private Material[] materials;
    private Material skinMaterial;
    public float expressionTransitionTime = 0.2F; //the time it takes to blush/pale, so how long the transition between the current blushfactor and the target blushfactor will take
    private float targetBlush;
    private float smoothDampVelocity;
    
    void Start () {
        m_Renderer = GetComponent<Renderer>();
        materials = m_Renderer.materials;
        skinMaterial = materials[3];  //materials 3 is the skin of the character (MBLab_human_skin), at least in our build that we use, this might have to be adjusted for other builds
    }
	
	void Update () {

        float currentblush = getBlushFactor();
        float newblush = Mathf.SmoothDamp(currentblush, targetblush, ref smoothDampVelocity, this.expressionTransitionTime);
        blend = newblush;
        skinMaterial.SetFloat("_Blend", blend);

    }

    //This method is useless right now, unless you also set the TargetBlushFactor to the same value, otherwise the blushfactor will just change to the current TargetBlushFactor
    public void setBlushFactor(float blushfactor)
    {
        blend = blushfactor;
    }

    //Use this to set the target blush factor (this is the blushfactor after the transition time), values should be between -1 and 1
    public void setTargetBlushFactor(float blushfactor)
    {
        targetblush = blushfactor;
    }

    public float getBlushFactor()
    {
        return(blend);
    }

    public float getTargetBlushFactor()
    {
        return (targetblush);
    }
    //set how long the transition will take, takes seconds as input
    public void setTransitionTime(float time)
    {
        expressionTransitionTime = time;
    }

    public float getTransitionTime()
    {
        return (expressionTransitionTime);
    }
}
