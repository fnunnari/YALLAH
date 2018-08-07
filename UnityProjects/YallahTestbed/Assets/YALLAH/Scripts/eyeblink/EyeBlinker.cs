using System;
using System.Diagnostics;

namespace HaxeSpeedTest
{
	public class BlinkException : Exception
	{
		public BlinkException()
		{
		}

		public BlinkException(string message)
        : base(message)
    {
		}
	}

	enum BlinkStatus
	{
		OPENING,
		CLOSING,
		WAITING
	}

	public class EyeBlinker
	{
		private static float EYE_CLOSE_SPEED = 8.0f;
		private static float EYE_OPEN_SPEED = 10.0f;

		// https://www.ncbi.nlm.nih.gov/pubmed/23403736
		private static float MIN_DELAY = 4.0f;
		private static float MAX_DELAY = 8.0f;

		private static Random random = new Random();


		public static string[] VISEMES = new string[] {
//           "Expressions_eye02L_max",
//		   "Expressions_eye02R_max"
			// For MBLab v1.6
			"Expressions_eyeClosedL_max",
			"Expressions_eyeClosedR_max"
	    };

		/** Returns the number of Visemes which this module expects to handle.*/
		public static int get_viseme_count()
		{
			return VISEMES.Length;
		}

        private float last_time = -1.0f;

        private float current_weight = 0.0f;

        private float next_blink_time = 0.0f;
        private BlinkStatus blink_status = BlinkStatus.WAITING;

		/** The high-frequency routine to call 30+ times per second.

			@param now the current time, in seconds.
			@param viseme_weights An array of current weights of the viseme blend shapes.
			 Values will be updated according to the synthesis time.
			 The size of this vector must be the same of the module static vector of visemes.
			 Values will be clamped in the range [0,1].
			@returns None
		 */
		public void update(float now, double[] viseme_weights)
		{
			if (viseme_weights.Length != VISEMES.Length)
			{
				throw new BlinkException("Viseme weight vector has wrong size."
									   + " Expected " + VISEMES.Length + ", found " + viseme_weights.Length);
			}

            if (this.last_time == -1.0f)
			{
				this.last_time = now;
				return;
			}

			float delta_time = now - this.last_time;

			this.last_time = now;

			//
			//
			if (this.blink_status == BlinkStatus.WAITING)
			{
				if (now >= this.next_blink_time)
				{
					this.blink_status = BlinkStatus.CLOSING;
				}
			} else if (this.blink_status == BlinkStatus.CLOSING) {
				float delta_close = EYE_CLOSE_SPEED * delta_time;
				this.current_weight += delta_close;
				if (this.current_weight >= 1.0f)
				{
					this.current_weight = 1.0f;
					this.blink_status = BlinkStatus.OPENING;
				}
			} else if (this.blink_status == BlinkStatus.OPENING) {
                float delta_open = EYE_OPEN_SPEED * delta_time;
				this.current_weight -= delta_open;
				if (this.current_weight <= 0.0f)
				{
					this.current_weight = 0.0f;
					this.blink_status = BlinkStatus.WAITING;
					// Compute next closing time
					float delay = MIN_DELAY + (MAX_DELAY - MIN_DELAY) * (float) random.NextDouble();

					this.next_blink_time = now + delay;
				}
			}

			//
			// UPDATE THE WEIGHTS VECTOR
			for (int i = 0; i < VISEMES.Length; i++)
			{
				//string viseme = VISEMES[i];
				// update the weight
				viseme_weights[i] = this.current_weight;
			}

		}  // end update





		static void Main(string[] args)
		{
			// Display the number of command line arguments:
			test_speed();
		}

		public static void test_speed()
		{

			float SIM_REAL_TIME = 10.0f;

			EyeBlinker blinker = new EyeBlinker();
			Console.WriteLine("Instance " + blinker);

			//
			//
			int n_visemes = EyeBlinker.get_viseme_count();
			double[] visemes_buffer = new double[n_visemes];
			// The vector MUST be manually initialised for some target platforms (Python included)
			for (int i = 0; i < visemes_buffer.Length; i++)
			{
				visemes_buffer[i] = 0.0f;
			}

			float simulated_time = 0.0f;

			int iterations = 0;
			Stopwatch sw = new Stopwatch();
			sw.Start();

			while (sw.ElapsedMilliseconds / 1000 < SIM_REAL_TIME)
			{
				blinker.update(simulated_time, visemes_buffer);
				// trace(blinker.blink_status + "\t" + visemes_buffer[0] + "\t" + visemes_buffer[1]) ;
				simulated_time += 0.02f;
				iterations++;
			}

			sw.Stop();

			Console.WriteLine("Simulated " + SIM_REAL_TIME + " seconds");
			double iter_per_sec = iterations / sw.Elapsed.TotalSeconds;
			Console.WriteLine("Number of iterations: " + iterations);
			Console.WriteLine("Elapsed (secs): " + sw.Elapsed.TotalSeconds);
			Console.WriteLine("Iter / sec: " + iter_per_sec);

			Console.WriteLine("Main finished.");
		}

	}
}
