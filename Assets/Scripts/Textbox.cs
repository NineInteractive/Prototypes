using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Nine;

namespace NetworkGame {

[RequireComponent(typeof(Text))]
public class Textbox : MonoBehaviour {

    const float SECONDS_BETWEEN_TEXT = 3.5f;

    Text uitext;
    Queue<string> thingsToSay = new Queue<string>();

    void Awake() {
        uitext = GetComponent<Text>();
        StartCoroutine(_Speak());
    }

    public void Speak(params string[] speech) {
        // TODO if new text arrives, speed it up
        foreach (var s in speech) {
            thingsToSay.Enqueue(s);
        }
    }

    IEnumerator _Speak() {
        uitext.text = "";
        while (true) {
            if (thingsToSay.Count > 0) {
                uitext.text = thingsToSay.Dequeue();
                yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
            } else {
                uitext.text = "";
                yield return null;
            }
        }
    }
}
}

