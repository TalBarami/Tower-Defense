using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameReader : MonoBehaviour
{
    public InputField inputField;
    public Action<string> onNameSubmit;

    void Start()
    {
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitName);
        inputField.onEndEdit = se;
    }

    private void SubmitName(string name)
    {
        onNameSubmit?.Invoke(name);
    }
}