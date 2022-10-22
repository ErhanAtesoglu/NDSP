// CPPFFT.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "XDSP.h"
#define M_PI       3.14159265358979323846f   // pi

using XMVECTOR = DirectX::XMVECTOR;
using FXMVECTOR = DirectX::FXMVECTOR;
using GXMVECTOR = DirectX::GXMVECTOR;
using CXMVECTOR = DirectX::CXMVECTOR;
using XMFLOAT4A = DirectX::XMFLOAT4A;

void TraceComplex(XMVECTOR* pReal,
	XMVECTOR* pImaginary,
	size_t uLength,
	float frequency)
{
	int x = 0;
	for (size_t i = 0; i < uLength / 4; i++)
	{
		pReal[i] = {
			cos(2.0f * M_PI * x * frequency / uLength),
			cos(2.0f * M_PI * (x + 1) * frequency / uLength),
			cos(2.0f * M_PI * (x + 2) * frequency / uLength),
			cos(2.0f * M_PI * (x + 3) * frequency / uLength)
		};
		pImaginary[i] = {
			sin(2.0f * M_PI * x * frequency / uLength),
			sin(2.0f * M_PI * (x + 1) * frequency / uLength),
			sin(2.0f * M_PI * (x + 2) * frequency / uLength),
			sin(2.0f * M_PI * (x + 3) * frequency / uLength)
		};
		x += 4;
	}

}


void PrintComplex(XMVECTOR* pReal, XMVECTOR* pImaginary, size_t uLength)
{
	int x = 0;
	for (size_t i = 0; i < uLength / 4; i++)
	{
		std::cout << x + 0 << " :  " << pReal[i].m128_f32[0]
			<< "  :   " << pImaginary[i].m128_f32[0] << std::endl
			<< x + 1 << " :  " << pReal[i].m128_f32[1]
			<< "  :   " << pImaginary[i].m128_f32[1] << std::endl
			<< x + 2 << " :  " << pReal[i].m128_f32[2]
			<< "  :   " << pImaginary[i].m128_f32[2] << std::endl
			<< x + 3 << " :  " << pReal[i].m128_f32[3]
			<< "  :   " << pImaginary[i].m128_f32[3] << std::endl;

		x += 4;
	}

}

void PrintUnity(XMVECTOR* pUnity, size_t uLength)
{
	int x = 0;
	for (size_t i = 0; i < uLength / 4; i++)
	{
		std::cout << pUnity[i].m128_f32[0] << std::endl
			<< pUnity[i].m128_f32[1] << std::endl
			<< pUnity[i].m128_f32[2] << std::endl
			<< pUnity[i].m128_f32[3] << std::endl;

		x += 4;
	}

}
void TestLength16()
{
	size_t uLength = 16;

	XMVECTOR* pReal = new XMVECTOR[uLength / 4];
	XMVECTOR* pImaginary = new XMVECTOR[uLength / 4];
	XMVECTOR* pUnityTable = new XMVECTOR[uLength / 2];
	XMVECTOR* unswizzle = new XMVECTOR[uLength / 4];
	//	XDSP::FFTInitializeUnityTable(pUnityTable, uLength);
	TraceComplex(pReal, pImaginary, uLength, 2.0f);
	PrintComplex(pReal, pImaginary, uLength);
	XDSP::FFT4(pReal, pImaginary, 4);
	PrintComplex(pReal, pImaginary, uLength);

}

void TestLength32()
{

	size_t uLength = 32;

	XMVECTOR* pReal = new XMVECTOR[uLength / 4];
	XMVECTOR* pImaginary = new XMVECTOR[uLength / 4];
	XMVECTOR* pUnityTable = new XMVECTOR[uLength / 2];
	XMVECTOR* unswizzleR = new XMVECTOR[uLength / 4];
	XMVECTOR* unswizzleI = new XMVECTOR[uLength / 4];
	XDSP::FFTInitializeUnityTable(pUnityTable, uLength);
	TraceComplex(pReal, pImaginary, uLength, 8.0f);
	PrintComplex(pReal, pImaginary, uLength);
	XDSP::FFT(pReal, pImaginary, pUnityTable, uLength, 1);
	PrintComplex(pReal, pImaginary, uLength);
	//XDSP::FFTUnswizzle(unswizzleR, pReal, log2(uLength));
	//XDSP::FFTUnswizzle(unswizzleI, pImaginary, log2(uLength));
	//PrintComplex(unswizzleR, unswizzleI, uLength);
}

int main()
{
	TestLength32();
}
// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
