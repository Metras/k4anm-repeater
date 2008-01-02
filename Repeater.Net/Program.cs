using System;
using System.Collections.Generic;
using System.Text;




namespace Repeater.Net
{
	class Program
	{



const double PI = System.Math.PI;


const double SAMPLING_RATE= 8000.0;	//8kHz
const double TARGET_FREQUENCY=	941.0	;//941 Hz
const uint N=205;	//Block size

		static double coeff;
		static double Q1;
		static double Q2;
		static double sine;
		static double cosine;

static char[] testData = new char[N];

/* Call this routine before every "block" (size=N) of samples. */
static void ResetGoertzel()
{
  Q2 = 0;
  Q1 = 0;
}

/* Call this once, to precompute the constants. */
static void InitGoertzel()
{
  int	k;
  double doubleN;
  double omega;

  doubleN = (double) N;
  k = (int) (0.5 + ((doubleN * TARGET_FREQUENCY) / SAMPLING_RATE));
  omega = (2.0 * PI * k) / doubleN;
  sine = System.Math.Sin(omega);
  cosine = System.Math.Cos(omega);
  coeff = 2.0 * cosine;

  Console.WriteLine("For SAMPLING_RATE = " + SAMPLING_RATE);
  Console.WriteLine(" N = "+ N);
  Console.WriteLine(" and FREQUENCY = " + TARGET_FREQUENCY);
  Console.WriteLine("k = " + k + " and coeff = " + coeff);

  ResetGoertzel();
}

/* Call this routine for every sample. */
		static void ProcessSample(char sample)
{
  double Q0;
  Q0 = coeff * Q1 - Q2 + sample;
  Q2 = Q1;
  Q1 = Q0;
}


/* Basic Goertzel */
/* Call this routine after every block to get the complex result. */
		static void GetRealImag(ref double realPart, ref double imagPart)
{
  realPart = (Q1 - Q2 * cosine);
  imagPart = (Q2 * sine);
}

/* Optimized Goertzel */
/* Call this after every block to get the RELATIVE magnitude squared. */
		static double GetMagnitudeSquared()
{
  double result;

  result = Q1 * Q1 + Q2 * Q2 - Q1 * Q2 * coeff;
  return result;
}

/*** End of Goertzel-specific code, the remainder is test code. */

/* Synthesize some test data at a given frequency. */
		static void Generate(double frequency)
{
  int	index;
  double	step;

  step = frequency * ((2.0 * PI) / SAMPLING_RATE);

  /* Generate the test data */
  for (index = 0; index < N; index++)
  {
    testData[index] = (char) (100.0 * System.Math.Sin(index * step) + 100.0);
  }
}

/* Demo 1 */
static void GenerateAndTest(double frequency)
{
  int	index;

  double	magnitudeSquared;
  double	magnitude;
  double	real=0;
  double	imag=0;

  Console.WriteLine("For test frequency " + frequency.ToString("G8"));
  Generate(frequency);

  /* Process the samples */
  for (index = 0; index < N; index++)
  {
    ProcessSample(testData[index]);
  }

  /* Do the "basic Goertzel" processing. */
  GetRealImag(ref real, ref imag);

  Console.WriteLine("real = " + real + " imag = " + imag);

  magnitudeSquared = real*real + imag*imag;
  Console.WriteLine("Relative magnitude squared = " + magnitudeSquared);
  magnitude = Math.Sqrt (magnitudeSquared);
  Console.WriteLine("Relative magnitude = "+ magnitude);
  Console.WriteLine();

  /* Do the "optimized Goertzel" processing */
  magnitudeSquared = GetMagnitudeSquared();
  Console.WriteLine("Opt Relative magnitude squared = " + magnitudeSquared);
  magnitude = Math.Sqrt(magnitudeSquared);
  Console.WriteLine("Opt Relative magnitude = " + magnitude);
  Console.WriteLine();

  ResetGoertzel();
}

/* Demo 2 */
		static void GenerateAndTest2(double frequency)
{
  int	index;

  double	magnitudeSquared;
  double	magnitude;
  double	real=0;
  double	imag=0;

  Console.WriteLine("Freq= " + frequency);
  Generate(frequency);

  /* Process the samples. */
  for (index = 0; index < N; index++)
  {
    ProcessSample(testData[index]);
  }

  /* Do the "standard Goertzel" processing. */
  GetRealImag(ref real, ref imag);

  magnitudeSquared = real*real + imag*imag;
  Console.WriteLine("rel mag^2=" + magnitudeSquared.ToString("G8"));
  magnitude = System.Math.Sqrt(magnitudeSquared);
  Console.WriteLine("rel mag=" + magnitude.ToString("G10"));

  ResetGoertzel();
}

static void Main(string[] args)
{
  double freq;

  InitGoertzel();

  /* Demo 1 */
  GenerateAndTest(TARGET_FREQUENCY - 250);
  GenerateAndTest(TARGET_FREQUENCY);
  GenerateAndTest(TARGET_FREQUENCY + 250);

  /* Demo 2 
  for (freq = TARGET_FREQUENCY - 300; freq <= TARGET_FREQUENCY + 300; freq += 15)
  {
    GenerateAndTest2(freq);
  }
	*/

}


			
		}
	}

