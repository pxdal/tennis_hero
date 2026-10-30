using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerBehaviour : MonoBehaviour
{
    public Text timerText;

    private float m_startTime = -1f;

    void Start()
    {
        timerText.text = "0.0s";    
    }

    public void StartTiming()
    {
        SetTime(0f);
    }

    public void StopTiming()
    {
        m_startTime = -1f;

        // NOTE: explicitly don't Update() so that old time persists
    }

    public void SetTime(float time)
    {
        m_startTime = Time.time - time;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_startTime > 0)
        {
            float elapsed = Time.time - m_startTime;
            float rounded = (float)Math.Round(elapsed, 2);

            timerText.text = $"{rounded:0.00}s";
        }
    }
}
