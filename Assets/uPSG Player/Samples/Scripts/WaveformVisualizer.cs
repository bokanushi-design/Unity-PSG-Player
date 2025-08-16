using UnityEngine;

public class WaveformVisualizer : MonoBehaviour
{
    public AudioSource audioSource;
    public LineRenderer lineRenderer;
    public int numSamples = 1024;
    public float scale = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (audioSource == null || lineRenderer == null)
        {
            Debug.LogError("AudioSource and LineRenderer must be assigned.");
            enabled = false;
            return;
        }

        lineRenderer.positionCount = numSamples;
        lineRenderer.useWorldSpace = false; // ローカル座標で表示
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float[] samples = new float[numSamples];
        audioSource.GetOutputData(samples, 0);

        for (int i = 0; i < numSamples; i++)
        {
            float y = samples[i] * scale;
            lineRenderer.SetPosition(i, new Vector3(i * (1f / numSamples), y, 0));
        }
    }
}
