using UnityEngine;

public class ParamBar : MonoBehaviour
{
    [SerializeField] private GameObject paramBarPrefabObject;
    [SerializeField] private GameObject paramBarBoxObject;
    [SerializeField] private GameObject paramBarObject;
    [SerializeField] private GameObject paramCenterObject;
    [SerializeField] private int paramMin = 0;
    [SerializeField] private int paramMax = 100;
    [SerializeField] private int paramCenter = 50;
    [SerializeField] private int paramValue = 50;
    [SerializeField] private int paramBarId;
    public WavetableEditor wavetableEditor;
    private RectTransform targetRect;

    void Start()
    {
        targetRect = paramBarPrefabObject.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 screenPos = Input.mousePosition;
            Vector2 localPos;
            bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPos, null, out localPos);
            if (isInside && targetRect.rect.Contains(localPos))
            {
                paramValue = (int)System.Math.Round((localPos.y + (targetRect.rect.height / 2)) / targetRect.rect.height * (paramMax - paramMin), 0, System.MidpointRounding.AwayFromZero);
                paramValue = Mathf.Clamp(paramValue, paramMin, paramMax);
                SetParamValue(paramValue);
                wavetableEditor.OnParamBarChange(paramBarId, paramValue);
            }
        }
    }

    public void SetParamValue(int _val)
    {
        paramValue = _val;
        float val = (float)paramValue / (paramMax - paramMin);
        paramBarObject.GetComponent<RectTransform>().localScale = new Vector3(1f, val, 1f);
    }

    public void SetParamBox(int _min, int _max, int _center)
    {
        paramMin = _min;
        paramMax = _max;
        paramCenter = _center;
        var sizeY = paramBarBoxObject.GetComponent<RectTransform>().rect.height;
        float cy = sizeY * paramCenter / (paramMax - paramMin);
        var centerPos = paramCenterObject.GetComponent<RectTransform>().anchoredPosition;
        centerPos = new Vector2(centerPos.x, cy);
        paramCenterObject.GetComponent<RectTransform>().anchoredPosition = centerPos;
    }

    public void SetId(int _id)
    {
        paramBarId = _id;
    }
}
