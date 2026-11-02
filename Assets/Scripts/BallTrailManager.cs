using UnityEngine;

public class BallTrailManager : MonoBehaviour
{
    public BallBehaviour parent;

    public TrailRenderer trailRenderer;

    [SerializeField] private float m_minBallSpeedForTrail;

    [SerializeField] private AnimationCurve m_trailWidthCurve;
    
    void Update()
    {
        if(parent.GetCurrentFlySpeed() < m_minBallSpeedForTrail || parent.transform.position.x < GameController.Instance.GameOverVerticalBoundary)
        {
            trailRenderer.emitting = false;
        } else
        {
            trailRenderer.emitting = true;

            float t_trailMultiplier = m_trailWidthCurve.Evaluate(parent.GetCurrentFlySpeed());

            trailRenderer.widthMultiplier = t_trailMultiplier;
        }
    }
}
