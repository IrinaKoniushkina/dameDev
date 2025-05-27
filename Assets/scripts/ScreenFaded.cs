using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1f;
    
    private float fadeTarget;
    private float fadeProgress;
    private bool isFading;

    void Update()
    {
        if (isFading)
        {
            fadeProgress += Time.deltaTime * fadeSpeed;
            float alpha = Mathf.Lerp(0, fadeTarget, fadeProgress);
            fadeImage.color = new Color(0, 0, 0, alpha);

            if (fadeProgress >= 1f)
            {
                isFading = false;
            }
        }
    }

    public void FadeOut(float duration)
    {
        fadeSpeed = 1f / duration;
        fadeTarget = 1f;
        fadeProgress = 0f;
        isFading = true;
    }
}