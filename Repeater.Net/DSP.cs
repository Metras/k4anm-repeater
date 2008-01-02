/*
Copyright © 2007 Nik Martin, K4ANM <><

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;


namespace DSP
{
	public class Goertzel
	{

		private double coeff=0;
		private double Q1=0;
		private double Q2=0;
		private double sine=0;
		private double cosine=0;

		char[] testData;


		public Goertzel()
			{
			InitGoertzel();
			}

		private double _Frequency;
		public double Frequency
			{
			get
				{
				return _Frequency;
				}
			set
				{
				_Frequency = value;
				}
			}
		
		private double  _SamplingRate = 8000;
		public double  SamplingRate
			{
			get
				{
				return _SamplingRate;
				}
			set
				{
				_SamplingRate = value;
				}
			}

		private uint _BlockSize=205;
		public uint BlockSize
			{
			get
				{
				return _BlockSize;
				}
			set
				{
				_BlockSize = value;
				}
			}
	
		


/* Call this routine before every "block" (size=N) of samples. */
private void ResetGoertzel()
{
  Q2 = 0;
  Q1 = 0;
}

/* Call this once, to precompute the constants. */
public void InitGoertzel()
{
  int	k;
  double doubleN;
  double omega;

  doubleN = (double) _BlockSize;
  k = (int) (0.5 + ((doubleN * _Frequency) / _SamplingRate ));
  omega = (2.0 * System.Math.PI * k) / doubleN;
  sine = System.Math.Sin(omega);
  cosine = System.Math.Cos(omega);
  coeff = 2.0 * cosine;

  Console.WriteLine("For SAMPLING_RATE = " + _SamplingRate );
  Console.WriteLine(" _BlockSize = " + _BlockSize);
  Console.WriteLine(" and FREQUENCY = " + _Frequency );
  Console.WriteLine("k = " + k + " and coeff = " + coeff);

  ResetGoertzel();
}

/* Call this routine for every sample. */
private void ProcessSample(char sample)
{
  double Q0;
  Q0 = coeff * Q1 - Q2 + sample;
  Q2 = Q1;
  Q1 = Q0;
}


/* Basic Goertzel */
/* Call this routine after every block to get the complex result. */
public void GetRealImag(ref double realPart, ref double imagPart)
{
  realPart = (Q1 - Q2 * cosine);
  imagPart = (Q2 * sine);
}

/* Optimized Goertzel */
/* Call this after every block to get the RELATIVE magnitude squared. */
private double GetMagnitudeSquared()
{
  double result;

  result = Q1 * Q1 + Q2 * Q2 - Q1 * Q2 * coeff;
  return result;
}

/*** End of Goertzel-specific code, the remainder is test code. */

/* Synthesize some test data at a given frequency. */
private void Generate(double frequency)
{
  int	index;
  double	step;

  step = frequency * ((2.0 * System.Math.PI) / _SamplingRate);

  /* Generate the test data */
  for (index = 0; index < _BlockSize; index++)
  {
    testData[index] = (char) (100.0 * System.Math.Sin(index * step) + 100.0);
  }
}

/* Demo 1 */
public void GenerateAndTest(double frequency)
{
  int	index;

  double	magnitudeSquared;
  double	magnitude;
  double	real=0;
  double	imag=0;

  testData = new char[_BlockSize];
  Console.WriteLine("For test frequency " + frequency.ToString("G8"));
  Generate(frequency);

  /* Process the samples */
  for (index = 0; index < _BlockSize; index++)
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
public void GenerateAndTest2(double frequency)
{
  int	index;

  double	magnitudeSquared;
  double	magnitude;
  double	real=0;
  double	imag=0;
  testData = new char[_BlockSize];
  Console.WriteLine("Freq= " + frequency);
  Generate(frequency);

  /* Process the samples. */
  for (index = 0; index < _BlockSize; index++)
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




			
		}
	}

