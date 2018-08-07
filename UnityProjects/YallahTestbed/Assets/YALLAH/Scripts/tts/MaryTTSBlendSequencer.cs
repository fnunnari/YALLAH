using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;

namespace HaxeSpeedTest
{

	public class MaryTTSException: Exception
	{
	    public MaryTTSException()
	    {
	    }

	    public MaryTTSException(string message)
	        : base(message)
	    {
	    }
	}

	public class MaryTTSBlendSequencer {

	    private const float RAMP_DOWN_PROPORTION = 1.5f;
	    private const float DEFAULT_RAMP_UP_SPEED = 10.0f; // How many units per second I change the blend shapes
	    private const float DEFAULT_RAMP_DOWN_SPEED = DEFAULT_RAMP_UP_SPEED * RAMP_DOWN_PROPORTION;

	    // The viseme will be brought to full weight in this fraction of its duration.
	    // Range: [0.0,1.0]
	    private const float RAMP_UP_DURATION_PROPORTION = 0.5f;

	    private const float MIN_RAMP_SPEED = 8.0f;
	    private const float MAX_RAMP_SPEED = 12.0f;

	    private const float ANTICIPATION_SECS = 0.3f;

	    // Assumed duration when no following phoneme is present (useful for the last of the sequence.
	    private const float DEFAULT_VISEME_DURATION_SECS = 0.2f;

		public static readonly string[] VISEMES = {
			"phoneme_a_01",
			"phoneme_a_02",
			"phoneme_b_01",
			"phoneme_b_02",
			"phoneme_c_01",
			"phoneme_c_02",
			"phoneme_d_01",
			"phoneme_e_01",
			"phoneme_f_01",
			"phoneme_g_01",
			"phoneme_i_01",
			"phoneme_i_02",
			"phoneme_k_01",
			"phoneme_l_01",
			"phoneme_m_01",
			"phoneme_n_01",
			"phoneme_o_01",
			"phoneme_o_02",
			"phoneme_p_01",
			"phoneme_q_01",
			"phoneme_r_01",
			"phoneme_r_02",
			"phoneme_s_01",
			"phoneme_t_01",
			"phoneme_u_01",
			"phoneme_w_01",
			"phoneme_z_01"
		};

		/** Returns the number of Visemes which this module expects to handle.*/
	    public static int get_viseme_count() {
	        return VISEMES.Length;
	    }

		static Dictionary<string, string> PHONEMES_MAP = new Dictionary<string, string>() {

			{"gi", "phoneme_g_01"} , // "G",
			{"ge", "phoneme_g_01"}, // "G",
			{"ji", "phoneme_g_01"}, // "G",
			{"c", "phoneme_g_01"}, // "G",
			{"il", "phoneme_l_01"}, //"Etc",  // "L",
			{"el", "phoneme_l_01"}, //"Etc",  // "L",
		// "n", "L",
			{"di", "phoneme_d_01"}, // "TH",
			{"eh", "phoneme_e_01"}, // "EH",
		// "i" , "EH",
		// "e" , "EE",
			{"ie", "phoneme_i_01"}, // "EE",
			{"ee", "phoneme_i_01"}, // "EE",
			{"sh", "phoneme_g_01"}, // "SH",
			{"sch", "phoneme_c_02"}, // "SH",
			{"s", "phoneme_e_01"}, // yes: "e" looks better // "S",
			{"fv", "phoneme_f_01"}, // "FV",
			{"fw", "phoneme_f_01"}, // "FV",
			{"er", "phoneme_r_01"}, // "R",
			{"o", "phoneme_o_01"}, // "O",
			{"O", "phoneme_o_01"}, // "O",
			{"ov", "phoneme_o_01"}, // "O",
			{"oo", "phoneme_u_01"}, // "OO",
			{"oh", "phoneme_u_01"}, // "OO",
			{"mb", "phoneme_m_01"}, // "MBP",
			{"p", "phoneme_p_01"}, // "MBP",
			{"ah", "phoneme_a_01"}, // "AH",
			{"a", "phoneme_a_01"}, // "AH",

			{"{", "phoneme_e_01"}, // "EH",
			{"@U", "phoneme_o_01"}, //"O",  // TODO -- split in O-OO, like in "goes"
			{"A", "phoneme_a_01"}, //"AH",
			{"AI", "phoneme_a_01"}, //"AH",  // TODO -- split to AH-EE
			{"aU", "phoneme_a_01"}, //"AH",  // TODO -- split into AH-OO
			{"b", "phoneme_b_02"}, //"MBP",
			{"d", "phoneme_d_01"}, //"",
			{"D", "phoneme_d_01"}, //"TH",   // Like in "the"
			{"dZ", "phoneme_d_01"},

			{"E", "phoneme_e_01"}, //"EH",
			{"EI", "phoneme_i_01"}, //"EE",

		// "h",
			{"i", "phoneme_i_01"}, //"EE",
			{"I", "phoneme_i_01"}, //"EE",
		// "k", ""
			{"m", "phoneme_m_01"}, //"MBP",
			{"f", "phoneme_f_01"}, //"FV",
		// "g", "", // This is guttural GH
			{"l", "phoneme_l_01"}, //"Etc",  // TODO - This will map to "L" when we implement tongue and collar muscles
		// "n", "", //
		// "N", "", //
			{"r", "phoneme_r_01"}, //"R",
			{"r=", "phoneme_r_01"}, //"R",
			{"S", "phoneme_c_01"}, //"SH",
		// "T", "TH",  // TODO - this will use tongue, when available.
		// "t", "", // not tongue between the teeth
			{"u", "phoneme_u_01"}, //"OO",
			{"v", "phoneme_b_01"}, //"FV",
			{"V", "phoneme_o_01"}, //"O",
			{"w", "phoneme_w_01"}, //"OO",
			{"z", "phoneme_z_01"}, //"S",

			{"_", null}   // "End of sequence"
		};

		private List<float> realized_times;
	    private List<string> realized_visemes;

	    private float last_time = -1;
	    private float speak_start_time = -1;
	    private int position_tts = 0;

	    private bool ready_to_speak  = false;  // This will be set true after loading a sentence or resetting the timer.
	    private string active_viseme = null;

	    private float ramp_up_speed = DEFAULT_RAMP_UP_SPEED;
	    private float ramp_down_speed = DEFAULT_RAMP_DOWN_SPEED;


		public void stop_sequencer() {
	        this.ready_to_speak = false;
	    }

	    public bool is_speaking() {
	        return this.ready_to_speak;
	    }

		/** Parse a MaryTTS realized durations file and fills object vectors for later sequencing.
	     @param realized_durations: The MaryTTS durations file as single multi-line string.
	     */
	    public void parse_realized_durations(string realized_durations) {

	        this.realized_times = new List<float>();
	        this.realized_visemes = new List<string>();

	        List<string> parsed_durations = new List<string>();
	        List<string> parsed_phonemes = new List<string>();

			string[] lines = realized_durations.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

	        // storing phonemes and durations from marytts file to arrays
	        foreach (string line in lines) {
	            // trace("== " + line) ;
	            if (line.Length == 0) {  // Skip empty lines
	                continue;
	            }
	            if (line[0] == '#') {  // Skip comments
	                continue;
	            }

	            // trace("== " + line) ;

	            string[] split_line = line.Split(' ');
	            if (split_line.Length != 3) {
	                throw new MaryTTSException("Expected 3 data per line. Found " + split_line.Length+". Line: '" + line + "'");
	            }

	            parsed_durations.Add(split_line[0]) ;
	            parsed_phonemes.Add(split_line[2]) ;
	        }

	        // assert len(parsed_durations) == len(parsed_phonemes)
	        Console.WriteLine("PARSED (" + parsed_durations.Count + ") phonemes.");

	        //
	        // MAPPING PHONEMES
	        // from the parsed code to the blend shape name
	        int i = 0;
	        foreach (string p in parsed_phonemes) {
	            // p = p.lower()
	            float dur = float.Parse( parsed_durations[i], CultureInfo.InvariantCulture);

				string shape = "";
				if (PHONEMES_MAP.TryGetValue (p, out shape)) {
					//Console.WriteLine("For key = \"tif\", value = {0}.", value);
				} else {
					shape = "Etc";
				}

	            Console.WriteLine("Mapping '" + p + "' --> '" + shape + "' at " + dur);

	            this.realized_times.Add(dur);
	            this.realized_visemes.Add(shape);

	            i++;
	        }

	        this.ready_to_speak = true;

	        Console.WriteLine("MAPPED (" + this.realized_visemes.Count + "):");
	    }

		/** Reset the internal timers. This must be called if you want to synthetise the same sentence
	     * again without re-parsing the durations.
	     */
	    public void reset_timers() {
	        this.last_time = -1;
	        this.speak_start_time = -1;
	        this.position_tts = 0;
	        this.ready_to_speak = true;
	        this.active_viseme = null;
	    }

		/** The high-frequency routine to call 30+ times per second.

	        @param now the current time, in seconds.
	        @param viseme_weights An array of current weights of the viseme blend shapes.
	         Values will be updated according to the synthesis time.
	         The size of this vector must be the same of the module static vector of visemes.
	         Values will be clamped in the range [0,1].
	        @returns None
	     */
	    public void update(float now, double[] viseme_weights) {

	        if (viseme_weights.Length != VISEMES.Length) {
	            throw new MaryTTSException("Viseme weight vector has wrong size."
	                                   + " Expected " + VISEMES.Length + ", found " + viseme_weights.Length) ;
	        }

	        if (this.ready_to_speak == false) {
	            return;
	        }

	        if (this.last_time == -1) {
	            this.last_time = now;
	            this.speak_start_time = now;
	            this.position_tts = 0;
	            return;
	        }

	        float elapsed_time = now - this.speak_start_time; // time elapsed
	        float delta_time = now - this.last_time;

	        this.last_time = now;

	        elapsed_time += ANTICIPATION_SECS ;

	        //
	        // CHECK WHICH PHONEME WE HAVE TO USE
	        int phonemes_count = this.realized_visemes.Count;  // how many actions

	        if (this.position_tts < phonemes_count) {
	            float time_to_wait = this.realized_times[this.position_tts];
	            if (elapsed_time >= time_to_wait) {
	                // Current viseme
	                this.active_viseme = this.realized_visemes[this.position_tts];
	                // And advance to the next phoneme
	                // this.position_tts += 1 ;
	                this.position_tts = this.position_tts + 1;

	                // Look ahead time, and adjust ramp speeds
	                float viseme_duration = DEFAULT_VISEME_DURATION_SECS;
	                if (this.position_tts < phonemes_count) {
	                    float next_time_to_wait = this.realized_times[this.position_tts];
	                    // print("ww={}, next {}".format(time_to_wait, next_time_to_wait))
	                    viseme_duration = next_time_to_wait - time_to_wait;
	                }

	                // How long has to last the ramp duration
	                float ramp_up_duration = viseme_duration * RAMP_UP_DURATION_PROPORTION;
	                // Ramp up speed. (Trivial formula, since the range is 0 to 1
	                this.ramp_up_speed = 1 / ramp_up_duration;
	                // clamp :-)
	                this.ramp_up_speed = this.ramp_up_speed < MIN_RAMP_SPEED
											? MIN_RAMP_SPEED
											: (this.ramp_up_speed > MAX_RAMP_SPEED ? MAX_RAMP_SPEED : this.ramp_up_speed);
	                this.ramp_down_speed = this.ramp_up_speed * RAMP_DOWN_PROPORTION;

	                // print("Switching to viseme {}, duration {}, ramp_up_dur={} -> speed={}".format(self.active_viseme, viseme_duration, ramp_up_duration, self.ramp_up_speed))


	          }
	        } else {
	            // The last index has been reached. So reset everything and
	            // declare that we are not ready to speak

	            this.active_viseme = null;
	            float last_time_to_wait = 0;

	            if (this.realized_times.Count > 0) {
	                last_time_to_wait = this.realized_times[this.realized_times.Count - 1];
	            }

	            if (elapsed_time > (last_time_to_wait + 1.0)) {
	                this.ready_to_speak = false;
	            }
	        }

	        //
	        // Debug - print nicely formatted table with blend weights
	        //tab_template = "{:3s} "
	        //tab_template += "{:.2f}  " * len(viseme_weights)
	        //print(tab_template.format(self.active_viseme if self.active_viseme is not None else "--", *viseme_weights))

	        // var debug_line: String = "" ;
	        // debug_line += '$now  ' ;
	        // debug_line += this.active_viseme != null ? this.active_viseme : "--" ;
	        // for(w in viseme_weights) {
	        //     debug_line += '  $w' ;
	        // }
	        // trace(debug_line) ;

	        //
	        // UPDATE THE WEIGHTS VECTOR
	        for (int i = 0; i < VISEMES.Length; i++) {
	            string viseme = VISEMES[i] ;
	            float current_weight = (float) viseme_weights[i] ;
	            if (viseme == this.active_viseme) {  // This has to ramp up to 1.0
	                float inc = this.ramp_up_speed * delta_time;
	                current_weight += inc;
	                if (current_weight > 1.0) {
	                    current_weight = 1.0f;
	                }

	            } else {  // This ha to ramp down to 0.0
	                float dec = - this.ramp_down_speed * delta_time;
	                current_weight += dec;
	                if (current_weight < 0.0) {
	                	current_weight = 0.0f;
	                }

	            }

	            // update the weight
	            viseme_weights[i] = current_weight;
	        }

	    }  // end update

	    static void Main(string[] args)
	    {
	        // Display the number of command line arguments:
	        test_speed();
	    }

	    public static void test_speed() {

	        int NUM_ITER = 100000;
	        float SIM_TIME = 4.0f;

	        MaryTTSBlendSequencer seq = new MaryTTSBlendSequencer();
	        int n_visemes = MaryTTSBlendSequencer.get_viseme_count() ;
	        double[] visemes_buffer = new double[n_visemes];
	        // The vector MUST be manually initialised for some target platforms (Python included)
	        for (int i = 0; i < visemes_buffer.Length; i++) {
	          visemes_buffer[i] = 0.0 ;
	        }

	        string durations = File.ReadAllText("realized_duration.txt");
	        seq.parse_realized_durations(durations);

	        int iterations = 0;
	        Stopwatch sw = new Stopwatch();
	        sw.Start();

	        for (int s = 0; s < NUM_ITER; s++) {
	            float simulated_time = 0 ;
	            seq.reset_timers();
	            while(simulated_time < SIM_TIME) {
	                seq.update(simulated_time, visemes_buffer) ;
	                // trace('Sim time $simulated_time: $visemes_buffer') ;
	                simulated_time += 0.05f;
	                iterations += 1 ;
	            }
	        }

	        sw.Stop();

	        Console.WriteLine("Simulated " + NUM_ITER + " sessions of " + SIM_TIME + " seconds") ;
	        double iter_per_sec = iterations / sw.Elapsed.TotalSeconds;
	        Console.WriteLine("Number of iterations: " + iterations);
	        Console.WriteLine("Elapsed (secs): " + sw.Elapsed.TotalSeconds) ;
	        Console.WriteLine("Iter / sec: " + iter_per_sec) ;
	    }

	}

}