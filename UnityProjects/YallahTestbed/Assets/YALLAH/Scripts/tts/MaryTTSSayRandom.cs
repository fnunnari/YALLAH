using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MaryTTSController))]
public class MaryTTSSayRandom : MonoBehaviour {

	private string[] SENTENCES = new string[] {
		"Hello, how are you?",
		"If everything goes right both individuals shake hands and go back to the world with a smile.",
		"I am gonna make him an offer he cant refuse.",
		"They may take our lives but not our freedom.",
		"The quick brown fox jumps over the lazy dog.",
	} ;

	// The controller which allows us to speak through MaryTTS
    private MaryTTSController tts_controller;

    void Awake() {
        this.tts_controller = GetComponent<MaryTTSController>();
    }

	public void SaySomething()
	{
		if (this.tts_controller.IsMaryTTSspeaking ()) {
			return;
		}

		// Quick Dirty hack: reset the facial expression to normal before speaking.
		FacialExpressionsController faceCtrl = GetComponent<FacialExpressionsController>() ;
		if (faceCtrl) {
			faceCtrl.ClearFacialExpression() ;
		}

		// Select a random sentence and say it.
		int rnd_id = Random.Range (0, SENTENCES.Length);
		string sentence = SENTENCES [rnd_id];
		//Debug.Log(sentence);
		this.tts_controller.MaryTTSspeak(sentence);
	}

}
