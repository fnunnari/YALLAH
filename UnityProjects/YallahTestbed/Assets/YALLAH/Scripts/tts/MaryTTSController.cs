using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using haxe.root;
//using HaxeSpeedTest ;

/**
 * Blender blend shape value range: 0.0-1.0
 * Unity blend shape value range: 0.0-100.0
 */
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class MaryTTSController : MonoBehaviour {

	//
	// MaryTTS server info
	public enum MaryServer {
		LOCAL,
		DFKI,
		DECAD
	};
		
	private static Dictionary<MaryServer, string> server_addresses = new Dictionary<MaryServer, string>
	{
		{MaryServer.LOCAL, "localhost:59125"}, 
		{MaryServer.DFKI,  "mary.dfki.de:59125"}, 
		{MaryServer.DECAD,  "decad.sb.dfki.de:59125"},
	};

	public MaryServer server;

	//
	// MaryTTS voice info
	public enum MaryTTSVoice {
		DFKI_PRUDENCE,
		DFKI_PRUDENCE_HSMM,
		DFKI_POPPY,
		DFKI_POPPY_HSMM,
		CMU_SLT,
		CMU_SLT_HSMM,
		DFKI_OBADIAH,
		DFKI_OBADIAH_HSMM,
		DFKI_SPIKE,
		DFKI_SPIKE_HSMM,
		FR_UPMC_PIERRE_HSMM,
		FR_ENST_CAMILLE_HSMM
	};

	/** Small structure to hold voice name and its language. */
	private class VoiceInfo {
		public string voice;
		public string locale;

		public VoiceInfo(string voice, string locale) {
			this.voice = voice;
			this.locale = locale;
		}
	}


	private static readonly Dictionary<MaryTTSVoice, VoiceInfo> VOICES = new Dictionary<MaryTTSVoice, VoiceInfo>
	{
		{MaryTTSVoice.CMU_SLT, new VoiceInfo("cmu-slt", "en_US")},
		{MaryTTSVoice.CMU_SLT_HSMM,new VoiceInfo("cmu-slt-hsmm", "en_US")},
		{MaryTTSVoice.DFKI_PRUDENCE, new VoiceInfo("dfki-prudence", "en_GB")},
		{MaryTTSVoice.DFKI_PRUDENCE_HSMM, new VoiceInfo("dfki-prudence-hsmm", "en_GB")},
		{MaryTTSVoice.DFKI_POPPY, new VoiceInfo("dfki-poppy", "en_GB")},
		{MaryTTSVoice.DFKI_POPPY_HSMM, new VoiceInfo("dfki-poppy-hsmm", "en_GB")},
		{MaryTTSVoice.DFKI_OBADIAH, new VoiceInfo("dfki-obadiah", "en_GB")},
		{MaryTTSVoice.DFKI_OBADIAH_HSMM, new VoiceInfo("dfki-obadiah-hsmm", "en_GB")},
		{MaryTTSVoice.DFKI_SPIKE, new VoiceInfo("dfki-spike", "en_GB")},
		{MaryTTSVoice.DFKI_SPIKE_HSMM, new VoiceInfo("dfki-spike-hsmm", "en_GB")},
		{MaryTTSVoice.FR_UPMC_PIERRE_HSMM, new VoiceInfo("upmc-pierre-hsmm", "fr")},
		{MaryTTSVoice.FR_ENST_CAMILLE_HSMM, new VoiceInfo("enst-camille-hsmm", "fr")}
	} ;


	public MaryTTSVoice mary_tts_voice;

	// A global multiplier to amplify, attenuate mouth articulation.
	public double blendshapesMultiplier = 1.0;
	

	// URL composition instructions at: http://mary.dfki.de:59125/documentation.html
//	private const string MARY_TTS_AUDIO_PARAMETER = "&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=en_US";
//	private const string MARY_TTS_REALISED_DURATION_PARAMETER = "&OUTPUT_TYPE=REALISED_DURATIONS&LOCALE=en_US";
	private const string MARY_TTS_AUDIO_PARAMETER = "&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE";
	private const string MARY_TTS_REALISED_DURATION_PARAMETER = "&OUTPUT_TYPE=REALISED_DURATIONS";


	private SkinnedMeshRenderer skinnedMeshRenderer;
	private Mesh skinnedMesh;

    private MaryTTSBlendSequencer sequencer = new MaryTTSBlendSequencer("Assets/YALLAH/Scripts/tts/MaryTTS-Info-MBLab1_6.json");
    private double[] viseme_weights ;

	private AudioClip audioClip = null;


	#if UNITY_EDITOR

	[Header("Test:")]
	public bool saySomething ;

    private static readonly Dictionary<string, string[]> TEST_SENTENCES = new Dictionary<string, string[]> {
        {"en_US",
            new string [] {
                "The quick brown fox jumps over the lazy dog",
                 "Hello, how are you?"
            }
        },

        {"en_GB",
            new string [] {
                "The quick brown fox jumps over the lazy dog",
                "Hello, how are you?"
            }
        },

        {"fr",
            new string[] {
                "Bienvenue dans le monde de la synthèse de la parole!",
                "Bonjour, comment ça marche?"
            }
        }
    };

	#endif


	void Awake () {
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		skinnedMesh = GetComponent<SkinnedMeshRenderer> ().sharedMesh;
	}

	void Start () {
		Assert.IsNotNull(skinnedMeshRenderer); 
		Assert.IsNotNull(skinnedMesh);

        //
        // Check that all the phonemes required by the Sequencer are indeed present in the mesh.
        string[] needed_visemes = this.sequencer.getVisemes();
        for (int i = 0; i < needed_visemes.Length; i++) {
            string bs_name = needed_visemes[i];
            if( skinnedMesh.GetBlendShapeIndex(bs_name) == -1) {
                Debug.LogError("The BlendShape '" + bs_name + "' is required by TTS but missing in skinnedMesh.");
            }
        }

        this.viseme_weights = new double[this.sequencer.get_viseme_count()];
	}

	public void MaryTTSspeak(string text) {
		StartCoroutine (ProcessInputText (text));
	}

	private IEnumerator ProcessInputText(string text) {
		//text = "if everything goes right both individuals shake hands and go back to the world with a smile";
		// Debug.Log("Your sentence: " + text);

		yield return RetrieveMaryTTSdata(text);

		// Play audio
		AudioSource.PlayClipAtPoint(audioClip, transform.position);
		this.sequencer.reset_timers();
	}

	private IEnumerator RetrieveMaryTTSdata(string text) {
		text = text.Replace(" ", "+");


		String MARY_TTS_HTTP_ADDRESS = "http://" + MaryTTSController.server_addresses[this.server];
		// Debug.Log ("Composed address: " + MARY_TTS_HTTP_ADDRESS);

		VoiceInfo voice_info = MaryTTSController.VOICES[this.mary_tts_voice];

		String voice_parameter = "&VOICE=" + voice_info.voice;
		// Debug.Log("Selected voice: " + voice_parameter);

		String locale_parameter = "&LOCALE=" + voice_info.locale;

		string request_url = MARY_TTS_HTTP_ADDRESS + "/process?INPUT_TEXT=" + text + "&INPUT_TYPE=TEXT" + voice_parameter + locale_parameter;
		// Debug.Log("Request URL: " + request_url);
			
		// Fetch audio
		WWW audioResponse = new WWW(request_url + MARY_TTS_AUDIO_PARAMETER);
		yield return MaryTTWaitForRequest(audioResponse);

		this.audioClip = audioResponse.GetAudioClip(false, false, AudioType.WAV);

		// Fetch realised durations
		WWW rdurationsResponse = new WWW(request_url + MARY_TTS_REALISED_DURATION_PARAMETER);
		yield return MaryTTWaitForRequest (rdurationsResponse);

		lock (sequencer) {
//			string[] lines = rdurationsResponse.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
//			for (int i = 1; i < lines.Length; i++) {
//				Debug.Log (" === " + lines [i]);
//			}
			// Debug.Log(rdurationsResponse.text);
			sequencer.parse_realized_durations(rdurationsResponse.text);
		}

	}

	IEnumerator MaryTTWaitForRequest(WWW www) {
		yield return www;

		// Check for errors
		if (!string.IsNullOrEmpty(www.error)) {
            Debug.LogError("WWW error: " + www.error);
		}
	}
	
	// Update is called once per frame
	void Update() {


		#if UNITY_EDITOR
		if (this.saySomething) {
			VoiceInfo voice_info = MaryTTSController.VOICES[this.mary_tts_voice];
			string[] locale_sentences = TEST_SENTENCES[voice_info.locale] ;

			string to_say = locale_sentences[UnityEngine.Random.Range(0, locale_sentences.Length)] ;
			this.MaryTTSspeak (to_say);
			this.saySomething = false;
		}
		#endif


		this.sequencer.update(Time.time, this.viseme_weights);
		// Debug.Log (this.viseme_weights [0]);


        for (int i=0 ; i < this.sequencer.get_viseme_count() ; i++) {
            string viseme = (string)(this.sequencer.VISEMES [i]);

			int blendShapeIdx = this.skinnedMesh.GetBlendShapeIndex(viseme);
			// Debug.Log ("Looking for viseme " + viseme+". Index: " + blendShapeIdx);



			// Simple version
			double weight = this.viseme_weights[i] * 100.0 * this.blendshapesMultiplier;

			// Tries to soften movements at low values
			//double sw = this.viseme_weights[i] ;
			//sw = -2.0 * (sw * sw * sw) + 3.0 * (sw * sw);
			//double weight = sw * 100.0 * this.blendshapesMultiplier;

			skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIdx, (float) weight);
		}

	}

	//	
	public bool IsMaryTTSspeaking() {
		return this.sequencer.is_speaking();
	}

}
