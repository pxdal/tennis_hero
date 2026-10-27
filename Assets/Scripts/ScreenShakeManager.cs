using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    [SerializeField] private Vector2 m_maxShakeMagnitude;
    
    [SerializeField] private float m_baseDecayRatePerSecond = 1f;

    [SerializeField] private AnimationCurve m_shakeDecayCurve;

    [SerializeField] private float m_jitterDurationSeconds = 0.1f;

    [Header("Debug Settings")]

    [SerializeField] private float m_debugShakeMagnitude = 1f;

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    void DebugShake()
    {
        Shake(m_debugShakeMagnitude);
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void StopShaking()
    {
        if(m_currentShakeHandler != null)
        {
            StopCoroutine(m_currentShakeHandler);

            Camera.main.transform.position = new(0, 0, Camera.main.transform.position.z);
        }
    }

    private Coroutine m_currentShakeHandler = null;

    /// <summary>
    /// relative magnitude is a fraction of the max, and is clamped between 0 and 1
    /// 
    /// if already shaking, this overrides the first shake completely (not additive)
    /// </summary>
    /// <param name="relativeMagnitude"></param>
    public void Shake(float relativeMagnitude)
    {
        relativeMagnitude = Mathf.Max(relativeMagnitude, 0);

        StopShaking();

        m_currentShakeHandler = StartCoroutine(ShakeHandler(relativeMagnitude));
    }

    IEnumerator ShiftBySpeed(Transform transform, Vector2 target, float speed)
    {
        Vector2 t_pos2D = transform.position;

        while (t_pos2D != target)
        {
            t_pos2D = Vector2.MoveTowards(t_pos2D, target, speed * Time.deltaTime);

            transform.position = new(t_pos2D.x, t_pos2D.y, transform.position.z);

            yield return null;
        }
    }

    IEnumerator ShiftByDuration(Transform transform, Vector2 target, float duration)
    {
        Vector2 t_pos2D = transform.position;

        float speed = (target - t_pos2D).magnitude / duration;

        yield return ShiftBySpeed(transform, target, speed);
    }

    [ReadOnly]
    [SerializeField] private float m_debugCurrentMagnitude;

    IEnumerator ShakeHandler(float relativeShakeMagnitude)
    {
        float startTime = Time.time;
        float currentMagnitude = relativeShakeMagnitude;

        Camera camera = Camera.main;

        // need to manually calculate delta time because ShiftByDuration takes varying amounts
        float t_lastTime = startTime;

        while(currentMagnitude > 0)
        {
            m_debugCurrentMagnitude = currentMagnitude;

            // shift camera
            Vector2 target = Random.onUnitCircle;

            Vector2 absoluteShakeMagnitude = currentMagnitude * m_maxShakeMagnitude;

            target *= absoluteShakeMagnitude;
            
            yield return ShiftByDuration(camera.transform, target, m_jitterDurationSeconds);

            float t_deltaTime = Time.time - t_lastTime;
            t_lastTime = Time.time;

            // calculate next magnitude
            float t_decayAmount = t_deltaTime * m_baseDecayRatePerSecond * m_shakeDecayCurve.Evaluate(currentMagnitude);

            currentMagnitude -= t_decayAmount;

            // currentMagnitude = relativeShakeMagnitude * m_shakeDecayCurve.Evaluate(t_p);
        }

        // move back to center
        yield return ShiftByDuration(camera.transform, Vector2.zero, m_jitterDurationSeconds);

        m_debugCurrentMagnitude = 0;
    }

}
