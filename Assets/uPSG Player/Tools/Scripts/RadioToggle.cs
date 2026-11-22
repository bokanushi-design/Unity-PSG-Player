using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadioToggle : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    private int toggleId;

    public delegate void OnRadioButtonChanged(int index);
    public OnRadioButtonChanged buttonChangedHandler;

    public void SetRadioToggle(int id, string labelText, ToggleGroup toggleGroup, bool isOn)
    {
        toggleId = id;
        toggle.GetComponentInChildren<Text>().text = labelText;
        toggle.group = toggleGroup;
        toggle.SetIsOnWithoutNotify(isOn);
    }

    public void OnToggleSelected(bool isOn)
    {
        if (isOn && buttonChangedHandler != null)
        {
            buttonChangedHandler.Invoke(toggleId);
        }
    }
}
