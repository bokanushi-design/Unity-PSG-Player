using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavetableEditor : MonoBehaviour
{
    /**** v0.9.10beta ****/
    /// <summary>
    /// This is a sample that demonstrates how to create MML files for use with the PSG Player's waveform memory sound source based on waveforms.
    /// When you create a waveform for one cycle in the Waveform Editor, MML code is generated.
    /// * How to Use *
    /// Specify the number of samples per cycle and the resolution, then press the "SET" button to display the waveform editor.
    /// When you draw a waveform in the Waveform Editor, the MML changes in real time.
    /// Use the "Saw Wave", "Sine Wave", and "Flat" buttons on the right to draw simple waveforms.
    /// Clicking the "Copy" button copies the MML to the clipboard.
    /// Enter some MML into the "Test MML" input field and click the "Play Test" button to hear how it sounds.
    /// + Additional Information +
    /// I've listed the specifications for each audio chip, which I looked up on Wikipedia, in the lower-left corner.
    /// </summary>

    [SerializeField] private PSGPlayer psgPlayer;
    [SerializeField] private GameObject waveEditorArea;
    [SerializeField] private GameObject paramBarPrefab;
    [SerializeField] private TMP_InputField samplesInputField;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_InputField wavetableMmlInputField;
    [SerializeField] private TMP_InputField testMmlInputField;
    [SerializeField] private Slider volumeSlider;

    private int waveLength = 32;
    private int waveResolution = 16;
    [SerializeField] private int[] waveParams;
    [SerializeField] private string wavetableMml = "";
    [SerializeField] private string testMml;
    private List<GameObject> paramBarList = new();

    void Start()
    {
        testMml = "w0cdefg";
        testMmlInputField.text = testMml;
        volumeSlider.value = psgPlayer.GetComponent<AudioSource>().volume;
    }

    private void CreateWaveEditor()
    {
        waveParams = new int[waveLength];
        paramBarList.Clear();
        var areaRect = waveEditorArea.GetComponent<RectTransform>().rect;
        var barRect = paramBarPrefab.GetComponent<RectTransform>().rect;
        var barPitch = areaRect.width / waveLength;
        Vector3 barScale = new Vector3(barPitch / barRect.width, areaRect.height / barRect.height, 1);
        for (int i = 0; i < waveLength; i++)
        {
            waveParams[i] = waveResolution / 2;
            Vector3 pos = new Vector3(barPitch * i - areaRect.width / 2 + barRect.width / 2, 0, 0);
            var pb = Instantiate(paramBarPrefab, waveEditorArea.transform);
            pb.GetComponent<RectTransform>().anchoredPosition = pos;
            pb.GetComponent<RectTransform>().localScale = barScale;
            pb.GetComponent<ParamBar>().wavetableEditor = this;
            pb.GetComponent<ParamBar>().SetId(i);
            pb.GetComponent<ParamBar>().SetParamBox(0, waveResolution - 1, waveParams[i]);
            pb.GetComponent<ParamBar>().SetParamValue(waveParams[i]);
            paramBarList.Add(pb);
        }
        GenerateWavetableMML();
    }

    private void DeleteWaveEditor()
    {
        var objs = GameObject.FindGameObjectsWithTag("ParamBar");
        if (objs == null) { return; }
        foreach (var obj in objs)
        {
            Destroy(obj);
        }
    }

    public void OnParamBarChange(int _id, int _val)
    {
        //Debug.Log($"ID:{_id} / Val:{_val}");
        waveParams[_id] = _val;
        GenerateWavetableMML();
    }

    private void GenerateWavetableMML()
    {
        wavetableMml = "w0{" + waveLength.ToString() + "," + waveResolution.ToString();
        foreach (var param in waveParams)
        {
            wavetableMml += "," + param.ToString();
        }
        wavetableMml += "}";
        wavetableMmlInputField.text = wavetableMml;
    }

    public void OnSamplesInputEndtEdit(string text)
    {
        string filtered = Regex.Replace(text, @"[^0-9]", "");
        if (text != filtered)
        {
            samplesInputField.text = filtered;
        }
        int samples = Mathf.Clamp(int.Parse(filtered), 2, 64);
        samplesInputField.text = samples.ToString();
        waveLength = samples;
    }

    public void OnResolutionDropdownChanged(int index)
    {
        if (index < 5)
        {
            waveResolution = 16 * (int)Mathf.Pow(2, index);
        }
    }

    public void OnWaveEditorSet()
    {
        DeleteWaveEditor();
        CreateWaveEditor();
    }

    public void OnTestPlay()
    {
        if (wavetableMml == "") { return; }
        psgPlayer.mmlString = wavetableMml + testMml;
        psgPlayer.Play();
    }

    public void OnTestMmlChanged(string text)
    {
        testMml = text;
    }

    public void OnCopyWavetableMML()
    {
        if (wavetableMml == "") { return; }
        GUIUtility.systemCopyBuffer = wavetableMml;
        Debug.Log("Copied to clipboard.");
    }

    public void OnSawWave()
    {
        for (int i=0; i<waveLength; i++)
        {
            waveParams[i] = (waveResolution - 1) - (i * (waveResolution - 1) / (waveLength - 1));
            paramBarList[i].GetComponent<ParamBar>().SetParamValue(waveParams[i]);
            OnParamBarChange(i, waveParams[i]);
        }
    }

    public void OnSineWave()
    {
        for (int i=0; i<waveLength; i++)
        {
            var rad = i * 2 * Mathf.PI / (waveLength - 1);
            waveParams[i] = Mathf.Clamp((int)(Mathf.Sin(rad) * (waveResolution - 1) / 2f + waveResolution  / 2f), 0, waveResolution - 1);
            paramBarList[i].GetComponent<ParamBar>().SetParamValue(waveParams[i]);
            OnParamBarChange(i, waveParams[i]);
        }
    }

    public void OnFlatWave()
    {
        for (int i=0; i<waveLength; i++)
        {
            waveParams[i] = waveResolution / 2;
            paramBarList[i].GetComponent<ParamBar>().SetParamValue(waveParams[i]);
            OnParamBarChange(i, waveParams[i]);
        }
    }

    public void OnVolumeChange(float val)
    {
        psgPlayer.GetComponent<AudioSource>().volume = val;
    }

}
