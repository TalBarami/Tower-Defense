using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyGUI : MonoBehaviour
{
    public string strategy;
    public Image flashImage;
    public Text flashText;
    
    private Color originalImageColor;
    private Color originalTextColor;
    private Color flashImageColor;
    private Color flashTextColor;

    private float t;
    private int d;
    private bool visible;

    void Start()
    {
        t = 0.0f;
        d = 1;
        originalImageColor = flashImage.color;
        originalTextColor = flashText.color;

        flashImageColor = originalImageColor;
        flashTextColor = originalTextColor;
        flashImageColor.a = 0.5f;
        flashTextColor.a = 0.5f;

        flashImage.color = flashText.color = new Color(0, 0, 0, 0);
    }

    public void Set(string strategy)
    {
        this.strategy = strategy;
        flashText.text = $"New Strategy:\n{strategy}";
        visible = true;
        StartCoroutine(Disable());
        // After 5 seconds, visible = false
    }

    void Update()
    {
        if (visible)
        {
            d = (t > 1 || t < 0) ? -Math.Sign(t) : d;
            t += d * Time.deltaTime;
            flashImage.color = Color.Lerp(originalImageColor, flashImageColor, t);
            flashText.color = Color.Lerp(originalTextColor, flashTextColor, t);
        }
        
    }

    private IEnumerator Disable()
    {
        yield return new WaitForSeconds(5);
        visible = false;
        flashImage.color = flashText.color = new Color(0, 0, 0, 0);
    }
}