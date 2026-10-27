using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : GenericTennisGuyBehaviour
{
    [Header("Input Actions")]

    [SerializeField] private string m_upActionName;
    [SerializeField] private string m_downActionName;
    [SerializeField] private string m_hitActionName;


    // private //

    private InputAction m_upAction;
    private InputAction m_downAction;
    private InputAction m_hitAction;


    public override void Start()
    {
        base.Start();

        InitInputActions();
    }

    public override void Update()
    {
        base.Update();
        
        CheckMoveInputsAndSetTarget();
        CheckSwingInput();
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
            SetCurrentMoveTarget(Target.Upper);
        }

        if (m_downAction.WasPressedThisFrame())
        {
            SetCurrentMoveTarget(Target.Lower);
        }
    }

    void CheckSwingInput()
    {
        if (m_hitAction.WasPressedThisFrame())
        {
            Swing();
        }
    }
}
