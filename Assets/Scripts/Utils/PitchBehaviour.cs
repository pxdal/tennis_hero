using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PitchBehaviour : MonoBehaviour
{
    public Vector2 pitchRange;

    private AudioSource m_audioSource;
    
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    private bool m_wasPlayingLastFrame;

    void Update()
    {
        if(!m_audioSource.isPlaying && m_wasPlayingLastFrame)
        {
            m_audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }

        m_wasPlayingLastFrame = m_audioSource.isPlaying;
    }
}
