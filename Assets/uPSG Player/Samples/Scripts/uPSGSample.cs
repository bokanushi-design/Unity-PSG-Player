using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class uPSGSample : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PSGPlayer psgPlayer;   // 設置したPSG Playerプレハブを登録する

    public string mmlString;
    private string[] sampleMMLs;
    private int sampleMMLIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sampleMMLIndex = 0;
        sampleMMLs = new string[] {
            "// CURSE\nt150@1v15l16\no2 b>b<b->b-< b>b<b->b-< b>b<b->b-< b>b<b->b-\ne8.g32a-32<b-",
            "// NOISE\nt150@5v15l8\no5 g g- f e e- d d- c < b b- a a- g g- f e",
            "// LOOP\nt150@2v12l16\no5Lc>c<b>cec<b>c<\nc>c<b->cec<b>c<\nc>c<a>cec<b>c<\nc>c<b->cec<b>c<",
            "// JUMP\nt120@2V1{15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0}\nv15l4o4S-10a32&@1S+40V1aS0",
            "// KICK\nt120@2V1{9,10,11,12,13,14,15}\nl16G90V1S+80o4b->fS0",
            "// MUSHROOM\nt120@2v15l64G99o4c32gg+c+32g+ad32aa+d+32a+be32b>c<f32>cc+<f+32>c+d<g32>dd+",
            "// 1UP\nt200@2V1{15,14,13,12,11,10,9,8,7,6}\nV1l8o6eg>ecdg",
            "// COIN\nt120@2V1{15,14,14,13,13,12,12,11,11,10,10,9,9,8,8,7,6,6,5,4,4,3,2,2,1,0}\nv15G99o5l4b32>V1e"
        };
        mmlString = sampleMMLs[sampleMMLIndex];
        inputField.text = mmlString;
        psgPlayer.mmlString = mmlString;    // PSG PlayerのmmlString変数にMMLを入れる
        psgPlayer.sampleRate = 44100;   // PSG Playerのサンプルレートを設定
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayButton()
    {
        if (psgPlayer.IsPlaying())  // PSG Playerが鳴っているか
        {
            psgPlayer.Stop();   // 再生を停止
        }
        else
        {
            psgPlayer.Play();   // MMLをデコードして再生
        }
    }

    public void OnChangeSampleMML()
    {
        sampleMMLIndex++;
        if (sampleMMLIndex >= sampleMMLs.Length) { sampleMMLIndex = 0; }
        mmlString = sampleMMLs[sampleMMLIndex];
        inputField.text = mmlString;
        psgPlayer.mmlString = mmlString;
    }

    public void OnInputChange(string inputText)
    {
        mmlString = inputText;
        psgPlayer.mmlString = mmlString;
    }

    public void OnNextButton()
    {
        SceneManager.LoadScene("MultiChannelSample");
    }
}
