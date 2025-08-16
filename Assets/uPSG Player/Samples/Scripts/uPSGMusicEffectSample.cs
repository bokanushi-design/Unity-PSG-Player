using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGMusicEffectSample : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private MMLSplitter mmlSplitter;   // BGM用のMML Splitter
    [SerializeField] private PSGPlayer psgPlayerSE; // 効果音用のPSG Player
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeBgmSlider;
    [SerializeField] private Slider volumeSeSlider;

    public string mmlString;
    private string bgmMML;
    private string[] seMMLs;
    private int seMMLIndex;
    private bool isMute = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seMMLIndex = -1;
        seMMLs = new string[] {
            "// JUMP\nt120@2V1{,,,,,,,,,,,,,,,,,,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0}\nv15l4o4S-10a32&@1S+40V1aS0" ,
            "// KICK\nt120@2V1{9,10,11,12,13,14,15}\nl16G90V1S+80o4b->fS0" ,
            "// MUSHROOM\nt120@2v15l64G99o4c32gg+c+32g+ad32aa+d+32a+be32b>c<f32>cc+<f+32>c+d<g32>dd+" ,
            "// 1UP\nt200@2V1{15,14,13,12,11,10,9,8,7,6}\nV1l8o6eg>ecdg" ,
            "// COIN\nt120@2V1{15,14,,13,,12,,11,,10,,9,,8,,7,6,,5,4,,3,2,,1,0}\nv15G99o5l4b32>V1e"
        };
        bgmMML = Resources.Load<TextAsset>("sample-bgm_MML").text;
        Resources.UnloadUnusedAssets();
        inputField.text = bgmMML;
        mmlString = bgmMML;
        mmlSplitter.SetAllChannelsSampleRate(32000);    // BGMのサンプルレートを設定
        mmlSplitter.SetAllChannelClipSize(200); // BGMのAudioClipの長さを設定

        audioMixer.GetFloat("PSG-Mix", out float _volBgm);
        _volBgm = Mathf.Clamp(_volBgm, -80f, 20f);
        volumeBgmSlider.value = Mathf.Clamp01(Mathf.Pow(10f, _volBgm / 20f));

        audioMixer.GetFloat("PSG-Mix", out float _volSe);
        _volSe = Mathf.Clamp(_volSe, -80f, 20f);
        volumeSeSlider.value = Mathf.Clamp01(Mathf.Pow(10f, _volSe / 20f));
    }

    // Update is called once per frame
    void Update()
    {
        // 効果音が鳴っている間、BGMのAチャンネルをミュート
        if (isMute && !psgPlayerSE.IsPlaying())
        {
            mmlSplitter.MuteChannel(0, false);
            isMute = false;
        }
    }

    public void OnPlayButton()
    {
        if (mmlSplitter.IsAnyChannelPlaying())  // BGMのいずれかのPSG Playerが再生中か
        {
            mmlSplitter.StopAllChannels();  // BGMの全てのPSG Playerを再生停止
        }
        else
        {
            mmlSplitter.SplitMML(mmlString);    // BGMのMMLを各PSG Playerに分配
            mmlSplitter.PlayAllChannels();  // BGMをデコードして再生
        }
    }

    public void OnSeButton(int _id)
    {
        if (psgPlayerSE.IsPlaying())    // 効果音が鳴っているか
        {
            psgPlayerSE.Stop(); // 効果音を停止
        }
        if (seMMLIndex == _id)
        {
            psgPlayerSE.PlayDecoded();  // 連続して同じSEならMMLデコードしないで鳴らす
        }
        else
        {
            psgPlayerSE.mmlString = seMMLs[_id];    // 新しいMMLを渡す
            psgPlayerSE.Play(); // 効果音をデコードして再生
        }
        seMMLIndex = _id;
        mmlSplitter.MuteChannel(0, true);   // BGMのAチャンネルをミュート
        isMute = true;
    }

    public void OnInputChange(string inputText)
    {
        mmlString = inputText;
    }

    public void OnNextButton()
    {
        SceneManager.LoadScene("SingleChannelSample");
    }

    public void OnBgmVolumeChange(float val)
    {
        val = Mathf.Clamp(val, 0.0001f, 10f);
        audioMixer.SetFloat("PSG-Mix", 20f * Mathf.Log10(val));
    }
    public void OnSeVolumeChange(float val)
    {
        val = Mathf.Clamp(val, 0.0001f, 10f);
        audioMixer.SetFloat("PSG-SE", 20f * Mathf.Log10(val));
    }
}
