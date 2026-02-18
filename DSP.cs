/*
 *  DSP.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 07-02-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/  
 *  
 */

using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace SDR_DEV_APP
{
    // Типы демодуляции для всего приложения
    public enum DemodulationType { LSB, USB, AM, FM }

    public static class DSP
    {
        #region Спектры
        
        // Комплексный спектр (I + jQ) с FFTShift
        public static float[] ComputeComplexSpectrum(ReadOnlySpan<float> iSamples, ReadOnlySpan<float> qSamples, int fftSize)
        {
            if (iSamples.Length < fftSize || qSamples.Length < fftSize)
                throw new ArgumentException("Not enough samples for FFT");

            // 1. Применяем окно Ханна и формируем комплексный буфер
            Complex[] fftBuffer = new Complex[fftSize];
            double windowSum = 0.0;

            for (int k = 0; k < fftSize; k++)
            {
                double w = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * k / (fftSize - 1)));
                fftBuffer[k] = new Complex(iSamples[k] * w, qSamples[k] * w);
                windowSum += w;
            }

            // 2. FFT
            Fourier.Forward(fftBuffer, FourierOptions.Matlab);

            // 3. Преобразуем в dB с нормализацией
            const double epsilon = 1e-12;
            double scale = 1.0 / windowSum;
            float[] spectrumDB = new float[fftSize];

            for (int k = 0; k < fftSize; k++)
            {
                double mag = fftBuffer[k].Magnitude * scale;
                mag = Math.Max(mag, epsilon);
                spectrumDB[k] = (float)(20.0 * Math.Log10(mag));
            }

            // 4. FFTShift: ноль в центр
            float[] shifted = new float[fftSize];
            int half = fftSize / 2;
            Array.Copy(spectrumDB, half, shifted, 0, fftSize - half);
            Array.Copy(spectrumDB, 0, shifted, fftSize - half, half);
            return shifted;
        }

        // Спектр одного канала (Real I или Real Q)
        public static float[] ComputeRealSpectrum(ReadOnlySpan<float> samples, int fftSize)
        {
            if (samples.Length < fftSize)
                throw new ArgumentException("Not enough samples for FFT");

            int n = fftSize;
            int spectrumSize = n / 2 + 1;
            Complex[] fftBuffer = new Complex[n];
            double windowSum = 0.0;

            // Hann window
            for (int i = 0; i < n; i++)
            {
                double w = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (n - 1)));
                windowSum += w;
                fftBuffer[i] = new Complex(samples[i] * w, 0.0);
            }

            // FFT
            Fourier.Forward(fftBuffer, FourierOptions.Matlab);

            // Амплитуда → dB
            const double epsilon = 1e-12;
            double scale = 2.0 / windowSum;
            float[] spectrumDB = new float[spectrumSize];

            for (int k = 0; k < spectrumSize; k++)
            {
                double mag = fftBuffer[k].Magnitude;
                if (k != 0 && k != n / 2)
                    mag *= scale;
                else
                    mag /= windowSum; // DC и Nyquist не удваиваем

                mag = Math.Max(mag, epsilon);
                spectrumDB[k] = (float)(20.0 * Math.Log10(mag));
            }

            return spectrumDB;
        }

        // Анализ комплексного спектра: пик, частота, SNR
        public static (float peakDB, double peakFreqHz, float snrDB) AnalyzeFullSpectrum(
            ReadOnlySpan<float> spectrumDB, double sampleRate)
        {
            int n = spectrumDB.Length;
            float peakDB = float.MinValue;
            int peakIndex = 0;

            for (int k = 0; k < n; k++)
            {
                if (spectrumDB[k] > peakDB)
                {
                    peakDB = spectrumDB[k];
                    peakIndex = k;
                }
            }

            double df = sampleRate / n;
            double peakFreq = (peakIndex - n / 2) * df; // центрирование на 0 Гц

            // Оценка шума (исключаем ±2% вокруг пика)
            double noiseSum = 0;
            int noiseCount = 0;
            int excludeRange = Math.Max(1, n / 50);
            for (int k = 0; k < n; k++)
            {
                if (Math.Abs(k - peakIndex) > excludeRange)
                {
                    noiseSum += spectrumDB[k];
                    noiseCount++;
                }
            }

            float noiseDB = (noiseCount > 0) ? (float)(noiseSum / noiseCount) : peakDB - 40f;
            float snrDB = peakDB - noiseDB;

            return (peakDB, peakFreq, snrDB);
        }

        // Анализ Real-спектра
        public static (float peakDB, double peakFreqHz, float snrDB) AnalyzeRealSpectrum(
            ReadOnlySpan<float> spectrumDB, double sampleRate)
        {
            int n = (spectrumDB.Length - 1) * 2; // восстанавливаем fftSize
            float peakDB = float.MinValue;
            int peakIndex = 0;

            for (int k = 0; k < spectrumDB.Length; k++)
            {
                if (spectrumDB[k] > peakDB)
                {
                    peakDB = spectrumDB[k];
                    peakIndex = k;
                }
            }

            double df = sampleRate / n;
            double peakFreq = peakIndex * df;

            double noiseSum = 0;
            int noiseCount = 0;
            int excludeRange = Math.Max(1, spectrumDB.Length / 50);
            for (int k = 0; k < spectrumDB.Length; k++)
            {
                if (Math.Abs(k - peakIndex) > excludeRange)
                {
                    noiseSum += spectrumDB[k];
                    noiseCount++;
                }
            }

            float noiseDB = (noiseCount > 0) ? (float)(noiseSum / noiseCount) : peakDB - 40f;
            float snrDB = peakDB - noiseDB;

            return (peakDB, peakFreq, snrDB);
        }
        #endregion

        #region Фильтрация комплексного спектра
        
        // Функция Бесселя первого рода (для окна Кайзера)
        private static double BesselI0(double x)
        {
            double sum = 1.0, term = 1.0;
            for (int k = 1; k < 20; k++)
            {
                term *= (x * x) / (4.0 * k * k);
                sum += term;
                if (term < 1e-12) break;
            }
            return sum;
        }

        // Расчёт коэффициентов КИХ ФНЧ с плоской АЧХ
        public static float[] DesignFlatTopLpf(float cutoffHz, float sampleRate, int order)
        {
            float[] coeffs = new float[order + 1];
            float fc = cutoffHz / sampleRate; // Нормированная частота (0..0.5)
            float beta = 8.6f; // Окно Кайзера для 80+ дБ подавления

            // Идеальный ФНЧ (сэмплированный синус)
            for (int i = 0; i <= order; i++)
            {
                int n = i - order / 2;
                if (n == 0)
                    coeffs[i] = 2.0f * fc;
                else
                    coeffs[i] = (float)(2.0f * fc * Math.Sin(2.0f * Math.PI * fc * n) / (2.0f * Math.PI * fc * n));
            }

            // Применяем окно Кайзера
            double i0beta = BesselI0(beta);
            for (int i = 0; i <= order; i++)
            {
                float alpha = (i - order / 2.0f) / (order / 2.0f);
                float w = (float)(BesselI0(beta * Math.Sqrt(1.0 - alpha * alpha)) / i0beta);
                coeffs[i] *= w;
            }

            // Нормализация: сумма коэффициентов = 1.0 → плоская АЧХ в полосе пропускания
            float sum = 0.0f;
            for (int i = 0; i <= order; i++) sum += coeffs[i];
            for (int i = 0; i <= order; i++) coeffs[i] /= sum;

            return coeffs;
        }

        // Применение КИХ-фильтра с правильной обработкой кольцевого буфера
        public static void ApplyFlatTopLpf(Span<float> iSamples, Span<float> qSamples, ReadOnlySpan<float> coeffs, Span<float> filterBufferI, Span<float> filterBufferQ, ref int filterIndex)
        {
            // Защита: если фильтр не инициализирован — пропускаем
            if (coeffs.Length == 0 || iSamples.Length == 0) return;

            int order = coeffs.Length - 1; // 127 для 128-отсчётного фильтра
            int n = iSamples.Length;

            // Свёртка с использованием кольцевого буфера
            for (int i = 0; i < n; i++)
            {
                float iOut = 0.0f;
                float qOut = 0.0f;

                // Текущий сэмпл сохраняем в буфер ДО обработки
                int writeIdx = (filterIndex + i) % (order + 1);
                filterBufferI[writeIdx] = iSamples[i];
                filterBufferQ[writeIdx] = qSamples[i];

                // Свёртка: читаем из буфера в обратном порядке (от текущего к старым)
                for (int k = 0; k <= order; k++)
                {
                    int readIdx = (writeIdx - k + order + 1) % (order + 1);
                    iOut += filterBufferI[readIdx] * coeffs[k];
                    qOut += filterBufferQ[readIdx] * coeffs[k];
                }

                iSamples[i] = iOut;
                qSamples[i] = qOut;
            }

            // Обновляем индекс (не сбрасываем!)
            filterIndex = (filterIndex + n) % (order + 1);
        }
        #endregion

        #region Коррекции
        
        // Измерение DC без коррекции (для отображения при выключенной коррекции)
        public static (float iDc, float qDc) MeasureDcOffset(ReadOnlySpan<float> iSamples, ReadOnlySpan<float> qSamples)
        {
            float iDc = 0.0f;
            float qDc = 0.0f;
            for (int i = 0; i < iSamples.Length; i++)
            {
                iDc += iSamples[i];
                qDc += qSamples[i];
            }
            iDc /= iSamples.Length;
            qDc /= qSamples.Length;
            return (iDc, qDc);
        }

        // Коррекция DC смещения (in-place)
        public static (float iDc, float qDc) ApplyDcCorrection(Span<float> iSamples, Span<float> qSamples)
        {
            float iDc = 0.0f;
            float qDc = 0.0f;
            for (int i = 0; i < iSamples.Length; i++)
            {
                iDc += iSamples[i];
                qDc += qSamples[i];
            }
            iDc /= iSamples.Length;
            qDc /= qSamples.Length;
            // Вычитаем (in-place)
            for (int i = 0; i < iSamples.Length; i++)
            {
                iSamples[i] -= iDc;
                qSamples[i] -= qDc;
            }
            return (iDc, qDc);
        }

        // Амплитудная коррекция (балансировка усиления)
        // Умножает канал Q на коэффициент gainRatio для выравнивания амплитуд с каналом I
        public static void ApplyGainBalance(Span<float> qSamples, float gainRatio)
        {
            if (Math.Abs(gainRatio - 1.0f) < 0.001f) return; // пропускаем если коэффициент ≈1.0
            for (int i = 0; i < qSamples.Length; i++)
            {
                qSamples[i] *= gainRatio;
            }
        }

        // Фазовая коррекция: Q_corrected = Q - k·I
        // Применяется ПОСЛЕ амплитудной коррекции!
        public static void ApplyPhaseCorrection(Span<float> iSamples, Span<float> qSamples, float phaseCoeff)
        {
            if (Math.Abs(phaseCoeff) < 0.001f) return;
            for (int i = 0; i < qSamples.Length; i++)
            {
                qSamples[i] -= phaseCoeff * iSamples[i]; // Q = Q - k·I
            }
        }

        // Возвращает угол в градусах и коэффициент коррекции k = tan(φ)
        public static (float phaseDeg, float phaseCoeff) MeasurePhaseError(ReadOnlySpan<float> iSamples, ReadOnlySpan<float> qSamples)
        {
            double sumIQ = 0.0;
            double sumI2 = 0.0;
            double sumQ2 = 0.0;
            int n = Math.Min(iSamples.Length, qSamples.Length);
            for (int i = 0; i < n; i++)
            {
                float iVal = iSamples[i];
                float qVal = qSamples[i];
                sumIQ += iVal * qVal;
                sumI2 += iVal * iVal;
                sumQ2 += qVal * qVal;
            }
            // Защита от деления на ноль
            if (sumI2 < 1e-12 || sumQ2 < 1e-12)
                return (0.0f, 0.0f);
            // Нормированная корреляция: ρ = E[I·Q] / sqrt(E[I²]·E[Q²])
            double correlation = sumIQ / Math.Sqrt(sumI2 * sumQ2);
            correlation = Math.Max(-1.0, Math.Min(1.0, correlation)); // ограничиваем [-1, 1]
                                                                      // Фазовый сдвиг: φ = arcsin(ρ) — ТОЧНАЯ формула для квадратурных сигналов
            double phaseRad = Math.Asin(correlation);
            float phaseDeg = (float)(phaseRad * 180.0 / Math.PI);
            // Коэффициент для коррекции: k = tan(φ)
            float phaseCoeff = (float)Math.Tan(phaseRad);
            return (phaseDeg, phaseCoeff);
        }
        #endregion

        #region Демодуляция SSB (I/Q, комплексный FIR, плавная интерполяция)

        // Порядок FIR для SSB
        private const int DEMOD_SSB_ORDER = 508;

        // Кэш фильтра
        private static float[]? ssbHrCurrent = null;
        private static float[]? ssbHiCurrent = null;
        private static float[]? ssbHrTarget = null;
        private static float[]? ssbHiTarget = null;
        private static float ssbInterpAlpha = 1.0f;

        // Delay line / кольцевой буфер
        private static float[]? ssbBufI = null;
        private static float[]? ssbBufQ = null;
        private static int ssbIndex = 0;

        // Последние параметры фильтра
        private static float cachedBw = -1f;
        private static float cachedFs = -1f;
        private static DemodulationType cachedType;

        // Шаг интерполяции (0..1)
        private const float DEMOD_SSB_INTERP_STEP = 0.15f;

        // Демодуляция SSB (USB / LSB)
        public static void DemodulateSSB(
            ReadOnlySpan<float> iSamples,
            ReadOnlySpan<float> qSamples,
            float sampleRate,
            float centerFreqHz,
            float bandwidthHz,
            DemodulationType demodType,
            Span<float> audioOut,
            ref double phaseAccumulator)
        {
            int n = Math.Min(iSamples.Length, qSamples.Length);
            n = Math.Min(n, audioOut.Length);
            if (n <= 0) return;

            Span<float> mixI = n <= 8192 ? stackalloc float[n] : new float[n];
            Span<float> mixQ = n <= 8192 ? stackalloc float[n] : new float[n];

            // Комплексный сдвиг к нулю
            double dphi = 2.0 * Math.PI * centerFreqHz / sampleRate;
            double ph = phaseAccumulator;

            for (int i = 0; i < n; i++)
            {
                double c = Math.Cos(ph);
                double s = Math.Sin(ph);
                mixI[i] = (float)(iSamples[i] * c + qSamples[i] * s);
                mixQ[i] = (float)(qSamples[i] * c - iSamples[i] * s);
                ph += dphi;
            }

            phaseAccumulator = ph % (2.0 * Math.PI);
            if (phaseAccumulator < 0) phaseAccumulator += 2.0 * Math.PI;

            // Кэширование и интерполяция FIR
            EnsureSsbFIR(bandwidthHz, sampleRate, demodType);

            Span<float> fi = n <= 8192 ? stackalloc float[n] : new float[n];
            Span<float> fq = n <= 8192 ? stackalloc float[n] : new float[n];

            ApplySsbComplexFIR(mixI, mixQ, fi, fq,
                ssbHrCurrent!, ssbHiCurrent!, ssbBufI!, ssbBufQ!, ref ssbIndex);

            // Аудио = реальная часть аналитического сигнала
            for (int i = 0; i < n; i++)
                audioOut[i] = fi[i];
        }

        // Инициализация FIR SSB и плавная интерполяция
        private static void EnsureSsbFIR(float bw, float fs, DemodulationType type)
        {
            int N = DEMOD_SSB_ORDER - 1;
            int len = N + 1;
            int mid = N / 2;

            // Проверяем необходимость пересчёта
            bool changed = ssbHrCurrent == null ||
                           cachedBw != bw ||
                           cachedFs != fs ||
                           cachedType != type;

            if (changed)
            {
                // Создаем целевые коэффициенты
                float f1 = (type == DemodulationType.USB) ? 0f : -bw;
                float f2 = (type == DemodulationType.USB) ? bw : 0f;

                ssbHrTarget = new float[len];
                ssbHiTarget = new float[len];

                for (int n = 0; n < len; n++)
                {
                    int k = n - mid;
                    float w = 0.54f - 0.46f * (float)Math.Cos(2.0 * Math.PI * n / N);

                    if (k == 0)
                    {
                        ssbHrTarget[n] = 2f * (f2 - f1) / fs * w;
                        ssbHiTarget[n] = 0f;
                    }
                    else
                    {
                        float a = 2f * (float)Math.PI * f2 * k / fs;
                        float b = 2f * (float)Math.PI * f1 * k / fs;
                        ssbHrTarget[n] = (float)((Math.Sin(a) - Math.Sin(b)) / (Math.PI * k)) * w;
                        ssbHiTarget[n] = (float)((Math.Cos(b) - Math.Cos(a)) / (Math.PI * k)) * w;
                    }
                }

                // Первый запуск
                if (ssbHrCurrent == null)
                {
                    ssbHrCurrent = ssbHrTarget;
                    ssbHiCurrent = ssbHiTarget;
                    ssbBufI = new float[len];
                    ssbBufQ = new float[len];
                    ssbIndex = 0;
                    ssbInterpAlpha = 1.0f;
                }
                else
                {
                    ssbInterpAlpha = 0.0f;
                }

                cachedBw = bw;
                cachedFs = fs;
                cachedType = type;
            }

            // Интерполяция коэффициентов
            if (ssbInterpAlpha < 1.0f && ssbHrTarget != null && ssbHiTarget != null)
            {
                ssbInterpAlpha = Math.Min(1.0f, ssbInterpAlpha + DEMOD_SSB_INTERP_STEP);
                float a = ssbInterpAlpha;
                float b = 1.0f - a;

                for (int i = 0; i < len; i++)
                {
                    ssbHrCurrent![i] = ssbHrCurrent![i] * b + ssbHrTarget[i] * a;
                    ssbHiCurrent![i] = ssbHiCurrent![i] * b + ssbHiTarget[i] * a;
                }
            }
        }

        // Комплексная FIR свёртка для SSB
        private static void ApplySsbComplexFIR(
            ReadOnlySpan<float> inI,
            ReadOnlySpan<float> inQ,
            Span<float> outI,
            Span<float> outQ,
            float[] hR,
            float[] hI,
            float[] bufI,
            float[] bufQ,
            ref int idx)
        {
            int len = hR.Length;

            for (int n = 0; n < inI.Length; n++)
            {
                bufI[idx] = inI[n];
                bufQ[idx] = inQ[n];

                float ai = 0f, aq = 0f;
                int p = idx;

                for (int k = 0; k < len; k++)
                {
                    float ir = bufI[p];
                    float iq = bufQ[p];

                    ai += hR[k] * ir - hI[k] * iq;
                    aq += hR[k] * iq + hI[k] * ir;

                    if (--p < 0) p = len - 1;
                }

                outI[n] = ai;
                outQ[n] = aq;

                if (++idx >= len) idx = 0;
            }
        }

        #endregion

        #region Демодуляция AM (I/Q, комплексный FIR, плавная интерполяция)

        // Порядок FIR для выделения полосы AM
        private const int DEMOD_AM_LPF_ORDER = 127;

        // Кэш фильтра
        private static float[]? amLpfCoeffsCurrent = null;
        private static float[]? amLpfCoeffsTarget = null;
        private static float amLpfInterpAlpha = 1.0f;

        // Delay line / кольцевой буфер
        private static float[]? amLpfBufI = null;
        private static float[]? amLpfBufQ = null;
        private static int amLpfIndex = 0;

        // Последние параметры фильтра
        private static float cachedAmFs = -1f;
        private static float cachedAmCutoff = -1f;

        // Шаг интерполяции (0..1)
        private const float DEMOD_AM_INTERP_STEP = 0.15f;

        // Демодуляция AM (I/Q)
        // iSamples/qSamples - входной I/Q сигнал
        // sampleRate - частота дискретизации
        // centerFreqHz - центральная частота сигнала AM
        // bandwidthHz - ширина полосы AM
        // audioOut - выход аудио
        // phaseAccumulator - сохранение фазы сдвига
        public static void DemodulateAM(
            ReadOnlySpan<float> iSamples,
            ReadOnlySpan<float> qSamples,
            float sampleRate,
            float centerFreqHz,
            float bandwidthHz,
            Span<float> audioOut,
            ref double phaseAccumulator)
        {
            int n = Math.Min(iSamples.Length, qSamples.Length);
            n = Math.Min(n, audioOut.Length);
            if (n <= 0) return;

            // Буферы для сдвинутого сигнала
            Span<float> shiftedI = n <= 8192 ? stackalloc float[n] : new float[n];
            Span<float> shiftedQ = n <= 8192 ? stackalloc float[n] : new float[n];

            // 1. Комплексный сдвиг к центру полосы AM
            double dphi = 2.0 * Math.PI * centerFreqHz / sampleRate;
            double ph = phaseAccumulator;

            for (int i = 0; i < n; i++)
            {
                double c = Math.Cos(ph);
                double s = Math.Sin(ph);
                shiftedI[i] = (float)(iSamples[i] * c + qSamples[i] * s);
                shiftedQ[i] = (float)(qSamples[i] * c - iSamples[i] * s);
                ph += dphi;
            }

            phaseAccumulator = ph % (2.0 * Math.PI);
            if (phaseAccumulator < 0) phaseAccumulator += 2.0 * Math.PI;

            // 2. Инициализация FIR LPF и интерполяция
            EnsureAmLpf(sampleRate, bandwidthHz / 2.0f);

            // 3. Фильтрация I/Q с кольцевым буфером
            Span<float> fi = n <= 8192 ? stackalloc float[n] : new float[n];
            Span<float> fq = n <= 8192 ? stackalloc float[n] : new float[n];

            ApplyComplexFIR(shiftedI, shiftedQ, fi, fq,
                amLpfCoeffsCurrent!, amLpfCoeffsCurrent!,
                amLpfBufI!, amLpfBufQ!, ref amLpfIndex);

            // 4. Огибающая |Z| = sqrt(I^2 + Q^2)
            for (int i = 0; i < n; i++)
            {
                audioOut[i] = MathF.Sqrt(fi[i] * fi[i] + fq[i] * fq[i]);
            }

            // 5. Усиление (по желанию)
            const float GAIN = 1.0f;
            for (int i = 0; i < n; i++)
                audioOut[i] *= GAIN;
        }

        // Инициализация LPF для AM и интерполяция
        private static void EnsureAmLpf(float sampleRate, float cutoff)
        {
            int N = DEMOD_AM_LPF_ORDER;
            int len = N + 1;

            // Первый запуск
            if (amLpfCoeffsCurrent == null)
            {
                amLpfCoeffsCurrent = DesignLpf(cutoff, sampleRate, N);
                amLpfCoeffsTarget = amLpfCoeffsCurrent;
                amLpfInterpAlpha = 1.0f;

                amLpfBufI = new float[len];
                amLpfBufQ = new float[len];
                amLpfIndex = 0;

                cachedAmFs = sampleRate;
                cachedAmCutoff = cutoff;
                return;
            }

            // Проверяем, нужно ли менять фильтр
            bool changed = Math.Abs(cutoff - cachedAmCutoff) > 0.1f || cachedAmFs != sampleRate;
            if (changed)
            {
                amLpfCoeffsTarget = DesignLpf(cutoff, sampleRate, N);
                amLpfInterpAlpha = 0.0f;
                cachedAmFs = sampleRate;
                cachedAmCutoff = cutoff;
            }

            // Интерполяция коэффициентов
            if (amLpfInterpAlpha < 1.0f && amLpfCoeffsTarget != null)
            {
                amLpfInterpAlpha = Math.Min(1.0f, amLpfInterpAlpha + DEMOD_AM_INTERP_STEP);
                float a = amLpfInterpAlpha;
                float b = 1.0f - a;
                for (int i = 0; i < len; i++)
                    amLpfCoeffsCurrent[i] = amLpfCoeffsCurrent[i] * b + amLpfCoeffsTarget[i] * a;
            }
        }

        // Простая FIR LPF с окном Ханна
        private static float[] DesignLpf(float cutoffHz, float fs, int N)
        {
            int len = N + 1;
            float[] h = new float[len];
            int mid = N / 2;
            for (int n = 0; n < len; n++)
            {
                int k = n - mid;
                float w = 0.5f - 0.5f * MathF.Cos(2.0f * MathF.PI * n / N);
                if (k == 0)
                    h[n] = 2.0f * cutoffHz / fs * w;
                else
                    h[n] = (float)(Math.Sin(2.0 * Math.PI * cutoffHz * k / fs) / (Math.PI * k)) * w;
            }
            return h;
        }

        // Комплексная FIR свёртка для AM
        private static void ApplyComplexFIR(
            ReadOnlySpan<float> inI,
            ReadOnlySpan<float> inQ,
            Span<float> outI,
            Span<float> outQ,
            float[] hR,
            float[] hI,
            float[] bufI,
            float[] bufQ,
            ref int idx)
        {
            int len = hR.Length;
            for (int n = 0; n < inI.Length; n++)
            {
                bufI[idx] = inI[n];
                bufQ[idx] = inQ[n];

                float ai = 0f, aq = 0f;
                int p = idx;

                for (int k = 0; k < len; k++)
                {
                    float ir = bufI[p];
                    float iq = bufQ[p];
                    ai += hR[k] * ir - hI[k] * iq;
                    aq += hR[k] * iq + hI[k] * ir;
                    if (--p < 0) p = len - 1;
                }

                outI[n] = ai;
                outQ[n] = aq;

                if (++idx >= len) idx = 0;
            }
        }

        #endregion

        #region Демодуляция FM (I/Q, комплексный фазовый дискриминатор, плавная интерполяция)

        // Порядок FIR для аудиофильтра после FM-демодуляции
        private const int DEMOD_FM_LPF_ORDER = 127;

        // Кэш фильтра
        private static float[]? fmLpfCoeffsCurrent = null;
        private static float[]? fmLpfCoeffsTarget = null;
        private static float fmLpfInterpAlpha = 1.0f;

        // Delay line / кольцевой буфер
        private static float[]? fmLpfBuf = null;
        private static int fmLpfIndex = 0;

        // Последние параметры фильтра
        private static float cachedFmFs = -1f;
        private static float cachedFmCutoff = -1f;

        // Шаг интерполяции коэффициентов FIR
        private const float DEMOD_FM_INTERP_STEP = 0.15f;

        // Демодуляция FM (I/Q, комплексный фазовый дискриминатор)
        // iSamples/qSamples - входной I/Q сигнал
        // sampleRate - частота дискретизации
        // audioOut - выход аудио
        // lpfCutoff - частота среза LPF после демодуляции
        public static void DemodulateFM(
            ReadOnlySpan<float> iSamples,
            ReadOnlySpan<float> qSamples,
            float sampleRate,
            Span<float> audioOut,
            float lpfCutoff = 5000f)
        {
            int n = Math.Min(iSamples.Length, qSamples.Length);
            n = Math.Min(n, audioOut.Length);
            if (n <= 0) return;

            // Инициализация FIR LPF и плавная интерполяция
            EnsureFmLpf(sampleRate, lpfCutoff);

            // Начальные предыдущие значения для фазового дискриминатора
            float prevI = iSamples[0];
            float prevQ = qSamples[0];

            for (int i = 0; i < n; i++)
            {
                float iVal = iSamples[i];
                float qVal = qSamples[i];

                // Комплексный фазовый дискриминатор: Im(conj(prev) * curr) ≈ i*prevQ - q*prevI
                float audio = iVal * prevQ - qVal * prevI;

                prevI = iVal;
                prevQ = qVal;

                // Сохраняем сэмпл в кольцевой буфер FIR
                fmLpfBuf![fmLpfIndex] = audio;

                // FIR LPF для аудио
                float acc = 0f;
                int len = fmLpfCoeffsCurrent!.Length;
                int p = fmLpfIndex;
                for (int k = 0; k < len; k++)
                {
                    acc += fmLpfCoeffsCurrent[k] * fmLpfBuf[p];
                    if (--p < 0) p = len - 1;
                }

                audioOut[i] = acc;

                if (++fmLpfIndex >= len) fmLpfIndex = 0;
            }
        }

        // Инициализация FIR LPF для FM и плавная интерполяция
        private static void EnsureFmLpf(float sampleRate, float cutoff)
        {
            int N = DEMOD_FM_LPF_ORDER;
            int len = N + 1;

            // Первый запуск
            if (fmLpfCoeffsCurrent == null)
            {
                fmLpfCoeffsCurrent = DesignLpf(cutoff, sampleRate, N);
                fmLpfCoeffsTarget = fmLpfCoeffsCurrent;
                fmLpfInterpAlpha = 1.0f;

                fmLpfBuf = new float[len];
                fmLpfIndex = 0;

                cachedFmFs = sampleRate;
                cachedFmCutoff = cutoff;
                return;
            }

            // Проверяем необходимость пересчёта фильтра
            bool changed = Math.Abs(cutoff - cachedFmCutoff) > 0.1f || cachedFmFs != sampleRate;
            if (changed)
            {
                fmLpfCoeffsTarget = DesignLpf(cutoff, sampleRate, N);
                fmLpfInterpAlpha = 0.0f;
                cachedFmFs = sampleRate;
                cachedFmCutoff = cutoff;
            }

            // Интерполяция коэффициентов
            if (fmLpfInterpAlpha < 1.0f && fmLpfCoeffsTarget != null)
            {
                fmLpfInterpAlpha = Math.Min(1.0f, fmLpfInterpAlpha + DEMOD_FM_INTERP_STEP);
                float a = fmLpfInterpAlpha;
                float b = 1.0f - a;
                for (int i = 0; i < len; i++)
                {
                    fmLpfCoeffsCurrent[i] = fmLpfCoeffsCurrent[i] * b + fmLpfCoeffsTarget[i] * a;
                }
            }
        }

        #endregion
    }
}