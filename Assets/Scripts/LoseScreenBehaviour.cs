using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LoseScreenBehaviour : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;

    [Header("Animator Settings")]

    [AnimatorParam("animator")]
    [SerializeField] private string m_popUpTriggerKey;

    [AnimatorParam("animator")]
    [SerializeField] private string m_resetTriggerKey;

    [Header("Input Settings")]

    [SerializeField] private string m_resetInputActionName;

    [Header("Events")]

    public UnityEvent onRestart;

    // private //

    private InputAction m_resetInputAction;

    private bool m_isShowing = false;


    void Start()
    {
        m_resetInputAction = InputSystem.actions.FindAction(m_resetInputActionName);    
    }

    public bool IsShowing()
    {
        return m_isShowing;
    }
    
    public void Show()
    {
        if(IsShowing()) return;

        m_isShowing = true;

        animator.SetTrigger(m_popUpTriggerKey);
    }

    void Hide()
    {
        if(!IsShowing()) return;

        m_isShowing = false;

        animator.SetTrigger(m_resetTriggerKey);
    }

    void Update()
    {
        if (!IsShowing()) return;

        if (m_resetInputAction.WasPressedThisFrame())
        {
            Hide();

            onRestart.Invoke();
        }
    }
}
