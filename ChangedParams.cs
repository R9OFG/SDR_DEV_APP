/*
 *  ChangedParams.cs
 *  
 *  SDR_DEV_APP
 *  Version: 1.0 beta
 *  Modified: 18-03-2026
 *  
 *  Autor: R9OFG.RU https://r9ofg.ru/
 *  
 */

namespace SDR_DEV_APP
{
    public class ChangedParams
    {
        // === Спектр ===
        public int FftSize { get; set; } = 8192;
        public float RefLevelDB { get; set; } = 10.0f;
        public float DisplayRangeDB { get; set; } = 190.0f;

        // === Водопад ===
        public float WaterfallColorRefDB { get; set; } = -50.0f;
        public float WaterfallColorRangeDB { get; set; } = 60.0f;
        public bool WaterfallScrollDown { get; set; } = true;

        // === Цвета ===
        public Color FullSpectrumColor { get; set; } = Color.LightSeaGreen;
        public Color IChannelColor { get; set; } = Color.Lime;
        public Color QChannelColor { get; set; } = Color.Lime;

        // === I/Q ===
        public bool SwapIQ { get; set; } = false;

        // === Коррекции ===
        public bool DcCorrectionEnabled { get; set; } = false;
        public bool GainBalanceEnabled { get; set; } = false;
        public float GainRatio { get; set; } = 1.0f;
        public bool PhaseCorrectionEnabled { get; set; } = false;
        public float PhaseCoeff { get; set; } = 0.0f;

        // === Фильтрация ===
        public bool DigitalLpfEnabled { get; set; } = false;

        // === Демодуляция ===
        public DemodulationType DemodType { get; set; } = DemodulationType.USB;
        public float DemodBandwidthHz { get; set; } = 2700.0f;

        // === Аудио ===
        public bool AGCEnabled { get; set; } = false;

        public float AGCTargetLevelDb { get; set; } = -12.0f;
        public float AGCAttackTimeMs { get; set; } = 10.0f;
        public float AGCDecayTimeMs { get; set; } = 500.0f;
        public float VolumePercent { get; set; } = 10.0f;

        public void SetDefaults()
        {
            FftSize = 8192;
            RefLevelDB = 10.0f;
            DisplayRangeDB = 190.0f;

            WaterfallColorRefDB = -50.0f;
            WaterfallColorRangeDB = 60.0f;
            WaterfallScrollDown = true;

            FullSpectrumColor = Color.LightSeaGreen;
            IChannelColor = Color.Lime;
            QChannelColor = Color.Lime;

            SwapIQ = false;

            DcCorrectionEnabled = false;
            GainBalanceEnabled = false;
            GainRatio = 1.0f;
            PhaseCorrectionEnabled = false;
            PhaseCoeff = 0.0f;

            DigitalLpfEnabled = false;

            DemodType = DemodulationType.USB;
            DemodBandwidthHz = 2700.0f;

            AGCEnabled = false;
            AGCTargetLevelDb = -12.0f;     // ✔ dB
            AGCAttackTimeMs = 10.0f;
            AGCDecayTimeMs = 500.0f;

            VolumePercent = 10.0f;
        }
    }
}