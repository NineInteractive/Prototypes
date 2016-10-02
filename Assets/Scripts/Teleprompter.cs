using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Nine;

namespace NetworkGame {

[RequireComponent(typeof(Text))]
public class Teleprompter : MonoBehaviour {

    public float secondsBetweenLines = 2f;
    public float charactersPerSecond = 20f;
    public Color startColor = Color.black;
    public Color endColor = Color.white;
    public int maxNumberOfLines = 20;

    Text textbox;
    Queue<string> linesToDisplay = new Queue<string>();
    string linesDisplayed = "";
    bool displayImmediately = false;
    int numberOfLines;
    bool _displaying;

    void Awake() {
        textbox = GetComponent<Text>();
        textbox.supportRichText = true;
        textbox.color = endColor;
        StartCoroutine(Roll());
    }

    public void DisplayLines(params string[] lines) {
        _displaying = true;
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
                _displaying = true;
                var line = linesToDisplay.Dequeue();

                float startTime = Time.time;
                float endTime = startTime + line.Length / charactersPerSecond;

                while (Time.time < endTime) {
                    if (displayImmediately) break;
                    textbox.text = ApplyFade(line, Time.time-startTime) + "\n\n" + linesDisplayed;
                    yield return null;
                }
                linesDisplayed = line + "\n\n"+ linesDisplayed;
                textbox.text = linesDisplayed;
                displayImmediately = false;
                numberOfLines++;

                // if there's additional line to display, wait
                if (linesToDisplay.Count > 0) {
                    yield return new WaitForSeconds(secondsBetweenLines);
                }
            } else {
                _displaying = false;
                if (numberOfLines > maxNumberOfLines) {
                    // max 100 chars per line? doesn't need to be exact
                    linesDisplayed = linesDisplayed.Substring(0, maxNumberOfLines * 100);
                    numberOfLines = 0;
                }
                displayImmediately = false;
                yield return null;
            }
        }
    }

    string ApplyFade(string line, float dtime) {
        var rtfLine = new StringBuilder();

        var chars = line.ToCharArray();
        for (int charIdx = 0; charIdx < chars.Length; charIdx++) {
            float fullyDisplayedCharIdx = charactersPerSecond * dtime;
            float opacity;
            if (charIdx < charactersPerSecond * dtime) {
                opacity = 1;
            } else {
                opacity = 1 - Mathf.Min((charIdx-fullyDisplayedCharIdx)/charactersPerSecond, 1);
            }
            var color = Color.Lerp(startColor, endColor, opacity);
            var rtfChar = string.Format("<color=#{0}>{1}</color>", ColorToHex(color), chars[charIdx]);
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

    public bool displaying {
        get {
            return _displaying;
        }
    }
}
}


