using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Comment here to use the Manually writte C# version of the blinker
using haxe.root ;
//using HaxeSpeedTest ;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class EyeBlinkController : MonoBehaviour {


	private SkinnedMeshRenderer skinnedMeshRenderer;
	private Mesh skinnedMesh;

	private EyeBlinker blinker = new EyeBlinker() ;
	private double[] viseme_weights = new double[EyeBlinker.get_viseme_count()] ;

	void Awake ()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		skinnedMesh = GetComponent<SkinnedMeshRenderer> ().sharedMesh;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		blinker.update (Time.time, this.viseme_weights);

		for(int i=0 ; i < EyeBlinker.get_viseme_count() ; i++) {
			string viseme = (string)(EyeBlinker.VISEMES [i]);

			int blendShapeIdx = this.skinnedMesh.GetBlendShapeIndex (viseme);
			// Debug.Log ("Looking for viseme " + viseme+". Index: " + blendShapeIdx);

			skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIdx, (float)(this.viseme_weights[i] * 100.0f));
		}

	}
}
