using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Input Actions")]

    [SerializeField] private string m_upActionName;
    [SerializeField] private string m_downActionName;
    [SerializeField] private string m_hitActionName;

    // private //

    private InputAction m_upAction;
    private InputAction m_downAction;
    private InputAction m_hitAction;

    void Start()
    {
        InitInputActions();
    }

    void Update()
    {
        
    }


    // utils //

    void InitInputActions()
    {
        m_upAction = InputSystem.actions.FindAction(m_upActionName);
        m_downAction = InputSystem.actions.FindAction(m_downActionName);
        m_hitAction = InputSystem.actions.FindAction(m_hitActionName);
    }
}
