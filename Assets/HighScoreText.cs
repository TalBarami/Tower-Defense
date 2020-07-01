using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreText : MonoBehaviour
{
    public Text Text;
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
        Debug.Log("Received:" + string.Join(",", lst.Select(t => $"{t.Item1}={t.Item2}").ToArray()));
        var sb = new StringBuilder();
        var i = 1;
        foreach (var t in lst)
        {
            sb.Append($"{i++}. {t.Item1}: {t.Item2}\n");
            Debug.Log($"added item {i}: {t.Item1}, {t.Item2}");
            if (i > 10)
            {
                break;
            }
        }
        Text.text = $"Top score:\n {sb}";
    }
}
