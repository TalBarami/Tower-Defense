using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreManager : MonoBehaviour
{
    public Text Text;

    public Button Button;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set(List<Tuple<string, int>> lst)
    {
        var sb = new StringBuilder();
        var i = 1;
        foreach (var t in lst)
        {
            sb.Append($"{i++}. {t.Item1}: {t.Item2}\n");
            if (i > 10)
            {
                break;
            }
        }
        Text.text = $"Top score:\n {sb}";
    }

    public void OnButtonClick(Action a)
    {
        Button.onClick.AddListener(a.Invoke);
    }
}
