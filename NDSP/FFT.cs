using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;



namespace NDSP
{

    public class DSP
    {
        static float XM_PI = 3.141592654f;
        static float XM_PIDIV2 = 1.570796327f;
        static float XM_1DIV2PI = 0.159154943f;
        static Vector128<float> g_XMAbsMask = Vector128.Create(0x7FFFFFFF).AsSingle();
        static Vector128<float> g_XMNegativeZero = Vector128.Create(0x80000000).AsSingle();
        static Vector128<float> g_XMNegativeOne = Vector128.Create(-1.0f);
        static Vector128<float> g_XMNoFraction = Vector128.Create(8388608.0f);
        static Vector128<float> g_XMZero = Vector128.Create(0.0f);
        static Vector128<float> g_XMOne = Vector128.Create(1.0f);
        static Vector128<float> g_XMFour = Vector128.Create(4.0f);
        private static Vector128<float> g_XMPi = Vector128.Create(XM_PI);
        private static Vector128<float> g_XMHalfPi = Vector128.Create(XM_PIDIV2);
        static Vector128<float> g_XMSinCoefficients0 =
            Vector128.Create(-0.16666667f, +0.0083333310f, -0.00019840874f, +2.7525562e-06f);
        static Vector128<float> g_XMSinCoefficients1 =
            Vector128.Create(-2.3889859e-08f, -0.16665852f /*Est1*/, +0.0083139502f /*Est2*/, -0.00018524670f /*Est3*/);
        static Vector128<float> g_XMCosCoefficients0 =
            Vector128.Create(-0.5f, +0.041666638f, -0.0013888378f, +2.4760495e-05f);
        static Vector128<float> g_XMCosCoefficients1 =
            Vector128.Create(-2.6051615e-07f, -0.49992746f /*Est1*/, +0.041493919f /*Est2*/, -0.0012712436f /*Est3*/ );
        private static Vector128<float> g_XMReciprocalTwoPi;

        public static Vector128<float> g_XMTwoPi { get; private set; }

        public static void FFT(Vector128<float>[] pReal,
                        Vector128<float>[] pImaginary,
                        Vector128<float>[] pUnityTable,
                        uint uLength, uint uCount)
        {

            uint uTotal = uCount * uLength;
            uint uTotal_vectors = uTotal >> 2;
            uint uStage_vectors = uLength >> 2;
            uint uStage_vectors_mask = uStage_vectors - 1;
            uint uStride = uLength >> 4;
            uint uStrideMask = uStride - 1;
            uint uStride2 = uStride * 2;
            uint uStride3 = uStride * 3;
            uint uStrideInvMask = ~uStrideMask;

            for (uint uIndex = 0; uIndex < (uTotal_vectors >> 2); ++uIndex)
            {
                uint n = ((uIndex & uStrideInvMask) << 2) + (uIndex & uStrideMask);
                ButterflyDIT4_4(pReal[n],
                                pReal[n + uStride],
                                pReal[n + uStride2],
                                pReal[n + uStride3],
                                pImaginary[n],
                                pImaginary[n + uStride],
                                pImaginary[n + uStride2],
                                pImaginary[n + uStride3],
                                pUnityTable,
                                (n & uStage_vectors_mask),
                                ((n >> 2) & uStage_vectors_mask),
                                uStride, false);
            }

            if (uLength > 16 * 4)
            {
                FFT(pReal, pImaginary, pUnityTable, uLength >> 2, uCount * 4);
            }
            else if (uLength == 16 * 4)
            {
                FFT16(pReal, pImaginary, uCount * 4);
            }
            else if (uLength == 8 * 4)
            {
                FFT8(pReal, pImaginary, uCount * 4);
            }
            else if (uLength == 4 * 4)
            {
                FFT4(pReal, pImaginary, uCount * 4);
            }

        }

        private static void FFT4(Vector128<float>[] pReal, Vector128<float>[] pImaginary, uint v)
        {
            throw new NotImplementedException();
        }

        private static void FFT8(Vector128<float>[] pReal, Vector128<float>[] pImaginary, uint v)
        {
            throw new NotImplementedException();
        }

        private static void FFT16(Vector128<float>[] pReal, Vector128<float>[] pImaginary, uint uCount)
        {
                Vector128<float>[] UnityTable = {
                Vector128.Create(1.0f, 1.0f, 1.0f, 1.0f ),
                Vector128.Create(1.0f, 0.92387950f, 0.70710677f, 0.38268343f ),
                Vector128.Create(1.0f, 0.70710677f, -4.3711388e-008f, -0.70710677f ),
                Vector128.Create(1.0f, 0.38268343f, -0.70710677f, -0.92387950f),
                Vector128.Create( -0.0f, -0.0f, -0.0f, -0.0f ),
                Vector128.Create( -0.0f, -0.38268343f, -0.70710677f, -0.92387950f),
                Vector128.Create( -0.0f, -0.70710677f, -1.0f, -0.70710677f),
                Vector128.Create( -0.0f, -0.92387950f, -0.70710677f, 0.38268343f)
            };


            for (uint uIndex = 0; uIndex < uCount; ++uIndex)
            {
                ButterflyDIT4_4(pReal[uIndex * 4],
                    pReal[uIndex * 4 + 1],
                    pReal[uIndex * 4 + 2],
                    pReal[uIndex * 4 + 3],
                    pImaginary[uIndex * 4],
                    pImaginary[uIndex * 4 + 1],
                    pImaginary[uIndex * 4 + 2],
                    pImaginary[uIndex * 4 + 3],
                    UnityTable,
                    0, 4,
                    1, true);
            }
        }


        private Vector128<float> XMVectorReplicate(float Value)
        {

            return Vector128.Create(Value);
        }

        private static void vmulComplex(Vector128<float> r1, Vector128<float> i1, Vector128<float> r2, Vector128<float> i2)
        {
            Vector128<float> vr1r2 = XMVectorMultiply(r1, r2);
            Vector128<float> vr1i2 = XMVectorMultiply(r1, i2);
            r1 = XMVectorFuseMultiplySubtract(i1, i2, vr1r2);
            i1 = XMVectorFuseMultiplyAdd(i1, i2, vr1i2);

        }



        private static Vector128<float> XMVectorAdd(Vector128<float> v0, Vector128<float> v1)
        {
            return Sse2.Add(v0, v1);
        }

        private static Vector128<float> XMVectorMultiply(Vector128<float> v0, Vector128<float> v1)
        {
            return Sse2.Multiply(v0, v1);
        }

        private static Vector128<float> XMVectorFuseMultiplyAdd(Vector128<float> v0, Vector128<float> v1, Vector128<float> v2)
        {
            return Sse2.Add(v2, Sse2.Multiply(v0, v1));
        }

        private static Vector128<float> XMVectorFuseMultiplySubtract(Vector128<float> v0, Vector128<float> v1, Vector128<float> v2)
        {
            return Sse2.Subtract(v2, Sse2.Multiply(v0, v1));
        }
        private static Vector128<float> XMVectorSubtract(Vector128<float> v0, Vector128<float> v1)
        {
            return Sse2.Subtract(v0, v1);
        }

        private static void ButterflyDIT4_1(Vector128<float> r1, Vector128<float> i1)
        {
            // sign constants for radix-4 butterflies
            Vector128<float> vDFT4SignBits1 = Vector128.Create(1.0f, -1.0f, 1.0f, -1.0f);
            Vector128<float> vDFT4SignBits2 = Vector128.Create(1.0f, 1.0f, -1.0f, -1.0f);
            Vector128<float> vDFT4SignBits3 = Vector128.Create(1.0f, -1.0f, -1.0f, 1.0f);

            // calculating Temp
            // [r1X| r1X|r1Y| r1Y] + [r1Z|-r1Z|r1W|-r1W]
            // [i1X| i1X|i1Y| i1Y] + [i1Z|-i1Z|i1W|-i1W]
            Vector128<float> r1L = XMVectorSwizzle0011(r1);
            Vector128<float> r1H = XMVectorSwizzle2233(r1);

            Vector128<float> i1L = XMVectorSwizzle0011(i1);
            Vector128<float> i1H = XMVectorSwizzle2233(i1);

            Vector128<float> rTemp = XMVectorFuseMultiplyAdd(r1H, vDFT4SignBits1, r1L);
            Vector128<float> iTemp = XMVectorFuseMultiplyAdd(i1H, vDFT4SignBits1, i1L);

            // calculating Result
            // [rTempZ|rTempW|iTempZ|iTempW]
            Vector128<float> rZrWiZiW = XMVectorPermute2367(rTemp, iTemp);
            // [rTempZ|iTempW|rTempZ|iTempW]
            Vector128<float> rZiWrZiW = XMVectorSwizzle0303(rZrWiZiW);
            // [rTempZ|iTempW|rTempZ|iTempW]
            Vector128<float> iZrWiZrW = XMVectorSwizzle2121(rZrWiZiW);

            // [rTempX| rTempY| rTempX| rTempY] + [rTempZ| iTempW|-rTempZ|-iTempW]
            // [iTempX| iTempY| iTempX| iTempY] + // [iTempZ|-rTempW|-iTempZ| rTempW]
            Vector128<float> rTempL = XMVectorSwizzle0101(rTemp);
            Vector128<float> iTempL = XMVectorSwizzle0101(iTemp);

            r1 = XMVectorFuseMultiplyAdd(rZiWrZiW, vDFT4SignBits2, rTempL);
            i1 = XMVectorFuseMultiplyAdd(iZrWiZrW, vDFT4SignBits3, iTempL);

        }

        private static void ButterflyDIT4_4(Vector128<float> r0,
                                            Vector128<float> r1,
                                            Vector128<float> r2,
                                            Vector128<float> r3,
                                            Vector128<float> i0,
                                            Vector128<float> i1,
                                            Vector128<float> i2,
                                            Vector128<float> i3,
                                            Vector128<float>[] UnityTable,
                                            uint uReal,
                                            uint uImaginary,
                                            uint uStride,
                                            bool fLast)
        {
            // calculating Temp
            Vector128<float> rTemp0 = XMVectorAdd(r0, r2);
            Vector128<float> iTemp0 = XMVectorAdd(i0, i2);

            Vector128<float> rTemp2 = XMVectorAdd(r1, r3);
            Vector128<float> iTemp2 = XMVectorAdd(i1, i3);

            Vector128<float> rTemp1 = XMVectorSubtract(r0, r2);
            Vector128<float> iTemp1 = XMVectorSubtract(i0, i2);

            Vector128<float> rTemp3 = XMVectorSubtract(r1, r3);
            Vector128<float> iTemp3 = XMVectorSubtract(i1, i3);

            Vector128<float> rTemp4 = XMVectorAdd(rTemp0, rTemp2);
            Vector128<float> iTemp4 = XMVectorAdd(iTemp0, iTemp2);

            Vector128<float> rTemp5 = XMVectorAdd(rTemp1, iTemp3);
            Vector128<float> iTemp5 = XMVectorSubtract(iTemp1, rTemp3);

            Vector128<float> rTemp6 = XMVectorSubtract(rTemp0, rTemp2);
            Vector128<float> iTemp6 = XMVectorSubtract(iTemp0, iTemp2);

            Vector128<float> rTemp7 = XMVectorSubtract(rTemp1, iTemp3);
            Vector128<float> iTemp7 = XMVectorAdd(iTemp1, rTemp3);

            // calculating Result
            // vmulComplex(rTemp0, iTemp0, rTemp0, iTemp0, pUnityTableReal[0], pUnityTableImaginary[0]); // first one is always trivial
            vmulComplex(rTemp5, iTemp5, UnityTable[uReal + uStride], UnityTable[uImaginary + uStride]);
            vmulComplex(rTemp6, iTemp6, UnityTable[uReal + uStride * 2], UnityTable[uImaginary + uStride * 2]);
            vmulComplex(rTemp7, iTemp7, UnityTable[uReal + uStride * 3], UnityTable[uImaginary + uStride * 3]);

            if (fLast)
            {
                ButterflyDIT4_1(rTemp4, iTemp4);
                ButterflyDIT4_1(rTemp5, iTemp5);
                ButterflyDIT4_1(rTemp6, iTemp6);
                ButterflyDIT4_1(rTemp7, iTemp7);
            }

            r0 = rTemp4; i0 = iTemp4;
            r1 = rTemp5; i1 = iTemp5;
            r2 = rTemp6; i2 = iTemp6;
            r3 = rTemp7; i3 = iTemp7;
        }



        private static byte Shuffle(byte fp3, byte fp2, byte fp1, byte fp0)
        {
            return (byte)((fp3 << 6) | (fp2 << 4) | (fp1 << 2) | (fp0));
        }

        private static Vector128<float> XMVectorSwizzle0101(Vector128<float> v0)
        {
            return Sse2.MoveLowToHigh(v0, v0);
        }

        private static Vector128<float> XMVectorSwizzle2121(Vector128<float> v0)
        {
            return XM_PERMUTE_PS(v0, Shuffle(2, 1, 2, 1));
        }

        private static Vector128<float> XMVectorSwizzle0303(Vector128<float> v0)
        {
            return XM_PERMUTE_PS(v0, Shuffle(0, 3, 0, 3));
        }

        private static Vector128<float> XMVectorPermute2367(Vector128<float> v0, Vector128<float> v1)
        {
            return Sse2.UnpackHigh(v0, v1);
        }

        private static Vector128<float> XMVectorSwizzle2233(Vector128<float> v0)
        {
            return Sse2.UnpackHigh(v0, v0);
        }

        private static Vector128<float> XMVectorSwizzle0011(Vector128<float> v0)
        {
            return Sse2.UnpackLow(v0, v0);
        }

        public static void FFTInitializeUnityTable(Vector128<float>[] UnityTable, uint uLength)
        {
            Vector128<float> vXM0123 = Vector128.Create(0.0f, 1.0f, 2.0f, 3.0f);
            uLength >>= 2;
            Vector128<float> vlStep = Vector128.Create(XM_PIDIV2 / (float)uLength);
            uint i = 0;
            do
            {
                uLength >>= 2;
                Vector128<float> vJP = vXM0123;
                for (uint j = 0; j < uLength; ++j)
                {
                    Vector128<float> vSin, vCos;
                    Vector128<float> viJP, vlS;

                    UnityTable[i + j] = g_XMOne;
                    UnityTable[i + j + uLength * 4] = g_XMZero;

                    vlS = XMVectorMultiply(vJP, vlStep);
                    XMVectorSinCos(out vSin, out vCos, vlS);
                    UnityTable[i + j + uLength] = vCos;
                    UnityTable[i + j + uLength * 5] = XMVectorMultiply(vSin, g_XMNegativeOne);

                    viJP = XMVectorAdd(vJP, vJP);
                    vlS = XMVectorMultiply(viJP, vlStep);
                    XMVectorSinCos(out vSin, out vCos, vlS);
                    UnityTable[i + j + uLength * 2] = vCos;
                    UnityTable[i + j + uLength * 6] = XMVectorMultiply(vSin, g_XMNegativeOne);

                    viJP = XMVectorAdd(viJP, vJP);
                    vlS = XMVectorMultiply(viJP, vlStep);
                    XMVectorSinCos(out vSin, out vCos, vlS);
                    UnityTable[i + j + uLength * 3] = vCos;
                    UnityTable[i + j + uLength * 7] = XMVectorMultiply(vSin, g_XMNegativeOne);

                    vJP = XMVectorAdd(vJP, g_XMFour);
                }
                vlStep = XMVectorMultiply(vlStep, g_XMFour);
                i += uLength * 8;
            } while (uLength > 4);
        }

        private static Vector128<float> XMVectorModAngles(Vector128<float> Angles)
        {
            Vector128<float> vResult = XMVectorMultiply(Angles, g_XMReciprocalTwoPi);

            vResult = XMVectorRound(vResult);
            return XMVectorFuseMultiplyAdd(vResult, g_XMTwoPi, Angles);
        }

        private static Vector128<float> XMVectorMultiply(Vector128<float> angles, object g_XMReciprocalTwoPi)
        {
            throw new NotImplementedException();
        }

        private static Vector128<float> XMVectorRound(Vector128<float> V)
        {
            Vector128<float> sign = Sse2.And(V, g_XMNegativeZero);
            Vector128<float> sMagic = Sse2.Or(g_XMNoFraction, sign);
            Vector128<float> R1 = Sse.Add(V, sMagic);
            R1 = Sse2.Subtract(R1, sMagic);
            Vector128<float> R2 = Sse2.And(V, g_XMAbsMask);
            Vector128<float> mask = Sse2.CompareLessThanOrEqual(R2, g_XMNoFraction);
            R2 = Sse2.AndNot(mask, V);
            R1 = Sse2.And(R1, mask);

            return Sse2.Xor(R1, R2);
        }
        static Vector128<float> XM_PERMUTE_PS(Vector128<float> v, byte c)
        {
            return Sse2.Shuffle(v, v, c);
        }
        private static void XMVectorSinCos(out Vector128<float> vSin, out Vector128<float> vCos, Vector128<float> V)
        {
            Vector128<float> x = XMVectorModAngles(V);

            Vector128<float> sign = Sse2.And(x, g_XMNegativeZero);
            Vector128<float> c = Sse2.Or(g_XMPi, sign);
            Vector128<float> absx = Sse2.AndNot(sign, x);
            Vector128<float> rflx = Sse2.Subtract(c, x);
            Vector128<float> comp = Sse2.CompareLessThanOrEqual(absx, g_XMHalfPi);
            Vector128<float> select0 = Sse2.And(comp, x);
            Vector128<float> select1 = Sse2.AndNot(comp, rflx);

            x = Sse2.Or(select0, select1);
            select0 = Sse2.And(comp, g_XMOne);
            select1 = Sse2.AndNot(comp, g_XMNegativeOne);
            sign = Sse2.Or(select0, select1);

            Vector128<float> x2 = Sse2.Multiply(x, x);

            Vector128<float> SC1 = g_XMSinCoefficients1;
            Vector128<float> vConstantsB = XM_PERMUTE_PS(SC1, Shuffle(0, 0, 0, 0));
            Vector128<float> SC0 = g_XMSinCoefficients0;
            Vector128<float> vConstants = XM_PERMUTE_PS(SC0, Shuffle(3, 3, 3, 3));
            Vector128<float> Result = XMVectorFuseMultiplyAdd(vConstantsB, x2, vConstants);


            vConstants = XM_PERMUTE_PS(SC0, Shuffle(2, 2, 2, 2));
            Result = XMVectorFuseMultiplyAdd(Result, x2, vConstants);

            vConstants = XM_PERMUTE_PS(SC0, Shuffle(1, 1, 1, 1));
            Result = XMVectorFuseMultiplyAdd(Result, x2, vConstants);

            vConstants = XM_PERMUTE_PS(SC0, Shuffle(0, 0, 0, 0));
            Result = XMVectorFuseMultiplyAdd(Result, x2, vConstants);

            Result = XMVectorFuseMultiplyAdd(Result, x2, g_XMOne);
            Result = Sse2.Multiply(Result, x);
            vSin = Result;

            Vector128<float> CC1 = g_XMCosCoefficients1;
            vConstantsB = XM_PERMUTE_PS(CC1, Shuffle(0, 0, 0, 0));
            Vector128<float> CC0 = g_XMCosCoefficients0;
            vConstants = XM_PERMUTE_PS(CC0, Shuffle(3, 3, 3, 3));
            Result = XMVectorFuseMultiplyAdd(vConstantsB, x2, vConstants);


            vConstants = XM_PERMUTE_PS(CC0, Shuffle(2, 2, 2, 2));
            Result = XMVectorFuseMultiplyAdd(Result, x2, vConstants);

            vConstants = XM_PERMUTE_PS(CC0, Shuffle(1, 1, 1, 1));
            Result = XMVectorFuseMultiplyAdd(Result, x2, vConstants);

            vConstants = XM_PERMUTE_PS(CC0, Shuffle(0, 0, 0, 0));
            Result = XMVectorFuseMultiplyAdd(Result, x2, vConstants);

            Result = XMVectorFuseMultiplyAdd(Result, x2, g_XMOne);
            Result = Sse2.Multiply(Result, sign);
            vCos = Result;
        }
    }


}
