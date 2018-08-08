//
// Tools to implement a temporal blinking of the eyes.

import haxe.ds.Vector;


class BlinkException {
    public var reason: String ;

    public function new(reason: String) {
        this.reason = reason ;
    }
}

enum BlinkStatus {
    OPENING; CLOSING; WAITING ;
}

/** .
 */
class EyeBlinker {

    static var EYE_CLOSE_SPEED: Float = 8;  // The speed to close the eyes. In units/sec.
    static var EYE_OPEN_SPEED: Float = 10;

    // https://www.ncbi.nlm.nih.gov/pubmed/23403736
    static var MIN_DELAY: Float = 4.0 ;
    static var MAX_DELAY: Float = 8.0 ;


    static var VISEMES: Array<String> = [
          // For MBLab v1.5
          //'Expressions_eye02L_max',
          //'Expressions_eye02R_max'
          // For MBLab v1.6
          'Expressions_eyeClosedL_max',
          'Expressions_eyeClosedR_max'
            ] ;

    /** Returns the number of Visemes which this module expects to handle.*/
    static function get_viseme_count(): Int {
        return VISEMES.length;
    }

    private var last_time: Float = -1 ;

    private var current_weight: Float = 0.0 ;

    private var next_blink_time: Float = 0.0 ;
    private var blink_status: BlinkStatus = BlinkStatus.WAITING ;


    //
    // CONSTRUCTOR
    //
    public function new() {
    }


    /*
    public final int poisson(double a) {
        double limit = Math.exp(-a), prod = nextDouble();
        int n;
        for (n = 0; prod >= limit; n++)
            prod *= nextDouble();
        return n;
        }
        // nextDouble() is a function from the Random package in Java that returns a uniformly distributed random double, for example 0.885598042879084.
    */

    private static function poisson(lambda: Float): Int {
        var limit:Float = Math.exp(-lambda);
        var prod:Float = Math.random() ;

        var n:Int = 0 ;

        while(prod >= limit) {
            prod *= Math.random() ;
            n += 1 ;
        }

        return n ;

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
            throw new BlinkException("Viseme weight vector has wrong size."
                                   + " Expected "+VISEMES.length+", found "+viseme_weights.length) ;
        }

        if (this.last_time == -1) {
            this.last_time = now ;
            return ;
        }

        var delta_time: Float = now - this.last_time ;

        this.last_time = now ;

        //
        //
        if(this.blink_status == BlinkStatus.WAITING) {
            if(now >= this.next_blink_time) {
                this.blink_status = BlinkStatus.CLOSING ;
            }
        } else if(this.blink_status == BlinkStatus.CLOSING) {
            var delta_close: Float = EYE_CLOSE_SPEED * delta_time ;
            this.current_weight += delta_close ;
            if(this.current_weight >= 1.0) {
                this.current_weight = 1.0 ;
                this.blink_status = BlinkStatus.OPENING;
            }
        } else if(this.blink_status == BlinkStatus.OPENING) {
            var delta_open: Float = EYE_OPEN_SPEED * delta_time ;
            this.current_weight -= delta_open ;
            if(this.current_weight <= 0.0) {
                this.current_weight = 0.0 ;
                this.blink_status = BlinkStatus.WAITING ;
                // Compute next closing time
                var delay: Float = MIN_DELAY + (MAX_DELAY - MIN_DELAY) * Math.random() ;
                this.next_blink_time = now + delay ;
            }
        }

        //
        // UPDATE THE WEIGHTS VECTOR
        for (i in 0...VISEMES.length) {
            // var viseme: String = VISEMES[i] ;
            // update the weight
            viseme_weights[i] = this.current_weight ;
        }

    }  // end update



    //
    // MAIN
    //

    static function main() {
        // Uncomment the following line to execute a small local execution test.
        //test_speed() ;
        test_poisson() ;
    }

    static function test_speed() {

        var SIM_REAL_TIME: Float = 10.0 ;

        var blinker: EyeBlinker = new EyeBlinker();
        trace("Instance " + blinker);

        //
        //
        var n_visemes: Int = EyeBlinker.VISEMES.length ;
        var visemes_buffer: Vector<Float> = new Vector<Float>(n_visemes) ;
        // The vector MUST be manually initialised for some target platforms (Python included)
        for (i in 0...visemes_buffer.length) {
          visemes_buffer[i] = 0.0 ;
        }

        var simulated_time: Float = 0.0 ;

        var iterations: Int = 0 ;
        var before: Float = Sys.time() ;

        while(Sys.time() - before < SIM_REAL_TIME) {
            blinker.update(simulated_time, visemes_buffer) ;
            // trace('Sim time $simulated_time: $visemes_buffer') ;
            //trace(blinker.blink_status + "\t" + visemes_buffer[0] + "\t" + visemes_buffer[1]) ;
            simulated_time += 0.02 ;
            iterations += 1 ;
        }

        var after: Float = Sys.time() ;

        trace("Simulated " + SIM_REAL_TIME + " seconds") ;
        var elapsed: Float = after - before ;
        var iter_per_sec:Float = iterations / elapsed ;
        trace("Number of iterations: "+iterations);
        trace("Elapsed (secs): " + elapsed) ;
        trace("Iter / sec: " + iter_per_sec) ;

        trace("Main finished.");

    }

    static function test_poisson() {
        // see: http://www.cplusplus.com/reference/random/poisson_distribution/

        var sample_size: Int = 15 ;
        var n_rolls: Int = 1000;
        var max_plot_chars: Int = 150;

        var counters: Array<Int> = [for(i in 0...sample_size) 0] ; //new Array<Int>() ;

        trace("Randomizing...");

        for (i in 0...n_rolls) {
            var n:Int = EyeBlinker.poisson(0.5) ;
            if (n<sample_size) {
                counters[n] += 1 ;
            }
        }


        var max_count = 0 ;
        for (i in 0...counters.length) {
            var n:Int  = counters[i] ;
            if (n>max_count) { max_count = n ;}
        }

        var div = max_count / max_plot_chars ;

        //
        // "Plot" results
        for (i in 0...counters.length) {
            var n:Int = counters[i] ;
            Sys.print(i+"(" + n + "):\t") ;
            n = Std.int(n / div) ;
            for(j in 0...n) {
                Sys.print("*") ;
            }
            Sys.println("") ;
        }



    }

}
