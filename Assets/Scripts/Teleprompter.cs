using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Nine;

namespace NetworkGame {

[RequireComponent(typeof(Text))]
public class Teleprompter : MonoBehaviour {

    public float secondsBetweenLines = 3f;
    public float secondsToDisplayLine = 2f;
    public Color startColor = Color.black;
    public Color endColor = Color.white;
    public int maxNumberOfLines = 20;

    // Typewriter uitext;
    Text textbox;
    Queue<string> linesToDisplay = new Queue<string>();
    string linesDisplayed = "";
    bool displayImmediately = false;
    int numberOfLines;

    void Awake() {
        textbox = GetComponent<Text>();
        textbox.supportRichText = true;
        textbox.color = endColor;
        StartCoroutine(Roll());
    }

    public void DisplayLines(params string[] lines) {
        foreach (var line in lines) {
            linesToDisplay.Enqueue(line);
        }
    }

    public void DisplayImmediately() {
        displayImmediately = true;
    }

    IEnumerator Roll() {
        while (true) {
            if (linesToDisplay.Count > 0) {
                float startTime = Time.time;
                var line = linesToDisplay.Dequeue();
                while (Time.time < startTime + secondsBetweenLines) {
                    if (displayImmediately) break;
                    textbox.text = ApplyFade(line, Time.time-startTime) + "\n\n" + linesDisplayed;
                    yield return null;
                }
                linesDisplayed = line + "\n\n"+ linesDisplayed;
                textbox.text = linesDisplayed;
                displayImmediately = false;
                numberOfLines++;
                yield return null;
            } else {
                if (numberOfLines > maxNumberOfLines) {
                    // max 100 chars per line? doesn't need to be exact
                    linesDisplayed = linesDisplayed.Substring(0, maxNumberOfLines * 100);
                }
                displayImmediately = false;
                yield return null;
            }
        }
    }

    string ApplyFade(string line, float dtime) {
        var rtfLine = new StringBuilder();

        var chars = line.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            var charLocation = ((float)i)/line.Length;
            var progress = dtime/secondsToDisplayLine;
            float opacity;
            if (charLocation < progress) {
                opacity = 1;
            } else {
                opacity = 1 - (charLocation - progress) * (secondsToDisplayLine);
            }
            var color = Color.Lerp(startColor, endColor, opacity);
            var rtfChar = string.Format("<color=#{0}>{1}</color>", ColorToHex(color), chars[i]);
            rtfLine.Append(rtfChar);
        }

        return rtfLine.ToString();
    }

    string ColorToHex(Color32 color) {
	    string hex = color.r.ToString("X2") +
            color.g.ToString("X2") + color.b.ToString("X2") +
            color.a.ToString("X2");
	    return hex;
    }
}
}


