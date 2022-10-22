// See https://aka.ms/new-console-template for more information
using NDSP;
using System.Runtime.Intrinsics;

class Test
{
	public static void TraceComplex(Vector128<float>[] pReal,
		Vector128<float>[] pImaginary,
		uint uLength,
		float frequency)
	{
		int x = 0;
		
			for (int i = 0; i < uLength / 4; i++)
			{
                pReal[i]= Vector128.Create(
								      (float)Math.Cos(2.0 * Math.PI * (x ) * frequency / uLength),
									  (float)Math.Cos(2.0 * Math.PI * (x  + 1) * frequency / uLength),
									  (float)Math.Cos(2.0 * Math.PI * (x  + 2) * frequency / uLength),
									  (float)Math.Cos(2.0 * Math.PI * (x  + 3) * frequency / uLength));
									 
				
				pImaginary[i] = Vector128.Create(
									  (float)Math.Sin(2.0 * Math.PI * (x) * frequency / uLength),
									  (float)Math.Sin(2.0 * Math.PI * (x + 1) * frequency / uLength),
									  (float)Math.Sin(2.0 * Math.PI * (x + 2) * frequency / uLength),
									  (float)Math.Sin(2.0 * Math.PI * (x + 3) * frequency / uLength));
			x += 4;
			}

	}
	public static void Trace(Vector128<float>[] pReal, Vector128<float>[] pImaginary)
    {
		int x = 0;
			for (int i = 0; i < pReal.Length; i++)
			{
				Console.WriteLine("{0} : {1}  :  {2}", x, pReal[i].GetElement(0), pImaginary[i].GetElement(0));
				Console.WriteLine("{0} : {1}  :  {2}", x + 1, pReal[i].GetElement(1), pImaginary[i].GetElement(1));
				Console.WriteLine("{0} : {1}  :  {2}", x + 2, pReal[i].GetElement(2), pImaginary[i].GetElement(2));
				Console.WriteLine("{0} : {1}  :  {2}", x + 3, pReal[i].GetElement(3), pImaginary[i].GetElement(3));
				x += 4;
			}
	
	}
	public static void Main(string[] args)
    {

		uint uLength = 32;
		Vector128<float>[] pReal = new Vector128<float>[uLength / 4];
		Vector128<float>[] pImaginary = new Vector128<float>[uLength / 4];
		TraceComplex(pReal, pImaginary, uLength, 8.0f);
		Vector128<float>[] pUnityTable = new Vector128<float>[uLength / 2];
		DSP.FFTInitializeUnityTable(pUnityTable, uLength);
		Trace(pReal, pImaginary);
		DSP.FFT(pReal, pImaginary, pUnityTable, uLength, 1);
		Trace(pReal, pImaginary);

	}

	public void TestLength16()
    {
		Console.WriteLine("Hello, World!");
		uint uLength = 16;
		Vector128<float>[] pReal = new Vector128<float>[uLength / 4];
		Vector128<float>[] pImaginary = new Vector128<float>[uLength / 4];
		TraceComplex(pReal, pImaginary, uLength, 2.0f);
		Vector128<float>[] pUnityTable = new Vector128<float>[uLength / 2];
		DSP.FFTInitializeUnityTable(pUnityTable, uLength);
		Trace(pReal, pImaginary);
		DSP.FFT4(pReal, pImaginary, 4);
		Trace(pReal, pImaginary);
	}
}
