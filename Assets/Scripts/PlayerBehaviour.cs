using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Input Actions")]

    [SerializeField] private string m_upActionName;
    [SerializeField] private string m_downActionName;
    [SerializeField] private string m_hitActionName;

    [Header("Movement Targets")]

    [SerializeField] private Transform m_upperMoveTarget;
    
    [SerializeField] private Transform m_lowerMoveTarget;

    [Header("Movement Settings")]

    [SerializeField] private float m_movementLerpRate = 1f;

    // private //

    private InputAction m_upAction;
    private InputAction m_downAction;
    private InputAction m_hitAction;


    private Transform m_currentMoveTarget;


    void Start()
    {
        InitInputActions();

        m_currentMoveTarget = m_upperMoveTarget;
    }

    void Update()
    {
        CheckMoveInputsAndSetTarget();

        LerpToCurrentTarget();
    }


    // utils //

    void InitInputActions()
    {
        m_upAction = InputSystem.actions.FindAction(m_upActionName);
        m_downAction = InputSystem.actions.FindAction(m_downActionName);
        m_hitAction = InputSystem.actions.FindAction(m_hitActionName);
    }

    void CheckMoveInputsAndSetTarget()
    {
        if (m_upAction.WasPressedThisFrame())
        {
            m_currentMoveTarget = m_upperMoveTarget;
        }

        if (m_downAction.WasPressedThisFrame())
        {
            m_currentMoveTarget = m_lowerMoveTarget;
        }
    }

    void LerpToCurrentTarget()
    {
        Vector3 newIntermediatePosition = Vector3.Lerp(transform.position, m_currentMoveTarget.position, m_movementLerpRate * Time.deltaTime);

        transform.position = newIntermediatePosition;
    }
}
