using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Nine;

namespace NetworkGame {

[RequireComponent(typeof(Text))]
public class Textbox : MonoBehaviour {

    const float SECONDS_BETWEEN_TEXT = 7f;

    Typewriter uitext;
    Queue<string> thingsToSay = new Queue<string>();

    void Awake() {
        uitext = GetComponent<Typewriter>();
        StartCoroutine(_Speak());
    }

    public void Speak(params string[] speech) {
        // TODO if new text arrives, speed it up
        foreach (var s in speech) {
            thingsToSay.Enqueue(s);
        }
    }

    IEnumerator _Speak() {
        uitext.BeginDisplayMode("");
        while (true) {
            if (thingsToSay.Count > 0) {
                uitext.BeginDisplayMode(thingsToSay.Dequeue());
                yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
            } else {
                yield return null;
            }
        }
    }
}
}

