using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeOut : MonoBehaviour
{
    public float fadeOutTime;

    private SpriteRenderer m_spriteRenderer;

    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine(fadeOutTime));
    }

    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    IEnumerator FadeOutCoroutine(float fadeDuration)
    {
        Color baseColor = m_spriteRenderer.color;
        float startOpacity = baseColor.a;

        float fadeRate = -startOpacity / fadeDuration;

        while (baseColor.a > 0)
        {
            baseColor.a += fadeRate * Time.deltaTime;

            m_spriteRenderer.color = baseColor;

            yield return null;
        }
    }
}
