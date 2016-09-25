using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Nine;

namespace NetworkGame {

[RequireComponent(typeof(Text))]
public class Textbox : MonoBehaviour {

    const float SECONDS_BETWEEN_TEXT = 3f;

    Text uitext;

    void Awake() {
        uitext = GetComponent<Text>();
    }

    public void Speak(params string[] speech) {
        StartCoroutine(_Speak(speech));
    }

    IEnumerator _Speak(string[] speech) {
        uitext.text = "";
        yield return new WaitForSeconds(0.4f);
        foreach (var line in speech) {
            uitext.text = line;
            yield return new WaitForSeconds(SECONDS_BETWEEN_TEXT);
        }
        uitext.text = "";
    }
}
}

