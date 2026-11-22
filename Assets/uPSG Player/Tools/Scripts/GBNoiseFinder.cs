using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uPSG;

public class GBNoiseFinder : MonoBehaviour
{
    /**** v0.9.11beta ****/

    [SerializeField] private PSGPlayer psgPlayer;
    [SerializeField] private GameObject radioTogglePrefab;
    [SerializeField] private GameObject ratioGroup;
    [SerializeField] private GameObject shiftGroup;
    [SerializeField] private GameObject modeGroup;
    [SerializeField] private TMP_Text freqText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_InputField noteCharInput;
    [SerializeField] private TMP_InputField noteNumInput;
    [SerializeField] private ToggleGroup noteCharGroup;
    [SerializeField] private RadioToggle sharpRadioToggle;
    [SerializeField] private RadioToggle flatRadioToggle;
    private int noiseRatio;
    private int noiseShift;
    private int noteNumber;
    private readonly string[] noteSharp = { "c", "c+", "d", "d+", "e", "f", "f+", "g", "g+", "a", "a+", "b" };
    private readonly string[] noteFlat = { "c", "d-", "d", "e-", "e", "f", "g-", "g", "a-", "a", "b-", "b" };
    private bool isNoteSharp;
    private string mmlNoteChar;
    private string mmlMode;

    private void Start()
    {
        isNoteSharp = true;
        sharpRadioToggle.SetRadioToggle(0, "Sharp", noteCharGroup, true);
        sharpRadioToggle.buttonChangedHandler += OnSharpFlatChanged;
        flatRadioToggle.SetRadioToggle(1, "Flat", noteCharGroup, false);
        flatRadioToggle.buttonChangedHandler += OnSharpFlatChanged;
        noiseRatio = 0;
        noiseShift = 0;
        noteNumber = 0;
        mmlMode = "@7";
        for (int i=0; i<8; i++)
        {
            var rt = Instantiate(radioTogglePrefab, ratioGroup.transform);
            rt.GetComponent<RadioToggle>().SetRadioToggle(i, i.ToString(), ratioGroup.GetComponent<ToggleGroup>(), i == noiseRatio);
            rt.GetComponent<RadioToggle>().buttonChangedHandler += OnRatioToggleChanged;
        }
        for (int i=0; i<14; i++)
        {
            var st = Instantiate(radioTogglePrefab, shiftGroup.transform);
            st.GetComponent<RadioToggle>().SetRadioToggle(i, i.ToString(), shiftGroup.GetComponent<ToggleGroup>(), i == noiseShift);
            st.GetComponent<RadioToggle>().buttonChangedHandler += OnShiftToggleChanged;
        }
        var mt0 = Instantiate(radioTogglePrefab, modeGroup.transform);
        mt0.GetComponent<RadioToggle>().SetRadioToggle(0, "15 steps", modeGroup.GetComponent<ToggleGroup>(), true);
        mt0.GetComponent<RadioToggle>().buttonChangedHandler += OnModeToggleChanged;
        var mt1 = Instantiate(radioTogglePrefab, modeGroup.transform);
        mt1.GetComponent<RadioToggle>().SetRadioToggle(1, "7 steps", modeGroup.GetComponent<ToggleGroup>(), false);
        mt1.GetComponent<RadioToggle>().buttonChangedHandler += OnModeToggleChanged;

        GetNoteMML();

        volumeSlider.value = psgPlayer.GetComponent<AudioSource>().volume;
    }

    private void GetNoteMML()
    {
        noteNumber = noiseRatio * 14 + noiseShift;
        noteNumInput.text = $"{mmlMode}n{noteNumber}";
        int octave = noteNumber / 12 - 1;
        string note = (isNoteSharp) ? noteSharp[noteNumber % 12] : noteFlat[noteNumber % 12];
        mmlNoteChar = $"o{octave}{note}";
        noteCharInput.text = $"{mmlMode}{mmlNoteChar}";
        int r = (noiseRatio == 0) ? 8 : 16 * noiseRatio;
        double noiseFreq = ConstValue.GB_FREQ / (r * (1 << noiseShift));
        freqText.text = $"Freq:{noiseFreq}";
        if (psgPlayer.IsPlaying())
        {
            PlayNoise();
        }
    }

    private void PlayNoise()
    {
        psgPlayer.mmlString = $"t200l32{mmlMode}L{mmlNoteChar}&";
        psgPlayer.Play();
    }

    private void OnRatioToggleChanged(int index)
    {
        //Debug.Log($"ratio:{index}");
        noiseRatio = index;
        GetNoteMML();
    }

    private void OnShiftToggleChanged(int index)
    {
        //Debug.Log($"shift:{index}");
        noiseShift = index;
        GetNoteMML();
    }

    private void OnModeToggleChanged(int index)
    {
        mmlMode = (index == 0) ? "@7" : "@8";
        GetNoteMML();
    }

    private void OnSharpFlatChanged(int index)
    {
        isNoteSharp = (index == 0);
        GetNoteMML();
    }

    public void OnPlayButton()
    {
        if (psgPlayer.IsPlaying())
        {
            psgPlayer.Stop();
        }
        else
        {
            PlayNoise();
        }
    }

    public void OnCopyNoteChar()
    {
        GUIUtility.systemCopyBuffer = noteCharInput.text;
        Debug.Log("Copied to clipboard.");
    }

    public void OnCopyNoteNum()
    {
        GUIUtility.systemCopyBuffer = noteNumInput.text;
        Debug.Log("Copied to clipboard.");
    }

    public void OnVolumeChange(float val)
    {
        psgPlayer.GetComponent<AudioSource>().volume = val;
    }
}
