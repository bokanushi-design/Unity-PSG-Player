using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGMusicEffectSample : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private MMLSplitter mmlSplitter;   // BGM�p��MML Splitter
    [SerializeField] private PSGPlayer psgPlayerSE; // ���ʉ��p��PSG Player
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
        mmlSplitter.SetAllChannelsSampleRate(32000);    // BGM�̃T���v�����[�g��ݒ�
        mmlSplitter.SetAllChannelClipSize(200); // BGM��AudioClip�̒�����ݒ�

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
        // ���ʉ������Ă���ԁABGM��A�`�����l�����~���[�g
        if (isMute && !psgPlayerSE.IsPlaying())
        {
            mmlSplitter.MuteChannel(0, false);
            isMute = false;
        }
    }

    public void OnPlayButton()
    {
        if (mmlSplitter.IsAnyChannelPlaying())  // BGM�̂����ꂩ��PSG Player���Đ�����
        {
            mmlSplitter.StopAllChannels();  // BGM�̑S�Ă�PSG Player���Đ���~
        }
        else
        {
            mmlSplitter.SplitMML(mmlString);    // BGM��MML���ePSG Player�ɕ��z
            mmlSplitter.PlayAllChannels();  // BGM���f�R�[�h���čĐ�
        }
    }

    public void OnSeButton(int _id)
    {
        if (psgPlayerSE.IsPlaying())    // ���ʉ������Ă��邩
        {
            psgPlayerSE.Stop(); // ���ʉ����~
        }
        if (seMMLIndex == _id)
        {
            psgPlayerSE.PlayDecoded();  // �A�����ē���SE�Ȃ�MML�f�R�[�h���Ȃ��Ŗ炷
        }
        else
        {
            psgPlayerSE.mmlString = seMMLs[_id];    // �V����MML��n��
            psgPlayerSE.Play(); // ���ʉ����f�R�[�h���čĐ�
        }
        seMMLIndex = _id;
        mmlSplitter.MuteChannel(0, true);   // BGM��A�`�����l�����~���[�g
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
