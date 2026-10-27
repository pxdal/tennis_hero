using NaughtyAttributes;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{

    // public + serialized //

    [SerializeField] private float m_baseSpeed;

    [SerializeField] private int m_maxBallHitsUntilFullSpeed;

    [SerializeField] private float m_maxSpeed;

    [SerializeField] private AnimationCurve m_speedIncreaseCurve;


    [Header("Debug")]

    [ReadOnly]
    [SerializeField] private float m_currentBallSpeed;

    // private //

    private BoxCollider2D m_hitbox;

    private Vector3 m_currentDirection = Vector3.zero;

    private int m_numHits = 0;

    private bool m_isFrozen = false;


    // getters + setters //
    
    public bool IsFrozen()
    {
        return m_isFrozen;
    }

    public void SetFrozen(bool frozen)
    {
        m_isFrozen = frozen;
    }

    public void Freeze()
    {
        SetFrozen(true);
    }

    public void Unfreeze()
    {
        SetFrozen(false);
    }

    public Vector3 GetCurrentDirection()
    {
        // normalize just in case.  doesn't really cost anything these days!
        return m_currentDirection.normalized;
    }

    public void SetCurrentDirectionFromTargetPosition(Transform targetTransform)
    {
        m_currentDirection = (targetTransform.position - transform.position).normalized;
    }

    public void SetCurrentDirectionFromTargetPosition(Vector3 targetPosition)
    {
        m_currentDirection = (targetPosition - transform.position).normalized;
    }

    public void SetCurrentDirection(Vector3 direction)
    {
        m_currentDirection = direction.normalized;
    }

    public Vector3 GetProjectedPosition()
    {
        Vector3 deltaNormalizedMovement = GetCurrentFlySpeed() * Time.deltaTime * GetCurrentDirection();

        return transform.position + deltaNormalizedMovement;
    }

    public bool IsMovingTowardsOpponent()
    {
        return m_currentDirection.x > 0;
    }

    public int GetCurrentNumberOfHits()
    {
        return m_numHits;    
    }

    public float GetCurrentFlySpeed()
    {
        float t_p = (float)GetCurrentNumberOfHits() / m_maxBallHitsUntilFullSpeed;

        float t_speedMultiplier = m_speedIncreaseCurve.Evaluate(t_p);

        return t_speedMultiplier * (m_maxSpeed - m_baseSpeed) + m_baseSpeed;
    }

    // event listeners //

    void Awake()
    {
        m_hitbox = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        m_currentBallSpeed = GetCurrentFlySpeed();
    }
    
    void LateUpdate()
    {
        // run this here so that projected position is constant in Update() for other MonoBehaviours
        if(!IsFrozen()){
            StepToProjectedPosition();
        }
    }


    // actions //

    public void HitToTarget(Transform newTarget)
    {
        SetCurrentDirectionFromTargetPosition(newTarget);

        m_numHits++;
    }


    // utils //

    void StepToProjectedPosition()
    {
        if(GetCurrentFlySpeed() <= 0f) return;

        transform.position = GetProjectedPosition();
    }

    public bool WouldBeHitIfGuySwung(GenericTennisGuyBehaviour tennisPlayer)
    {
        RaycastHit2D[] boxcastResults = m_hitbox.BoxCastUpToEndpoint(GetProjectedPosition(), 1 << GameController.Instance.SwingHitboxLayer);

        foreach(RaycastHit2D boxcastResult in boxcastResults)
        {
            if(tennisPlayer.GetSwingHitbox() == boxcastResult.collider)
            {
                return true;
            }
        }

        return false;
    }
}
