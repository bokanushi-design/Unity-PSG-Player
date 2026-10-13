using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using uPSG;

[RequireComponent(typeof(AudioSource))]
public class AudioClipToDPCM : MonoBehaviour
{
    /**** v0.9.9beta ****/

    /// <summary>
    /// This is a sample that converts any audio file into DPCM data that can be used with the Unity PSG Player.
    /// The file selection dialog works only within the Unity Editor.
    /// Only audio files in formats supported by Unity (wav, ogg, mp3) can be used.
    /// *How to Use
    /// Click the “Select Audioclip” button to select the audio file you want to convert.
    /// Click “Play Source” to preview the selected audio file.
    /// Select the DPCM sample rate from the drop-down list, then click “Convert” to convert the data to DPCM.
    /// “Use LPF” and “Use Normalize” are options for converting to DPCM.
    /// Click “Play DPCM” to preview the converted DPCM data.
    /// You can save DPCM data to a file using “Save DPCM.”
    /// </summary>

    public AudioClip sourceClip;
    public DPCM_RATE_INDEX dpcmRate = DPCM_RATE_INDEX.Rate_33_14kHz;
    public bool useFilter = true;
    public bool useNormalize = true;
    public int streamSampleRate = 44100;

    [SerializeField] private TMP_InputField audioClipPathInputField;
    [SerializeField] private TMP_Dropdown rateDropdown;
    [SerializeField] private TMP_Text audioClipLog;
    [SerializeField] private TMP_Text dpcmLog;

    private AudioSource audioSource;
    private string audioClipPath = "";
    private byte[] dpcmData;
    private int dpcmDataIndex = 0;
    private int currentDpcmLevel = 64;
    private int currentBitIndex = 0;
    private double currentAudioClipTime = 0d;
    private double nextEventTime = 0d;
    private bool dpcmEnd = false;
    private string defaultFileName = "";

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        List<string> optionList = new();
        optionList.AddRange(System.Enum.GetNames(typeof(DPCM_RATE_INDEX)));
        rateDropdown.ClearOptions();
        rateDropdown.AddOptions(optionList);
        rateDropdown.value = (int)dpcmRate;
    }

    private void Update()
    {
        if (audioSource.isPlaying)
        {
            if (dpcmEnd)
            {
                audioSource.Stop();
                dpcmEnd = false;
            }
        }
    }

    public void ConvertToDpcm()
    {
        if (sourceClip == null)
        {
            audioClipLog.text = "Select audio clip.";
            return;
        }

        dpcmData = DpcmConverter.ConvertToNesDpcm(sourceClip, dpcmRate, useFilter, useNormalize);
        dpcmLog.text = $"[Convert complete] size: {dpcmData.Length} Byte / NES max(4081B): {(dpcmData.Length / 4081f * 100f):F1}%";
        defaultFileName = Path.GetFileNameWithoutExtension(audioClipPath) + $"_{dpcmRate}";
    }

    public void PlaySource()
    {
        audioSource.clip = sourceClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    public void PlayStream()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            return;
        }
        if (dpcmData == null)
        {
            ConvertToDpcm();
        }
        currentDpcmLevel = 64;
        currentBitIndex = 0;
        dpcmDataIndex = 0;
        AudioClip streamAudioClip = AudioClip.Create("StreamDPCM", streamSampleRate, 1, streamSampleRate, true, OnAudioRead, OnAudioSetPosition);
        audioSource.clip = streamAudioClip;
        audioSource.loop = true;
        dpcmEnd = false;
        audioSource.Play();
    }

    private void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (currentAudioClipTime >= nextEventTime)
            {
                if (dpcmDataIndex >= dpcmData.Length)
                {
                    data[i] = 0;
                    dpcmEnd = true;
                    continue;
                }

                int direction = (dpcmData[dpcmDataIndex] & (1 << currentBitIndex)) != 0 ? 1 : 0;
                if (direction == 1)
                {
                    currentDpcmLevel = Mathf.Min(127, currentDpcmLevel + 2);
                }
                else
                {
                    currentDpcmLevel = Mathf.Max(0, currentDpcmLevel - 2);
                }

                // Inverse mapping of a 7-bit counter value (0–127) to the Unity audio range (-1.0–1.0)
                data[i] = (currentDpcmLevel / 63.5f) - 1.0f;

                currentBitIndex++;
                if (currentBitIndex >= 8)
                {
                    currentBitIndex = 0;
                    dpcmDataIndex++;
                }

                nextEventTime += 1d / (double)GetNesFrequency(dpcmRate);
            }
            else
            {
                data[i] = (currentDpcmLevel / 63.5f) - 1.0f;
            }
            currentAudioClipTime += 1.0d / (double)streamSampleRate;
        }
    }

    private void OnAudioSetPosition(int newPosition)
    {

    }

    private float GetNesFrequency(DPCM_RATE_INDEX rateIndex)
    {
        return DpcmValue.DPCM_FREQ_TABLE[(int)rateIndex];
    }

    public void OnRateDropdownChanged()
    {
        dpcmRate = (DPCM_RATE_INDEX)rateDropdown.value;
    }

    public void OnUseFilter(bool isOn)
    {
        useFilter = isOn;
    }

    public void OnUseNormalize(bool isOn)
    {
        useNormalize = isOn;
    }

    public void OnSelectAudioClip()
    {
        audioClipPath = EditorUtility.OpenFilePanelWithFilters("Open Audio Clip", "", new string[] { "Audio Clip", "mp3,ogg,wav" });
        if (string.IsNullOrEmpty(audioClipPath))
        {
            return;
        }
        audioClipPathInputField.text = audioClipPath;
        StartCoroutine(LoadAudioClip());
    }

    IEnumerator LoadAudioClip()
    {
        string fileUri = "file://" + audioClipPath;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                audioClipLog.text = www.error;
            }
            else
            {
                sourceClip = DownloadHandlerAudioClip.GetContent(www);
                sourceClip.name = Path.GetFileNameWithoutExtension(audioClipPath);
                audioClipLog.text = $"[Channels : {sourceClip.channels.ToString()}] [Length: { sourceClip.length.ToString()}s] [Freq : {sourceClip.frequency.ToString()}Hz]";
            }
        }
    }

    public void OnSaveDpcm()
    {
        if (dpcmData == null)
        {
            return;
        }
        string savePath = EditorUtility.SaveFilePanel("Save DPCM", Path.GetDirectoryName(audioClipPath), defaultFileName, "dmc,bytes");
        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }
        Debug.Log(savePath);
        File.WriteAllBytes(savePath, dpcmData);
    }
}
