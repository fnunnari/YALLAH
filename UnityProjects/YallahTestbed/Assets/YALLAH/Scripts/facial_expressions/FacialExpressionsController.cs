//Class for changing facial expressions
//Written by Kiarash Tamaddon, September 2017

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


public class FacialExpressionsController : MonoBehaviour {

    // The name of the default expression, with no blend shapes applied.
    readonly static string DEFAULT_EXPRESSION_NAME = "Normal";

    // All teh blendshapes starting with this prefix will be considered as facial expressions.
    readonly static string EXPRESSION_BLENDSHAPE_PREFIX = "fe_";

	public float expressionTransitionTime = 0.2F;
	// This will contain the indices of all the BlendShapes that we selected for control.
	private int[] expressionBlendShapeIndex;


	// The desired target value of each of the facial expression blendshape.
	private float[] targetBlendShapesWeight;

	// The current smooth damp velocity of each facial blendshape.
	private float[] smoothDampVelocity;

	// Indicator of the curernt expression
	// 0 means no expression. 1 to expressionBlendShapeIndex.Lenght is an expression
	public int current_expression_index = 0 ;

	#if UNITY_EDITOR
	public string currentExpressionName;
	#endif

	// The Mesh from/to which we manipualte the BlendShapes
	private SkinnedMeshRenderer meshRendered;
	private Mesh sharedMesh;

	void Awake() {
		
		this.meshRendered = GetComponent<SkinnedMeshRenderer>();
		Assert.IsNotNull(this.meshRendered) ;
		this.sharedMesh = this.meshRendered.sharedMesh;

	}

	// Use this for initialization
	void Start () {		

		/////////////////////////////////////////////////////////
		//get facial expressions from blend shapes
		/////////////////////////////////////////////////////////
		List<int> feIndex1 = new List<int>();

		for (int i=0; i < sharedMesh.blendShapeCount; i++)
		{
			string s = sharedMesh.GetBlendShapeName(i);
            if(s.StartsWith(EXPRESSION_BLENDSHAPE_PREFIX)){
				feIndex1.Add (i);
			}
		}

		this.expressionBlendShapeIndex = feIndex1.ToArray() ;

		this.targetBlendShapesWeight = new float[this.expressionBlendShapeIndex.Length];
		this.smoothDampVelocity = new float[this.expressionBlendShapeIndex.Length];
		ClearFacialExpression ();
	}


	void Update () {

		#if UNITY_EDITOR
//		Debug.Log("expr index " + current_expression_index) ;
//		Debug.Log("bs num " + (current_expression_index == 0 ? -1 : expressionBlendShapeIndex[current_expression_index-1]) ) ;
        this.currentExpressionName = current_expression_index == 0 ? DEFAULT_EXPRESSION_NAME : meshRendered.sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[current_expression_index-1]) ;
		#endif
		
		//reset/set expressions values
		for (int expressionIndex = 0; expressionIndex < expressionBlendShapeIndex.Length - 1; expressionIndex++) {

			// set the target weight of the requested expression to 100.
			if ((expressionIndex == (current_expression_index-1))) {
				targetBlendShapesWeight [expressionIndex] = 100;
			}else{
				//reset other expressions
				targetBlendShapesWeight [expressionIndex] = 0.0f;
			}

			float currentBSWeight = meshRendered.GetBlendShapeWeight(expressionBlendShapeIndex[expressionIndex]) ;
			float newBSWeight = Mathf.SmoothDamp(currentBSWeight, this.targetBlendShapesWeight [expressionIndex], ref this.smoothDampVelocity[expressionIndex], this.expressionTransitionTime);

			meshRendered.SetBlendShapeWeight (this.expressionBlendShapeIndex[expressionIndex], newBSWeight);
		}
	}
		
	public void SetNextFacialExpression(){
		current_expression_index += 1;
		if (current_expression_index > expressionBlendShapeIndex.Length) current_expression_index = 0;
	}

	public void SetPreviousFacialExpression(){
		current_expression_index -= 1;
		if (current_expression_index < 0) current_expression_index = expressionBlendShapeIndex.Length;
	}

	public void SetExpressionTransitionTime(float transition_time_secs) {
		this.expressionTransitionTime = transition_time_secs;
	}

	public float GetExpressionTransitionTime() {
		return this.expressionTransitionTime;
	}

	public void ClearFacialExpression() {

		for (int expressionIndex = 0; expressionIndex < expressionBlendShapeIndex.Length - 1; expressionIndex++) {
			targetBlendShapesWeight [expressionIndex] = 0.0f;
		}

		current_expression_index = 0;
	}

	//sets facial expression as passed by its name
	public void SetCurrentFacialExpression(string expression_name){
		for (int expressionIndex = 0; expressionIndex < expressionBlendShapeIndex.Length - 1; expressionIndex++) {
			string s = sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[expressionIndex]);
			if (s == expression_name) {
				current_expression_index = expressionIndex + 1;
				break;
			}			
		}
	}

	//returns name of the current facial expression name
	public string GetCurrentFacialExpression (){
		if (current_expression_index == 0)
            return(DEFAULT_EXPRESSION_NAME);
		else
			return sharedMesh.GetBlendShapeName(expressionBlendShapeIndex [current_expression_index-1]);
	}

	public string[] ListFacialExpressions() {
		List<string> expressions = new List<string>();

		for(int i = 0; i< expressionBlendShapeIndex.Length;i++) 
		{
			expressions.Add(sharedMesh.GetBlendShapeName(expressionBlendShapeIndex[i])) ;
		}
		return expressions.ToArray() ;
	}
}








