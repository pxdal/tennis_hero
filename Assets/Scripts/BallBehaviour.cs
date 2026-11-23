using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
class BallSpeedData
{
    public int numHits;

    public float ballSpeed;
}

public class BallBehaviour : MonoBehaviour
{

    // public + serialized //

    [Tooltip("Key is number of hits, value is ball speed.  Any that don't have a value use the ball speed of the previous")]
    [SerializeField] private List<BallSpeedData> m_ballSpeeds = new();

    [Header("Components")]

    [SerializeField] private SpriteRenderer m_ballSprite;

    [Header("Settings")]

    [SerializeField] private Vector2 m_angularVelocityRange;

    [Header("Debug")]

    // NOTE: don't remove either of these, even though vs code says to.  used for debugging

    [ReadOnly]
    [SerializeField] private int m_currentNumHits;

    [ReadOnly]
    [SerializeField] private float m_currentBallSpeed;

    // private //

    private Vector3 m_positionAtStart;

    private BoxCollider2D m_hitbox;

    private Vector3 m_currentDirection = Vector3.zero;

    private int m_numHits = 0;

    private bool m_isFrozen = false;

    private readonly List<float> m_ballSpeedFullTable = new();

    private float m_maxFlySpeed;

    private float m_angularVelocity = 0f;

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

    public float GetFlySpeedAtNumHits(int numHits)
    {
        numHits = Mathf.Clamp(numHits, 0, m_ballSpeedFullTable.Count - 1);

        return m_ballSpeedFullTable[numHits];
    }

    public float GetCurrentFlySpeed()
    {
        int t_numHits = GetCurrentNumberOfHits();

        return GetFlySpeedAtNumHits(t_numHits);
    }

    public float GetMaxFlySpeed()
    {
        return m_maxFlySpeed;
    }

    // event listeners //

    void Awake()
    {
        m_hitbox = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        m_positionAtStart = transform.position;

        BuildFullSpeedTable();
    }

    void Update()
    {
        m_currentNumHits = GetCurrentNumberOfHits();
        m_currentBallSpeed = GetCurrentFlySpeed();

        if (!IsFrozen())
        {
            Vector3 eulerAngles = m_ballSprite.transform.localEulerAngles;
            eulerAngles.z += m_angularVelocity * Time.deltaTime * GetCurrentFlySpeed();
            m_ballSprite.transform.localEulerAngles = eulerAngles;
        }
    }
    
    void LateUpdate()
    {
        // run this here so that projected position is constant in Update() for other MonoBehaviours
        if(!IsFrozen()){
            StepToProjectedPosition();
        }
    }


    // actions //

    public void Teleport(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /// <summary>
    /// NOTE: not delta-corrected
    /// </summary>
    /// <param name="amount"></param>
    public void StepInCurrentDirection(float amount)
    {
        transform.position += amount * GetCurrentDirection();
    }
    
    public void HitToTarget(Transform newTarget)
    {
        SetCurrentDirectionFromTargetPosition(newTarget);

        HandleHit();
    }

    public void HitToTarget(Vector3 newTargetPosition)
    {
        SetCurrentDirectionFromTargetPosition(newTargetPosition);

        HandleHit();
    }

    public void ResetHits()
    {
        m_numHits = 0;    
    }

    public void Reset()
    {
        ResetHits();
        m_currentDirection = Vector3.zero;
        transform.position = m_positionAtStart;
        transform.eulerAngles = new(0, 0, 0);
        Unfreeze();
    }

    // utils //

    void BuildFullSpeedTable()
    {
        int t_last = -1;

        foreach(BallSpeedData speedData in m_ballSpeeds)
        {
            int t_index = speedData.numHits;
            float t_speed = speedData.ballSpeed;

            int t_numMissing = t_index - t_last;

            for(int i = 0; i < t_numMissing-1; i++)
            {
                m_ballSpeedFullTable.Add(m_ballSpeedFullTable[^1]);
            }

            m_ballSpeedFullTable.Add(t_speed);

            t_last = t_index;

            m_maxFlySpeed = Mathf.Max(m_maxFlySpeed, t_speed);
        }
    }

    void StepToProjectedPosition()
    {
        if(GetCurrentFlySpeed() <= 0f) return;

        transform.position = GetProjectedPosition();
    }

    public bool WouldBeHitIfGuySwung(GenericTennisGuyBehaviour tennisPlayer, out float distanceFromCollider)
    {
        distanceFromCollider = -1f;

        // NOTE: don't want to swing during hitstop (even though it's technically possible, so this is really a bandaid fix...)
        if (IsFrozen()) return false;

        RaycastHit2D[] boxcastResults = m_hitbox.BoxCastUpToEndpoint(GetProjectedPosition(), 1 << GameController.Instance.SwingHitboxLayer);

        foreach(RaycastHit2D boxcastResult in boxcastResults)
        {
            if(tennisPlayer.GetSwingHitbox() == boxcastResult.collider)
            {
                distanceFromCollider = boxcastResult.distance;

                return true;
            }
        }

        return false;
    }

    void HandleHit()
    {
        float newAngularVelocity = UnityEngine.Random.Range(m_angularVelocityRange.x, m_angularVelocityRange.y);

        float newSign = Mathf.Sign(m_angularVelocity) * -1;

        m_angularVelocity = newAngularVelocity * newSign;

        m_numHits++;
    }
}
