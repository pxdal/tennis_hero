using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimerBehaviour : MonoBehaviour
{
    public TMP_Text timerText;

    public TMP_Text bestTimeText;
    public TMP_Text bestTimeTextLabel;

    [Header("GREEN Settings")]

    public Color normal;
    public Color green;
    public float greenTime = 0.2f;

    public float internalTimeScale = 1f;

    private float m_bestTime = 0f;

    private float m_currentTime = -1f;

    private bool m_allowTwenty = false;

    void Start()
    {
        timerText.text = "0.00";    
    }

    public void SetNewBestTime(float time)
    {
        m_bestTime = time;

        // green
        StartCoroutine(WhatIsWrongWithYouWhyAreYouGreen());
    }

    IEnumerator WhatIsWrongWithYouWhyAreYouGreen()
    {
        Color baseColor = green;
        Color endColor = normal;

        bestTimeText.color = baseColor;
        bestTimeTextLabel.color = baseColor;

        float startTime = Time.time;
        float endTime = startTime + greenTime;

        while(Time.time < endTime)
        {
            Color newColor = Color.Lerp(baseColor, endColor, (Time.time - startTime) / greenTime);

            bestTimeText.color = newColor;
            bestTimeTextLabel.color = newColor;

            yield return null;
        }

        
        bestTimeText.color = endColor;
        bestTimeTextLabel.color = endColor;

        yield return null;
    }

    public float GetCurrentTime()
    {
        if(m_currentTime < 0) return 0;

        float elapsed = m_currentTime;

        if(!m_allowTwenty && elapsed >= 20)
        {
            elapsed = 19.95f;
        }
        
        return (float)Math.Round(elapsed, 2);
    }

    public void StartTiming()
    {
        SetTime(0f);
    }

    public void StopTiming()
    {
        float curTime = GetCurrentTime();

        if(curTime > m_bestTime) SetNewBestTime(curTime);

        m_currentTime = -1f;
    }

    public void SetTime(float time)
    {
        if(time >= 20) m_allowTwenty = true;

        m_currentTime = time;
        Update();
    }

    void SetRawText(float time, TMP_Text text)
    {
        float rounded = (float)Math.Round(time, 2);
        
        text.text = $"{rounded:0.00}";
    }

    public void SetText(float time)
    {
        SetRawText(time, timerText);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_currentTime >= 0)
        {
            float elapsed = GetCurrentTime();

            SetText(elapsed);
        }

        SetRawText(m_bestTime, bestTimeText);

        if(m_currentTime >= 0) m_currentTime += Time.deltaTime * internalTimeScale;
    }
}
