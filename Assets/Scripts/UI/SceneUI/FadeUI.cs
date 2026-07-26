using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    public static FadeUI Instance;

    public Image fadeImage;

    public float fadeTime=0.4f;

    void Awake()
    {
        Instance=this;
    }

    public IEnumerator FadeOut()
    {
        Color c=fadeImage.color;

        float t=0;

        while(t<fadeTime)
        {
            t+=Time.deltaTime;

            c.a=Mathf.Lerp(0,1,t/fadeTime);

            fadeImage.color=c;

            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        Color c=fadeImage.color;

        float t=0;

        while(t<fadeTime)
        {
            t+=Time.deltaTime;

            c.a=Mathf.Lerp(1,0,t/fadeTime);

            fadeImage.color=c;

            yield return null;
        }
    }
}