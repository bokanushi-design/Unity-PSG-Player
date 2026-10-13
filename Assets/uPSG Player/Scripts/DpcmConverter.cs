using UnityEngine;
using uPSG;

public class DpcmConverter : MonoBehaviour
{
    /**** v0.9.9beta ****/

    /// <summary>
    /// Converts the AudioClip to NES DPCM data by applying the specified playback rate and low-pass filter.
    /// </summary>
    /// <param name="clip">Source AudioClip</param>
    /// <param name="rateIndex">DPCM Sample Rate</param>
    /// <param name="enableLowPass">Low-pass filter for aliasing noise reduction</param>
    /// <returns>DPCM byte array.</returns>
    public static byte[] ConvertToNesDpcm(AudioClip clip, DPCM_RATE_INDEX rateIndex, bool enableLowPass = true, bool enableNormalize = true)
    {
        if (clip == null)
        {
            Debug.LogError("Null AudioClip.");
            return null;
        }

        float targetRate = DpcmValue.DPCM_FREQ_TABLE[(int)rateIndex];
        float srcRate = clip.frequency;

        // 1. Get all audio samples
        float[] srcSamples = new float[clip.samples * clip.channels];
        clip.GetData(srcSamples, 0);

        // 2. Conversion to Mono
        float[] monoSamples = new float[clip.samples];
        for (int i = 0; i < clip.samples; i++)
        {
            float sum = 0;
            for (int c = 0; c < clip.channels; c++)
            {
                sum += srcSamples[i * clip.channels + c];
            }
            monoSamples[i] = sum / clip.channels;
        }

        // 3. Low-pass filter (to prevent aliasing noise)
        // Set the cutoff just below half the target frequency (the Nyquist frequency)
        if (enableLowPass && targetRate < srcRate)
        {
            float cutoff = targetRate * 0.45f;
            ApplyLowPassFilter(monoSamples, srcRate, cutoff);
        }

        // 4. Sampling rate conversion (linear interpolation)
        float ratio = srcRate / targetRate;
        int targetLength = Mathf.FloorToInt(monoSamples.Length / ratio);
        float[] resampledSamples = new float[targetLength];

        for (int i = 0; i < targetLength; i++)
        {
            float srcIndex = i * ratio;
            int indexLeft = Mathf.FloorToInt(srcIndex);
            int indexRight = Mathf.Min(indexLeft + 1, monoSamples.Length - 1);
            float t = srcIndex - indexLeft;

            resampledSamples[i] = Mathf.Lerp(monoSamples[indexLeft], monoSamples[indexRight], t);
        }

        // 5. Normalize
        if (enableNormalize)
        {
            float maxPeak = 0f;

            // Find the gpeakh with the largest absolute value among all samples
            for (int i = 0; i < resampledSamples.Length; i++)
            {
                float absVal = Mathf.Abs(resampledSamples[i]);
                if (absVal > maxPeak)
                {
                    maxPeak = absVal;
                }
            }

            // If a peak exists and the volume is not already at its maximum (1.0), raise the volume to its limit.
            if (maxPeak > 0f && maxPeak < 1.0f)
            {
                float boostFactor = 1.0f / maxPeak; // Multiplier to maximize results
                for (int i = 0; i < resampledSamples.Length; i++)
                {
                    resampledSamples[i] *= boostFactor;
                }
            }
        }

        // 6. DPCM Encoding (1-bit quantization)
        int totalBytes = Mathf.CeilToInt(resampledSamples.Length / 8f);
        byte[] dpcmData = new byte[totalBytes];

        int currentLevel = 64; // Default Values for NES DPCM
        int byteIndex = 0;
        int bitIndex = 0;
        byte currentByte = 0;

        for (int i = 0; i < resampledSamples.Length; i++)
        {
            float scaledSample = (resampledSamples[i] + 1.0f) * 63.5f;
            int targetLevel = Mathf.Clamp(Mathf.RoundToInt(scaledSample), 0, 127);

            int nextLevelUp = Mathf.Min(127, currentLevel + 2);
            int nextLevelDown = Mathf.Max(0, currentLevel - 2);

            int diffUp = Mathf.Abs(nextLevelUp - targetLevel);
            int diffDown = Mathf.Abs(nextLevelDown - targetLevel);

            int bit = 0;
            if (currentLevel + 2 <= 127 && (diffUp < diffDown || currentLevel - 2 < 0))
            {
                bit = 1;
                currentLevel = nextLevelUp;
            }
            else
            {
                bit = 0;
                currentLevel = nextLevelDown;
            }

            // NES specification: Pack from the LSB
            currentByte |= (byte)(bit << bitIndex);
            bitIndex++;

            if (bitIndex == 8)
            {
                dpcmData[byteIndex] = currentByte;
                byteIndex++;
                currentByte = 0;
                bitIndex = 0;
            }
        }

        if (bitIndex > 0 && byteIndex < dpcmData.Length)
        {
            dpcmData[byteIndex] = currentByte;
        }

        return dpcmData;
    }

    /// <summary>
    /// Simple Low-Pass Filtering using a First-Order IIR Filter
    /// </summary>
    private static void ApplyLowPassFilter(float[] samples, float sampleRate, float cutoffFrequency)
    {
        float dt = 1.0f / sampleRate;
        float rc = 1.0f / (2.0f * Mathf.PI * cutoffFrequency);
        float alpha = dt / (rc + dt);

        float previousSample = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            // Blends past and current waveforms to smooth out the high frequencies (abrupt changes)
            samples[i] = previousSample + alpha * (samples[i] - previousSample);
            previousSample = samples[i];
        }
    }
}
