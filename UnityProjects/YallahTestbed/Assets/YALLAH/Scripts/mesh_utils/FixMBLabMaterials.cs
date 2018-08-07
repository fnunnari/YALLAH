using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** SUpport script to automatically fix the materials for a character imported from Manuel Bastioni Lab.
 */
public class FixMBLabMaterials : MonoBehaviour {

	/** It will be used as main color map for most materials. */
	public Texture diffuseMap;
	/** It will be used as bump map for the skin. */
	public Texture normalMap;
	/** It will be used as bump map for the skin. */
	public Texture eyeLashesDiffuse;

	// Use this for initialization
	void Start() {

		if (diffuseMap == null) {
			Debug.LogError ("diffuseMap should be present for  MLab character");
		}

		if (normalMap == null) {
			Debug.LogError ("normalMap should be present for  MLab character");
		}

		Shader stdSpecularShader = Shader.Find("Standard (Specular setup)");
		Debug.Log("Found shader: "+stdSpecularShader);

		// Retrieve the list of materials
		SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer> ();
		Material [] materials = renderer.materials;

		// For each material, apply the needed corrections.
		// In order to modify some material properties via script, we must use the EnableKeyword() method.
		// Please, read: https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
		foreach (Material m in materials) {

			// For ALL but not Fur and Cornea (Transparency issues)
			if (! (m.name.Contains ("fur") || m.name.Contains("cornea") )) {
				Debug.Log ("Fixing Material: '" + m.name + "'");
				m.shader = stdSpecularShader;

				m.EnableKeyword ("_GLOSSYREFLECTIONS_OFF");
				m.SetFloat ("_GlossyReflections", 0.0f);
			}


			//
			// Custom changes for each specific material
			if (m.name.Contains ("human_eyes")) {
				m.SetTexture ("_MainTex", diffuseMap);
				// m.SetFloat ("_Metallic", 0.5f);
				m.SetFloat ("_Glossiness", 0.6f);


			} else if (m.name.Contains ("pupil")) {
				m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");

				m.SetFloat("_SpecularHighlights", 0.0f);
				m.SetColor("_SpecColor", new Color(0, 0, 0));
				m.SetFloat ("_GlossMapScale", 0.5f);  // In the editor this is "Smoothness"

			} else if (m.name.Contains ("cornea")) {
//				m.SetFloat("_Mode", 3);
//
//				// Force the render queue to transparent
//				// http://answers.unity3d.com/questions/1004666/change-material-rendering-mode-in-runtime.html
//				m.DisableKeyword("_ALPHATEST_ON");
//				m.DisableKeyword("_ALPHABLEND_ON");
//				m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
//
//				m.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
//
//				m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
//				m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//				m.SetInt("_ZWrite", 0);
//				m.renderQueue = 3000;
//
//				m.SetTexture ("_MainTex", diffuseMap);
//				m.color = new Color (1f, 1f, 1f, 0.05f);
//				m.SetFloat ("_Glossiness", 0.8f);  // In the editor this is "Smoothness"
//
//				m.SetFloat("_SpecularHighlights", 1.0f);
//				m.SetColor("_SpecColor", new Color(25f/255f, 25f/255f, 25f/255f));
//				m.SetFloat ("_GlossMapScale", 0.85f);  // In the editor this is "Smoothness" (for Specular setup)

			} else if (m.name.Contains ("human_skin")) {
				//https://forum.unity.com/threads/cant-access-_specularhighlights-via-script-please-help.430238/
				//https://answers.unity.com/questions/1244189/reflection-and-specular-highlights-how-to-turn-ono.html
				m.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
				m.EnableKeyword("_NORMALMAP");

				m.SetTexture ("_MainTex", diffuseMap);
				m.color = Color.white;
				// m.EnableKeyword("_SPECGLOSSMAP"); -- use this only if you set a specular map.

				m.SetFloat("_SpecularHighlights", 1.0f);
				m.SetColor("_SpecColor", new Color(37f/255f, 23f/255f, 23f/255f));
				//m.SetFloat ("_Glossiness", 0.3f);  // In the editor this is "Smoothness" (for Metallic setup)
				m.SetFloat ("_GlossMapScale", 0.53f);  // In the editor this is "Smoothness" (for Specular setup)

				m.SetTexture ("_BumpMap", normalMap);  // In the editor this is "Normal Map"
				m.SetFloat("_BumpScale", 0.2f);  // In the editor this is the multiplier next to the Normal Map (no name).

			} else if (m.name.Contains ("generic")) {

			} else if (m.name.Contains ("fur")) {
//				m.SetFloat("_Mode", 2);
//				m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//				m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//				m.SetInt("_ZWrite", 0);
//				m.DisableKeyword("_ALPHATEST_ON");
//				m.EnableKeyword("_ALPHABLEND_ON");
//				m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//				m.renderQueue = 3000;
//
//				m.SetTexture ("_MainTex", eyeLashesDiffuse);
//
//				m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
//
//				m.color = Color.white;
//				// m.SetColor("_SpecColor", new Color(0f/255f, 0f/255f, 0f/255f));
//
//				m.SetFloat("_SpecularHighlights", 0.0f);
//				// Avoids grey/blue eyeleashes on Android builds.
//				//m.SetFloat ("_Glossiness", 0.0f);  // In the editor this is "Smoothness" (for metallic setup)
//				m.SetFloat ("_GlossMapScale", 0.0f);  // In the editor this is "Smoothness"

			} else if (m.name.Contains ("human_teeth")) {
				m.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");

				m.SetTexture ("_MainTex", diffuseMap);
				m.color = Color.white;

				m.SetFloat("_SpecularHighlights", 1.0f);
				m.SetColor("_SpecColor", new Color(70f/255f, 70f/255f, 70f/255f));
				m.SetFloat ("_Glossiness", 0.6f);  // In the editor this is "Smoothness" (for Metallic setup)
				//m.SetFloat ("_GlossMapScale", 0.75f);  // In the editor this is "Smoothness"

			} else {
				Debug.LogError ("Unexpected Material name: "+m.name);
			}
		}

	}

}
