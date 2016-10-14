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
    int numberOfLines;

    void Awake() {
        textbox = GetComponent<Text>();
        textbox.supportRichText = true;
        textbox.color = endColor;
    }

    public IEnumerator DisplayLines(string[] lines, bool clearBetweenLines) {
        if (lines == null) lines = new string[]{};

        foreach (var line in lines) {
            linesToDisplay.Enqueue(line);
        }
        yield return StartCoroutine(Roll(clearBetweenLines));
    }

    public void Clear() {
        linesDisplayed = "";
        textbox.text = "";
        numberOfLines = 0;
    }

    IEnumerator Roll(bool clearBetweenLines) {
        while (linesToDisplay.Count > 0) {
            var line = linesToDisplay.Dequeue();

            float startTime = Time.time;
            float endTime = startTime + line.Length / charactersPerSecond;

            while (Time.time < endTime) {
                if (DirectionUtil.FromInput() != Direction.None) break;
                textbox.text = ApplyFade(line, Time.time-startTime) + "\n\n" + linesDisplayed;
                yield return null;
            }

            linesDisplayed = line + "\n\n"+ linesDisplayed;
            textbox.text = linesDisplayed;
            numberOfLines++;

            // if there's additional line to display, wait
            if (linesToDisplay.Count > 0) {
                yield return new WaitForSeconds(secondsBetweenLines);
                if (clearBetweenLines) Clear();
            }
        }
        /*
        else {
            if (numberOfLines > maxNumberOfLines) {
                // max 100 chars per line? doesn't need to be exact
                linesDisplayed = linesDisplayed.Substring(0, maxNumberOfLines * 100);
                numberOfLines = 0;
            }
            yield return null;
        }

        */
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
}
}


