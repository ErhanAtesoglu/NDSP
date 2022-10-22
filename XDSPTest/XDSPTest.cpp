#include "pch.h"
#include "CppUnitTest.h"
#include "XDSP.h"
#include <iostream>
#include <string>
using namespace std;
using namespace Microsoft::VisualStudio::CppUnitTestFramework;
constexpr float XM_2PI = 6.283185307f;
namespace XDSPTest
{
	using XMVECTOR = DirectX::XMVECTOR;
	using FXMVECTOR = DirectX::FXMVECTOR;
	using GXMVECTOR = DirectX::GXMVECTOR;
	using CXMVECTOR = DirectX::CXMVECTOR;
	using XMFLOAT4A = DirectX::XMFLOAT4A;


	TEST_CLASS(XDSPTest)
	{
	private:
		void TraceSine(XMVECTOR* pReal,
			XMVECTOR* pImaginary,
			size_t uLength,
			float frequency)
		{
			int x = 0;
			for (size_t i = 0; i < uLength / 4; i++)
			{
				pReal[i] = { sin(XM_2PI * x * frequency / uLength),
								  sin(XM_2PI * (x + 1) * frequency / uLength),
								  sin(XM_2PI * (x + 2) * frequency / uLength),
								  sin(XM_2PI * (x + 3) * frequency / uLength)
				};
				pImaginary[i] = { 0.0f,0.0f,0.0f,0.0f };
				x += 4;
			}

		}
		void PrintTrace(XMVECTOR* pReal,
			XMVECTOR* pImaginary,
			size_t uLength)
		{
			int x = 0;
			for (size_t i = 0; i < uLength / 4; i++)
			{
				
				cout << x;
				x += 4;
			}

		}
	

	public:


		TEST_METHOD(TestUnityTable)
		{
			size_t uLength = 2048;
			XMVECTOR* pUnityTable = new XMVECTOR[uLength];
			XDSP::FFTInitializeUnityTable(pUnityTable, uLength);

		}

		TEST_METHOD(TestTraceSince)
		{
			size_t uLength = 2048;

			XMVECTOR* pReal = new XMVECTOR[uLength / 4];
			XMVECTOR* pImaginary = new XMVECTOR[uLength / 4];

			TraceSine(pReal, pImaginary, uLength, 440.0f);
		}

		TEST_METHOD(TestFFT)
		{
			size_t uLength = 32;

			XMVECTOR* pReal = new XMVECTOR[uLength / 4];
			XMVECTOR* pImaginary = new XMVECTOR[uLength / 4];
			XMVECTOR* pUnityTable = new XMVECTOR[uLength];
			XDSP::FFTInitializeUnityTable(pUnityTable, uLength);
			TraceSine(pReal, pImaginary, uLength, 440.0f);
			XDSP::FFT(pReal, pImaginary, pUnityTable, uLength, 1);
		}

		TEST_METHOD(TESTFFT4)
		{
			size_t uLength = 16;

			XMVECTOR* pReal = new XMVECTOR[uLength / 4];
			XMVECTOR* pImaginary = new XMVECTOR[uLength / 4];
			XMVECTOR* pUnityTable = new XMVECTOR[uLength / 2];
			//XDSP::FFTInitializeUnityTable(pUnityTable, uLength);
			TraceSine(pReal, pImaginary, uLength, 2.0f);
			XDSP::FFT4(pReal, pImaginary, 4);
			PrintTrace(pReal, pImaginary, uLength);
		}

		TEST_METHOD(TESTFFT8)
		{
			size_t uLength = 32;

			XMVECTOR* pReal = new XMVECTOR[uLength / 4];
			XMVECTOR* pImaginary = new XMVECTOR[uLength / 4];
			XMVECTOR* pUnityTable = new XMVECTOR[uLength / 2];
			XDSP::FFTInitializeUnityTable(pUnityTable, uLength);
			TraceSine(pReal, pImaginary, uLength, 2.0f);
			XDSP::FFT(pReal, pImaginary, pUnityTable, uLength,1);
			PrintTrace(pReal, pImaginary, uLength);
		}

	};
}
