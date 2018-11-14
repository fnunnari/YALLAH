//
// Tools to parse the MaryTTS duration files and synthesise them into visemes

import haxe.ds.Vector;

// See: https://api.haxe.org/StringTools.html
using StringTools;

import haxe.Json;
import haxe.DynamicAccess;


class MaryTTSException {
    public var reason: String ;

    public function new(reason: String) {
        this.reason = reason ;
    }
}


typedef TTSInfo = {
    var default_viseme:String;
    var visemes:Array<String>;
    var map:Map<String,String>;
}

/** This class can be used to parse a MaryTTS realized durations file and also to generate
 * in real-time the weights of the blend shapes.
 */
class MaryTTSBlendSequencer {

    static var RAMP_DOWN_PROPORTION: Float = 1.5;
    static var DEFAULT_RAMP_UP_SPEED: Float = 10.0;  // How many units per second I change the blend shapes
    static var DEFAULT_RAMP_DOWN_SPEED: Float = DEFAULT_RAMP_UP_SPEED * RAMP_DOWN_PROPORTION;

    // The viseme will be brought to full weight in this fraction of its duration.
    // Range: [0.0,1.0]
    static var RAMP_UP_DURATION_PROPORTION: Float = 0.5;

    static var MIN_RAMP_SPEED: Float = 2.0 ;
    static var MAX_RAMP_SPEED: Float = 6.0;

    static var ANTICIPATION_SECS: Float = 0.3;

    // Assumed duration when no following phoneme is present (useful for the last of the sequence.
    static var DEFAULT_VISEME_DURATION_SECS: Float = 0.2;


    //
    // Set form the JSON configuration file
    //
    private var DEFAULT_VISEME: String ;
    // How to get them in Blender:
    // for kb in bpy.context.active_object.data.shape_keys.key_blocks:
    //     print(kb.name)
    private var VISEMES: Array<String> ;
    private var PHONEMES_MAP: Map<String,String> ;


    private var realized_times: Array<Float> = null;
    private var realized_visemes: Array<String> = null ;

    private var last_time: Float = -1 ;
    private var speak_start_time: Float = -1 ;
    private var position_tts: Int = 0 ;

    private var ready_to_speak: Bool  = false ;  // This will be set true after loading a sentence or resetting the timer.
    private var active_viseme: String = null ;

    private var ramp_up_speed: Float = DEFAULT_RAMP_UP_SPEED;
    private var ramp_down_speed: Float = DEFAULT_RAMP_DOWN_SPEED;



    public function new(tts_info_json: String) {

        //
        // Load the JSON file
        // var tts_info_str: String = sys.io.File.getContent(tts_info_filename);
        // Sys.println("File loaded") ;
        // Sys.println(tts_info_str) ;

        var tts_info = Json.parse(tts_info_json) ;
        // Sys.println(tts_info) ;

        //
        // Take the data from the json structure
        this.DEFAULT_VISEME = tts_info.default_viseme ;
        this.VISEMES = tts_info.visemes ;

        // Convert the map in the json into an "easy" map
        // https://stackoverflow.com/questions/51717684/haxe-how-to-convert-json-dynamic-structure-to-map
        this.PHONEMES_MAP = new Map<String, String>() ;
        for(field in Reflect.fields(tts_info.map)) {
            var d = Reflect.field(tts_info.map, field) ;
            this.PHONEMES_MAP[field] = d ;
        }


        trace("Consistency check...");
        // Consistency check: all the table values MUST be in the MOUTH_SHAPES set.
        if(this.VISEMES.indexOf(this.DEFAULT_VISEME) == -1) {
              throw new MaryTTSException('Default viseme $this.DEFAULT_VISEME is not listed.') ;
        }

        for (ph in this.PHONEMES_MAP.keys()) {
            var ms: String = this.PHONEMES_MAP[ph] ;
            if (ms != null && this.VISEMES.indexOf(ms) == -1) {
              throw new MaryTTSException('For phoneme $ph, target mouth shape $ms is not listed.') ;
            }
         }
        trace("Check done.");



    }

    public function getVisemes(): Vector<String> {
        var n_visemes: Int = this.get_viseme_count() ;
        var out: Vector<String> = new Vector<String>(n_visemes) ;
        for (i in 0...n_visemes) {
            out[i] = this.VISEMES[i];
        }
        return out ;
    }

    /** Returns the number of Visemes which this module expects to handle.*/
    public function get_viseme_count(): Int {
        return this.VISEMES.length;
    }

    public function stop_sequencer(): Void {
        this.active_viseme = null ;
        this.ready_to_speak = false;
    }

    public function is_speaking(): Bool {
        return this.ready_to_speak == true ;
    }

    /** Parse a MaryTTS realized durations file and fills object vectors for later sequencing.
     @param realized_durations: The MaryTTS durations file as single multi-line string.
     */
    public function parse_realized_durations(realized_durations: String): Void {

        this.realized_times = new Array<Float>();
        this.realized_visemes = new Array<String>();

        var parsed_durations: Array<String> = new Array<String>() ;
        var parsed_phonemes: Array<String> = new Array<String>() ;

        // storing phonemes and durations from marytts file to arrays
        for (line in realized_durations.split('\n')) {
            // trace("== " + line) ;
            if (line.length == 0) {  // Skip empty lines
                continue ;
            }
            if (line.charAt(0) == '#') {  // Skip comments
                continue ;
            }

            // trace("== " + line) ;

            var split_line: Array<String> = line.split(" ") ;
            if (split_line.length != 3) {
                throw new MaryTTSException("Expected 3 data per line. Found "+split_line.length+". Line: '" + line + "'");
            }

            parsed_durations.push(split_line[0]) ;
            parsed_phonemes.push(split_line[2]) ;
        }

        // assert len(parsed_durations) == len(parsed_phonemes)
        trace("PARSED ("+parsed_durations.length+") phonemes.") ;

        //
        // MAPPING PHONEMES
        // from the parsed code to the blend shape name
        var i: Int = 0 ;
        for(p in parsed_phonemes) {
            // p = p.lower()
            var dur: Float = Std.parseFloat(parsed_durations[i]) ;

            var shape: String ;
            if(PHONEMES_MAP.exists(p)) {
                var viseme:String = PHONEMES_MAP[p] ;
                if(viseme == "null") {
                    shape = null;
                } else {
                    shape = viseme;
                }
            } else {
                shape = DEFAULT_VISEME ;
            }

            // trace("Mapping '" + p + "' --> '"+shape+"' at "+dur);

            this.realized_times.push(dur) ;
            this.realized_visemes.push(shape) ;

            i += 1 ;
        }

        this.ready_to_speak = true ;

        trace("MAPPED (" + this.realized_visemes.length + "):") ;

    }


    /** Reset the internal timers. This must be called if you want to synthetise the same sentence
     * again without re-parsing the durations.
     */
    public function reset_timers(): Void {

        this.last_time = -1 ;
        this.speak_start_time = -1 ;
        this.position_tts = 0 ;
        this.ready_to_speak = true ;
        this.active_viseme = null ;
    }


    /** The high-frequency routine to call 30+ times per second.

        @param now the current time, in seconds.
        @param viseme_weights An array of current weights of the viseme blend shapes.
         Values will be updated according to the synthesis time.
         The size of this vector must be the same of the module static vector of visemes.
         Values will be clamped in the range [0,1].
        @returns None
     */
    public function update(now: Float, viseme_weights: Vector<Float>) {

        if (viseme_weights.length != VISEMES.length) {
            throw new MaryTTSException("Viseme weight vector has wrong size."
                                   + " Expected "+VISEMES.length+", found "+viseme_weights.length) ;
        }

        // If this is true, we are performing the first iteration.
        if (this.last_time == -1) {
            this.last_time = now ;
            this.speak_start_time = now ;
            this.position_tts = 0 ;
            return ;
        }

        // Setup time vsariables
        var elapsed_time: Float = now - this.speak_start_time ; // time elapsed
        var delta_time: Float = now - this.last_time ;

        this.last_time = now ;

        elapsed_time += ANTICIPATION_SECS ;


        //
        // UPDATE THE WEIGHTS VECTOR
        // We do this even if a sentence is not loaded, maybe just to bring all visemes to 0.0.
        for (i in 0...VISEMES.length) {
            var viseme: String = VISEMES[i] ;
            var current_weight: Float = viseme_weights[i] ;
            if (viseme == this.active_viseme) {  // This has to ramp up to 1.0
                var inc: Float = this.ramp_up_speed * delta_time ;
                current_weight += inc ;
                if(current_weight > 1.0) {
                    current_weight = 1.0 ;
                }

            } else {  // This has to ramp down to 0.0
                var dec: Float = - this.ramp_down_speed * delta_time ;
                current_weight += dec ;
                if(current_weight < 0.0) {
                  current_weight = 0.0 ;
                }

            }

            // update the weight
            viseme_weights[i] = current_weight ;
        }

        // If this is false, no sentence is loaded or it has been invalidated. Bail out.
        if(this.ready_to_speak == false) {
            return ;
        }

        //
        // CHECK WHICH PHONEME WE HAVE TO USE
        var phonemes_count: Int = this.realized_visemes.length ;  // how many actions

        if (this.position_tts < phonemes_count) {
            var time_to_wait = this.realized_times[this.position_tts] ;
            if(elapsed_time >= time_to_wait) {
                // Current viseme
                this.active_viseme = this.realized_visemes[this.position_tts] ;
                // And advance to the next phoneme
                // this.position_tts += 1 ;
                this.position_tts = this.position_tts + 1 ;

                // Look ahead time, and adjust ramp speeds
                var viseme_duration: Float = DEFAULT_VISEME_DURATION_SECS ;
                if (this.position_tts < phonemes_count) {
                    var next_time_to_wait: Float = this.realized_times[this.position_tts] ;
                    // print("ww={}, next {}".format(time_to_wait, next_time_to_wait))
                    viseme_duration = next_time_to_wait - time_to_wait ;
                }

                // How long has to last the ramp duration
                var ramp_up_duration: Float = viseme_duration * RAMP_UP_DURATION_PROPORTION ;
                // Ramp up speed. (Trivial formula, since the range is 0 to 1
                this.ramp_up_speed = 1 / ramp_up_duration ;
                // clamp :-)
                this.ramp_up_speed = this.ramp_up_speed < MIN_RAMP_SPEED ? MIN_RAMP_SPEED : (this.ramp_up_speed > MAX_RAMP_SPEED ? MAX_RAMP_SPEED : this.ramp_up_speed) ;
                this.ramp_down_speed = this.ramp_up_speed * RAMP_DOWN_PROPORTION ;

                // print("Switching to viseme {}, duration {}, ramp_up_dur={} -> speed={}".format(self.active_viseme, viseme_duration, ramp_up_duration, self.ramp_up_speed))


          }
        } else {
            // The last index has been reached. So reset everything and
            // declare that we are not ready to speak

            this.active_viseme = null ;
            var last_time_to_wait: Float = 0 ;

            if (this.realized_times.length > 0) {
                last_time_to_wait = this.realized_times[this.realized_times.length-1] ;
            }

            if (elapsed_time > (last_time_to_wait + 1.0)) {
                this.ready_to_speak = false ;
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

    }  // end update



    //
    // Local test functions
    //

    static function local_test() {

        trace("Instance...");
        var seq: MaryTTSBlendSequencer = new MaryTTSBlendSequencer("MaryTTS-PhonemeMap-MBLab1_6.json");
        // trace(seq);

        seq.stop_sequencer() ;

        trace("Reading durations...");
        var durations: String = sys.io.File.getContent('data/realized_duration.txt');
        seq.parse_realized_durations(durations) ;

        //
        //
        var n_visemes: Int = seq.get_viseme_count() ;
        var visemes_buffer: Vector<Float> = new Vector<Float>(n_visemes) ;
        // The vector MUST be manually initialised for some target platforms (Python included)
        for (i in 0...visemes_buffer.length) {
          visemes_buffer[i] = 0.0 ;
        }

        var simulated_time: Float = 0 ;
        for( i in 0...100) {
            seq.update(simulated_time, visemes_buffer) ;
            // trace('Sim time $simulated_time: $visemes_buffer') ;
            simulated_time += 0.05 ;
        }

        trace("Main finished.");

    }



    static function test_speed() {

        var NUM_ITER: Int = 100000 ;
        var SIM_TIME: Float = 4.0 ;

        var seq: MaryTTSBlendSequencer = new MaryTTSBlendSequencer("MaryTTS-PhonemeMap-MBLab1_6.json");
        var n_visemes: Int = seq.get_viseme_count() ;
        var visemes_buffer: Vector<Float> = new Vector<Float>(n_visemes) ;
        // The vector MUST be manually initialised for some target platforms (Python included)
        for (i in 0...visemes_buffer.length) {
          visemes_buffer[i] = 0.0 ;
        }

        var durations: String = sys.io.File.getContent('data/realized_duration.txt');
        seq.parse_realized_durations(durations) ;

        var iterations: Int = 0 ;
        var before: Float = Sys.time() ;

        for(s in 0...NUM_ITER) {
            var simulated_time: Float = 0 ;
            seq.reset_timers() ;
            while(simulated_time < SIM_TIME) {
                seq.update(simulated_time, visemes_buffer) ;
                // trace('Sim time $simulated_time: $visemes_buffer') ;
                simulated_time += 0.05 ;
                iterations += 1 ;
            }
        }

        var after: Float = Sys.time() ;

        trace("Simulated " + NUM_ITER + " sessions of " + SIM_TIME + " seconds") ;
        var elapsed: Float = after - before ;
        var iter_per_sec:Float = iterations / elapsed ;
        trace("Number of iterations: "+iterations);
        trace("Elapsed (secs): " + elapsed) ;
        trace("Iter / sec: " + iter_per_sec) ;

    }
    



    //
    // MAIN
    //


    // Run with:
    // > haxe -main MaryTTSBlendSequencer --interp
    static function main() {
        // Uncomment the following line to perform a small local execution test.
        local_test() ;
        // test_speed() ;
    }


}
