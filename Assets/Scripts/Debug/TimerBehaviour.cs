using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerBehaviour : MonoBehaviour
{
    public Text timerText;

    private float m_startTime = -1f;

    void Start()
    {
        timerText.text = "0.0";    
    }

    public void StartTiming()
    {
        m_startTime = Time.time;
    }

    public void StopTiming()
    {
        m_startTime = -1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_startTime < 0)
        {
            if(GameController.Instance.Ball.GetCurrentFlySpeed() > 0 && GameController.Instance.GetCurrentGameState() != GameController.GameState.Lose)
            {
                StartTiming();
            }    
        }

        if(m_startTime > 0)
        {
            float elapsed = Time.time - m_startTime;
            float rounded = (float)Math.Round(elapsed, 2);

            timerText.text = $"{rounded:0.00}";
        }
    }
}
