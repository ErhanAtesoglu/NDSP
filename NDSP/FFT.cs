using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;



namespace NDSP
{

    public partial class DSP
    {

      
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
                ButterflyDIT4_4(ref pReal[n],
                                ref pReal[n + uStride],
                                ref pReal[n + uStride2],
                                ref pReal[n + uStride3],
                                ref pImaginary[n],
                                ref pImaginary[n + uStride],
                                ref pImaginary[n + uStride2],
                                ref pImaginary[n + uStride3],
                                pUnityTable,
                                (n & uStage_vectors_mask),
                                (uLength >> 2) + (n & uStage_vectors_mask),
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

        static public void FFT4(Vector128<float>[] pReal, Vector128<float>[] pImaginary, uint uCount)
        {
            for (uint uIndex = 0; uIndex < uCount; ++uIndex)
            {
                ButterflyDIT4_1(ref pReal[uIndex], ref pImaginary[uIndex]);
            }
        }

        private static void FFT8(Vector128<float>[] pReal, Vector128<float>[] pImaginary, uint uCount)
        {
            Vector128<float> wr1 = Vector128.Create(1.0f, 0.70710677f, 0.0f, -0.70710677f);
            Vector128<float> wi1 = Vector128.Create(0.0f, -0.70710677f, -1.0f, -0.70710677f);
            Vector128<float> wr2 = Vector128.Create(-1.0f, -0.70710677f, 0.0f, 0.70710677f);
            Vector128<float> wi2 = Vector128.Create(0.0f, 0.70710677f, 1.0f, 0.70710677f);

            for (uint uIndex = 0; uIndex < uCount; ++uIndex)
            {
                uint i = uIndex * 2;
                Vector128<float> oddsR = XMVectorPermute(1, 3, 5, 7, pReal[i], pReal[i+1]);
                Vector128<float> evensR = XMVectorPermute(0, 2, 4, 6, pReal[i], pReal[i+1]);
                Vector128<float> oddsI = XMVectorPermute(1, 3, 5, 7, pImaginary[i], pImaginary[i+1]);
                Vector128<float> evensI = XMVectorPermute(0, 2, 4, 6, pImaginary[i], pImaginary[i+1]);

                ButterflyDIT4_1(ref oddsR, ref oddsI);
                ButterflyDIT4_1(ref evensR, ref evensI);


                //vmulComplex(out Vector128<float> r, out Vector128<float> i, oddsR, oddsI, wr1, wi1);
                //pReal[0] = XMVectorAdd(evensR, r);
                //pImaginary[0] = XMVectorAdd(evensI, i);

                //vmulComplex(out r, out i, oddsR, oddsI, wr2, wi2);
                //pReal[1] = XMVectorAdd(evensR, r);
                //pImaginary[1] = XMVectorAdd(evensI, i);
            }
        }

        private static Vector128<float> XMVectorPermute(uint PermuteX, uint PermuteY, uint PermuteZ, uint PermuteW, Vector128<float> p1, Vector128<float> p2)
        {
      
            byte shuffle = Shuffle((byte)(PermuteW & 3), (byte)(PermuteZ & 3), (byte)(PermuteY & 3), (byte)(PermuteX & 3));
            bool WhichX = PermuteX > 3;
            bool WhichY = PermuteY > 3;
            bool WhichZ = PermuteZ > 3;
            bool WhichW = PermuteW > 3;
            return PermuteHelper(shuffle, WhichX, WhichY, WhichZ, WhichW, p1, p2);
        }
        private static Vector128<float> PermuteHelper(byte Shuffle, bool WhichX, bool WhichY, bool WhichZ, bool WhichW, Vector128<float> v1, Vector128<float> v2)

        {
            Vector128<float> selectMask = Vector128.Create(WhichX ? -1.0f : 0,
                                                          WhichY ? -1.0f : 0,
                                                          WhichZ ? -1.0f : 0,
                                                          WhichW ? -1.0f : 0);
            Vector128<float> shuffled1 = XM_PERMUTE_PS(v1, Shuffle);
            Vector128<float> shuffled2 = XM_PERMUTE_PS(v2, Shuffle);

            Vector128<float> masked1 = Sse2.AndNot(selectMask, shuffled1);
            Vector128<float> masked2 = Sse2.And(selectMask, shuffled2);

            return Sse2.Or(masked1, masked2);
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
                //ButterflyDIT4_4(pReal[uIndex * 4],
                //    pReal[uIndex * 4 + 1],
                //    pReal[uIndex * 4 + 2],
                //    pReal[uIndex * 4 + 3],
                //    pImaginary[uIndex * 4],
                //    pImaginary[uIndex * 4 + 1],
                //    pImaginary[uIndex * 4 + 2],
                //    pImaginary[uIndex * 4 + 3],
                //    UnityTable,
                //    0, 4,
                //    1, true);
            }
        }


  

        private static void ButterflyDIT4_1(ref Vector128<float> r1, ref Vector128<float> i1)
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

            Vector128<float> rTemp = XMVectorMultiplyAdd(r1H, vDFT4SignBits1, r1L);
            Vector128<float> iTemp = XMVectorMultiplyAdd(i1H, vDFT4SignBits1, i1L);

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

            r1 = XMVectorMultiplyAdd(rZiWrZiW, vDFT4SignBits2, rTempL);
            i1 = XMVectorMultiplyAdd(iZrWiZrW, vDFT4SignBits3, iTempL);

        }

        private static void ButterflyDIT4_4(ref Vector128<float> r0,
                                            ref Vector128<float> r1,
                                            ref Vector128<float> r2,
                                            ref Vector128<float> r3,
                                            ref Vector128<float> i0,
                                            ref Vector128<float> i1,
                                            ref Vector128<float> i2,
                                            ref Vector128<float> i3,
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
            // first one is always trivial
            //  vmulComplex(rTemp0, iTemp0, rTemp0, iTemp0, pUnityTableReal[0], pUnityTableImaginary[0]); 
            vmulComplex(rTemp5, iTemp5, UnityTable[uReal + uStride], UnityTable[uImaginary + uStride]);
            vmulComplex(rTemp6, iTemp6, UnityTable[uReal + uStride * 2], UnityTable[uImaginary + uStride * 2]);
            vmulComplex(rTemp7, iTemp7, UnityTable[uReal + uStride * 3], UnityTable[uImaginary + uStride * 3]);

            if (fLast)
            {
                ButterflyDIT4_1(ref rTemp4, ref iTemp4);
                ButterflyDIT4_1(ref rTemp5, ref iTemp5);
                ButterflyDIT4_1(ref rTemp6, ref iTemp6);
                ButterflyDIT4_1(ref rTemp7, ref iTemp7);
            }

            r0 = rTemp4; i0 = iTemp4;
            r1 = rTemp5; i1 = iTemp5;
            r2 = rTemp6; i2 = iTemp6;
            r3 = rTemp7; i3 = iTemp7;
        }



       
    }


}
