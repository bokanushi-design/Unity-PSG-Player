using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FpsDisplay : MonoBehaviour
{
    public TMP_Text fpsText;
    public Image fpsImage;
    private int flameCount;
    private float prevTime;
    private float fps;

    private void Awake()
    {
        Application.targetFrameRate = 120;
    }

    // Start is called before the first frame update
    void Start()
    {
        flameCount = 0;
        prevTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        flameCount++;
        float time = Time.realtimeSinceStartup - prevTime;
        if (time >= 0.5f)
        {
            fps = flameCount / time;
            flameCount = 0;
            prevTime = Time.realtimeSinceStartup;
            fpsText.text = "" + (int)fps + " FPS";
        }
        if (fpsImage != null)
        {
            fpsImage.transform.Rotate(new Vector3(0, 0, 50) * Time.deltaTime);
        }
    }
}
