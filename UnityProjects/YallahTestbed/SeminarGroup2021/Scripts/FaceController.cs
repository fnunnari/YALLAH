//Class for changing facial expressions
//Written by Kiarash Tamaddon, September 2017
/*
Modified by Jan Dickmann, Summer 2021, as part of the Seminar
We wanted the possibility to create our own facial expressions while the scene is running.
With this script, it is now possible to combine several facial expressions, and also to choose the intensity for them (number between 0 and 100)
For examples on how to use this script look at the FaceController.cs script
*/

//# Facial expressions are originally created as blendShapes in Blender and then as components of the character, they are imported to Unity. 
//In Unity, you can find them in the inspector of the imported mesh under the title of "Skinned Mesh Renderer" as "Blend Shapes".
//This script (SetExpressionValues.cs) provides the possibility to change the facial expressions in the inspector or while running (by pushing some buttons)
//
//# How to use: 
//1. add this code as component of the avatar's mesh

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class FaceController : MonoBehaviour
{

    public float expressionTransitionTime = 0.2F;
    // This will contain the indices of all the BlendShapes that we selected for control.
    private int[] expressionBlendShapeIndex;

    // The desired target value of each of the facial expression blendshapes.
    private float[] targetBlendShapesWeight;

    // The current smooth damp velocity of each facial blendshape.
    private float[] smoothDampVelocity;

    // Indicator of the curernt expressions
    // 0 means no expression. 1 to expressionBlendShapeIndex.Lenght is an expression
    //the first value is the blendShape that is used, the second value is the target intensity
    public List<Vector2Int> current_expression_index;
    private Vector2Int neutralFace;

#if UNITY_EDITOR
    public string currentExpressionName;
#endif

    // The Mesh from/to which we manipualte the BlendShapes
    private SkinnedMeshRenderer meshRendered;
    private Mesh sharedMesh;

    void Awake()
    {
        neutralFace.Set(0, 0);
        this.meshRendered = GetComponent<SkinnedMeshRenderer>();
        Assert.IsNotNull(this.meshRendered);
        this.sharedMesh = this.meshRendered.sharedMesh;

    }

    // Use this for initialization
    void Start()
    {
        current_expression_index.Add(neutralFace);

        /////////////////////////////////////////////////////////
        //get facial expressions from blend shapes
        /////////////////////////////////////////////////////////
        List<int> feIndex1 = new List<int>();

        for (int i = 0; i < sharedMesh.blendShapeCount; i++)
        {
            feIndex1.Add(i);
        }

        this.expressionBlendShapeIndex = feIndex1.ToArray();

        if (expressionBlendShapeIndex == null)
        {
            Debug.Log("expressionBlendShapeIndex is null");
        }

        this.targetBlendShapesWeight = new float[this.expressionBlendShapeIndex.Length];
        this.smoothDampVelocity = new float[this.expressionBlendShapeIndex.Length];
        ClearFacialExpression();
    }


    void Update()
    {

#if UNITY_EDITOR
        //		Debug.Log("expr index " + current_expression_index) ;
        //		Debug.Log("bs num " + (current_expression_index == 0 ? -1 : expressionBlendShapeIndex[current_expression_index-1]) ) ;
        this.currentExpressionName = current_expression_index[0].x == 0 ? "Normal" : meshRendered.sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[current_expression_index[0].x - 1]);
#endif

        //reset/set expressions values
        for (int expressionIndex = 0; expressionIndex < expressionBlendShapeIndex.Length - 1; expressionIndex++)
        {

            // set the target weight of the requested expressions.
            if ((current_expression_index.Exists(x => x.x == expressionIndex)))
            {
                //Debug.Log("Expression: "+ current_expression_index.Find(x => x.x - 1 == expressionIndex));
                targetBlendShapesWeight[expressionIndex] = current_expression_index.Find(x => x.x == expressionIndex).y;
            }
            else
            {
                //reset other expressions
                targetBlendShapesWeight[expressionIndex] = 0.0f;
            }

            float currentBSWeight = meshRendered.GetBlendShapeWeight(expressionBlendShapeIndex[expressionIndex]);
            float newBSWeight = Mathf.SmoothDamp(currentBSWeight, this.targetBlendShapesWeight[expressionIndex], ref this.smoothDampVelocity[expressionIndex], this.expressionTransitionTime);

            meshRendered.SetBlendShapeWeight(this.expressionBlendShapeIndex[expressionIndex], newBSWeight);
        }
    }

    public void SetExpressionTransitionTime(float transition_time_secs)
    {
        this.expressionTransitionTime = transition_time_secs;
    }

    public float GetExpressionTransitionTime()
    {
        return this.expressionTransitionTime;
    }

    public void ClearFacialExpression()
    {

        for (int expressionIndex = 0; expressionIndex < expressionBlendShapeIndex.Length - 1; expressionIndex++)
        {
            targetBlendShapesWeight[expressionIndex] = 0.0f;
        }
        current_expression_index.Clear();
        
        current_expression_index.Add(neutralFace);
    }

    //sets facial expression as passed by the names of the blend shapes and the associated intensity
    //expression_name is a string array that contains the names of the blend shapes that you want to use, the names have to match to the blend shapes of the MBLab Character (e.g. " string[] strInput = { "fe_scared01", "fe_shocked01", "Expressions_browsMidVert_max" }; ")
    //targetValue is a int array that contains the intensities for the blend shapes in expression_name, the order has to be the same as in expression_name and the values have to be between 0 and 100, (e.g. " int[] intInput = { 100, 50, 100 }; ")
    public void SetCurrentFacialExpression(string[] expression_name, int[] targetValue)
    {
        //check for some problems that could happen
        if(expression_name == null || expression_name.Length == 0 || targetValue == null || targetValue.Length == 0){
            Debug.LogError("ERROR: Array that is null or empty given as Argument", this);
            return;
        }
        if(expression_name.Length != targetValue.Length){
            Debug.LogError("ERROR: expression_name and targetValue arrays need to have same length", this);
            return;
        }
        //clear the old impressions
        current_expression_index.Clear();
        //for all expressions that we have in expressionBlendShapeIndex, get the name and then check for all expressions in expression_name whether the name matches
        for (int expressionIndex = 0; expressionIndex < expressionBlendShapeIndex.Length - 1; expressionIndex++)
        {
            string s = sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[expressionIndex]);
            for (int i = 0; i < expression_name.Length; i++)
            {
                if (expression_name[i] == s) //if the given expression is among the expressions that we know:
                {
                    //we create a new Vector2Int object, the first parameter is the expressionindex, the second variable is the target intensity value of that expression
                    Vector2Int expr = new Vector2Int(expressionIndex, targetValue[i]);
                    //and we add it to the current expressions
                    current_expression_index.Add(expr);
                }
            }
        }
    }
    
    //returns name of the current facial expression name
    public string GetCurrentFacialExpression()
    {
        foreach(Vector2Int v in current_expression_index)
        {
            if (v[0] == 0)
                return ("Normal");
            else
                return sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[v.x - 1]);
        }
        return "";
        
    }

    public string[] ListFacialExpressions()
    {
        List<string> expressions = new List<string>();

        for (int i = 0; i < expressionBlendShapeIndex.Length; i++)
        {
            expressions.Add(sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[i]));
        }
        return expressions.ToArray();
    }
}








