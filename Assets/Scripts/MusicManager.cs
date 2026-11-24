using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Clips")]

    public AudioClip normalTheme;

    public AudioClip victory;

    public AudioClip infinityIntro;

    public AudioClip infinityTheme;

    [Header("Sources")]

    public AudioSource calmPlayer;

    public AudioSource normalPlayer;
    public AudioSource infiniteIntroPlayer;
    public AudioSource infiniteLoopPlayer;

    private float m_calmBaseVolume;
    private float m_normalBaseVolume;

    private float m_infiniteBaseVolume;

    private bool m_infiniteMode = false;

    private bool m_isMuted = false;

    public bool IsMuted()
    {
        return m_isMuted;
    }

    void Start()
    {
        m_calmBaseVolume = calmPlayer.volume;
        m_normalBaseVolume = normalPlayer.volume;
        m_infiniteBaseVolume = infiniteLoopPlayer.volume;

        calmPlayer.Play();

        PlayCalm();
    }

    private int m_currentlyPlaying = 0;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleInfiniteMode()
    {
        m_infiniteMode = true;
    }

    public void PlayCalm()
    {
        m_currentlyPlaying = 0;

        calmPlayer.volume = m_calmBaseVolume;

        normalPlayer.Stop();
        infiniteIntroPlayer.Stop();
        infiniteLoopPlayer.Stop();

        if (m_isMuted)
        {
            Mute();
        }
    }

    public void PlayNormal()
    {
        m_currentlyPlaying = m_infiniteMode ? 2 : 1;

        calmPlayer.volume = 0f;

        normalPlayer.volume = m_normalBaseVolume;
        normalPlayer.clip = normalTheme;

        double curTime = AudioSettings.dspTime;
        float initialDelay = 0.1f;

        calmPlayer.PlayScheduled(curTime + initialDelay);
        normalPlayer.PlayScheduled(curTime + initialDelay);

        if (m_infiniteMode)
        {
            double introStartTime = curTime + initialDelay + normalTheme.length;

            infiniteLoopPlayer.volume = m_infiniteBaseVolume;
            infiniteIntroPlayer.volume = m_infiniteBaseVolume;
            infiniteIntroPlayer.PlayScheduled(introStartTime);
            infiniteLoopPlayer.PlayScheduled(introStartTime + infinityIntro.length);
        }

        if (m_isMuted)
        {
            Mute();
        }
    }

    public void PlayVictory(float delay)
    {
        normalPlayer.clip = victory;

        normalPlayer.Stop();
        normalPlayer.volume = m_normalBaseVolume;
        normalPlayer.PlayScheduled(AudioSettings.dspTime + delay);

        // if (m_isMuted)
        // {
        //     Mute();
        // }
    }

    public void Mute()
    {
        calmPlayer.volume = 0f;
        normalPlayer.volume = 0f;
        infiniteLoopPlayer.volume = 0f;
        infiniteIntroPlayer.volume = 0f;
    }

    public void ToggleMuteUnconditionally()
    {
        if (m_isMuted)
        {
            m_isMuted = false;

            if(m_currentlyPlaying == 0)
            {
                calmPlayer.volume = m_calmBaseVolume;
            } else if(m_currentlyPlaying >= 1)
            {
                
                calmPlayer.volume = 0f;

                normalPlayer.volume = m_normalBaseVolume;

                if(m_currentlyPlaying == 2)
                {
                    infiniteLoopPlayer.volume = m_infiniteBaseVolume;
                    infiniteIntroPlayer.volume = m_infiniteBaseVolume;
                }        
            }
        } else
        {
            Mute();

            m_isMuted = true;
        }
    }
}
