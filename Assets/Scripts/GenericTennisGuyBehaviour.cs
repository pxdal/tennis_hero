using System;
using UnityEngine;

public class GenericTennisGuyBehaviour : MonoBehaviour
{
    public enum Target {
        Lower,
        Upper
    }

    // public + serialized //

    [Header("Movement Targets")]

    [SerializeField] private Transform m_upperMoveTarget;
    
    [SerializeField] private Transform m_lowerMoveTarget;


    [Header("Movement Settings")]

    [SerializeField] private float m_movementLerpRate = 1f;


    // private //
    
    private Transform m_currentMoveTarget;


    // getters + setters //

    public Target GetCurrentTarget()
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

    public void SetCurrentTarget(Target target)
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


    // event listeners //

    public virtual void Start()
    {
        m_currentMoveTarget = m_upperMoveTarget;
    }


    public virtual void Update()
    {
        LerpToCurrentTarget();
    }

    void LerpToCurrentTarget()
    {
        Vector3 newIntermediatePosition = Vector3.Lerp(transform.position, m_currentMoveTarget.position, m_movementLerpRate * Time.deltaTime);

        transform.position = newIntermediatePosition;
    }
}
