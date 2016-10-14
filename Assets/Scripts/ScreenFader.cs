using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour {

    public static ScreenFader instance;

    public Image fadeImg;

    void Awake()
    {
        if (instance == null) {
            DontDestroyOnLoad(transform.gameObject);
            instance = this;
            /*
            if (fadeIn) {
                fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, 1.0f);
            }
            */
        } else {
            Destroy(transform.gameObject);
        }
    }

    void OnEnable() { }

    public IEnumerator FadeIn(float fadeDuration) {
        //fadeImg.gameObject.SetActive(true);
        var startTime = Time.time;
        var endTime = fadeDuration + startTime;
        while (Time.time < endTime) {
            fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, (endTime-Time.time)/fadeDuration);
            yield return null;
        }
    }

    public IEnumerator FadeOut(float fadeDuration) {
        //fadeImg.gameObject.SetActive(true);
        var startTime = Time.time;
        var endTime = fadeDuration + startTime;
        while (Time.time < endTime) {
            fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, (Time.time-startTime)/fadeDuration);
            yield return null;
        }
        //fadeImg.gameObject.SetActive(false);
    }
}
