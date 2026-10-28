using System;
using NaughtyAttributes;
using UnityEngine;

public class GenericTennisGuyBehaviour : MonoBehaviour
{
    public enum Target {
        Lower,
        Upper,

        None
    }

    public static Target GetRandomTarget()
    {
        if(UnityEngine.Random.Range(0, 2) == 0)
        {
            return Target.Lower;
        } else
        {
            return Target.Upper;
        }
    }

    // public + serialized //

    [Header("Swing Settings")]

    [SerializeField] private BoxCollider2D m_swingHitbox;

    [SerializeField] private float m_swingCooldownSeconds;

    [SerializeField] private float m_swingDurationSeconds = 0.1f;


    [Header("Movement Targets")]

    [SerializeField] private Transform m_upperMoveTarget;
    
    [SerializeField] private Transform m_lowerMoveTarget;


    [Header("Movement Settings")]

    [SerializeField] private float m_movementLerpRate = 1f;


    [Header("Animation Settings")]

    public Animator animator;

    [AnimatorParam("animator")]
    [SerializeField] private string m_swingUpTriggerName;
    
    [AnimatorParam("animator")]
    [SerializeField] private string m_swingDownTriggerName;
    
    [AnimatorParam("animator")]
    [SerializeField] private string m_swingStraightTriggerName;


    // private //
    
    private Transform m_currentMoveTarget;

    private bool m_isSwinging = false;

    private float m_lastSwingTime = -1f;

    private bool m_isFrozen = false;

    // getters + setters //

    public BoxCollider2D GetSwingHitbox()
    {
        return m_swingHitbox;    
    }

    public Target GetCurrentMoveTarget()
    {
        if(m_currentMoveTarget == m_lowerMoveTarget)
        {
            return Target.Lower;
        } else if(m_currentMoveTarget == m_upperMoveTarget)
        {
            return Target.Upper;
        } else
        {
            throw new Exception("current move target transform (" + m_currentMoveTarget + ") is invalid");
        }
    }

    /// <summary>
    /// NOTE: does not throw any exception for bad move targets
    /// </summary>
    /// <returns></returns>
    public Transform GetCurrentMoveTargetTransform()
    {
        return m_currentMoveTarget;
    }

    public Transform GetOppositeMoveTargetTransform()
    {
        if(GetCurrentMoveTarget() == Target.Lower)
        {
            return GetUpperMoveTargetTransform();
        } else
        {
            return GetLowerMoveTargetTransform();
        }
    }

    public void SetCurrentMoveTarget(Target target)
    {
        if(target == Target.Lower)
        {
            m_currentMoveTarget = m_lowerMoveTarget;
        } else if(target == Target.Upper)
        {
            m_currentMoveTarget = m_upperMoveTarget;
        } else
        {
            throw new Exception("got invalid move target " + target);
        }
    }

    public Transform GetMoveTargetTransform(Target target)
    {
        if(target == Target.Lower)
        {
            return GetLowerMoveTargetTransform();
        } else if(target == Target.Upper)
        {
            return GetUpperMoveTargetTransform();
        } else
        {
            throw new Exception("got invalid move target " + target);
        }
    }

    public Transform GetUpperMoveTargetTransform()
    {
        return m_upperMoveTarget;
    }

    public Transform GetLowerMoveTargetTransform()
    {
        return m_lowerMoveTarget;
    }
    
    public float GetSwingDurationSeconds()
    {
        return m_swingDurationSeconds;
    }

    public float GetSwingCooldownSeconds()
    {
        return m_swingCooldownSeconds;
    }

    public float GetLastSwingTime()
    {
        return m_lastSwingTime;    
    }

    public float GetLastSwingEndTime()
    {
        return GetLastSwingTime() + GetSwingDurationSeconds();
    }

    public float GetNextTimeCanSwing()
    {
        return GetLastSwingTime() + GetSwingCooldownSeconds();
    }

    public bool IsSwinging()
    {
        return m_isSwinging;
    }

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

    // event listeners //

    public virtual void Awake()
    {
        m_currentMoveTarget = m_upperMoveTarget;
    }

    public virtual void Start()
    {
    }


    public virtual void Update()
    {
        if(!IsFrozen()) LerpToCurrentTarget();

        CheckIfSwingIsOver();
    }

    // utils //

    void LerpToCurrentTarget()
    {
        Vector3 newIntermediatePosition = Vector3.Lerp(transform.position, m_currentMoveTarget.position, m_movementLerpRate * Time.deltaTime);

        transform.position = newIntermediatePosition;
    }

    void CheckIfSwingIsOver()
    {
        if(Time.time > GetLastSwingEndTime())
        {
            m_isSwinging = false;
        }
    }

    // actions //

    public void Swing(Target target = Target.None)
    {
        if(Time.time < GetNextTimeCanSwing())
        {
            Debug.Log("swing on cooldown!");
            return;
        }

        m_lastSwingTime = Time.time;

        m_isSwinging = true;

        // animate swing
        if(animator != null)
        {
            Target t_moveTarget = GetCurrentMoveTarget();

            if(t_moveTarget == target)
            {
                animator.SetTrigger(m_swingStraightTriggerName);
            } else if(t_moveTarget == Target.Upper)
            {
                animator.SetTrigger(m_swingDownTriggerName);
            } else
            {
                animator.SetTrigger(m_swingUpTriggerName);
            }
        }
    }

    public virtual void Reset()
    {
        Unfreeze();
        
        // TODO: bad hardcoded default
        SetCurrentMoveTarget(Target.Upper);
    }
    
    
    public bool AreSwingConditionsMet()
    {
        if(!m_isSwinging) return false;
    
        return GameController.Instance.Ball.WouldBeHitIfGuySwung(this);
    }

    public void HitBallToTargetIfSwingConditionsAreMet(Transform newTarget)
    {
        if (AreSwingConditionsMet())
        {
            ForceHitBall(newTarget);
        }
    }

    /// <summary>
    /// skips AreSwingConditionsMet() check and simply assumes the ball is hit
    /// </summary>
    public void ForceHitBall(Transform newTarget)
    {
        BallBehaviour ball = GameController.Instance.Ball;

        ball.HitToTarget(newTarget);
    }
}
