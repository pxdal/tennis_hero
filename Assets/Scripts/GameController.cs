using System.Reflection.Metadata.Ecma335;
using NaughtyAttributes;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // singleton stuff //

    private static GameController m_instance = null;

    public static GameController Instance
    {
        get
        {
            if(!m_instance) throw new System.Exception("no GameController instance available");

            return m_instance;
        }
    }


    // serialized fields //

    [Header("Components")]

    [SerializeField] private PlayerBehaviour m_player;
    [SerializeField] private OpponentBehaviour m_opponent;
    [SerializeField] private BallBehaviour m_ball;

    [Header("Settings")]

    [SerializeField] private float m_centerLineHorizontal;

    [Tooltip("Once the ball is past this point, the opponent hits it")]
    [SerializeField] private float m_opponentHitBallXPosition;

    [Layer]
    [SerializeField] private int m_swingHitboxLayer;

    // public properties //
    
    public PlayerBehaviour Player
    {
        get
        {
            return m_player;
        }
    }

    public OpponentBehaviour Opponent
    {
        get
        {
            return m_opponent;
        }
    }

    public BallBehaviour Ball
    {
        get
        {
            return m_ball;
        }
    }

    public int SwingHitboxLayer
    {
        get
        {
            return m_swingHitboxLayer;
        }
    }


    // event listeners //

    void Awake()
    {
        // singleton stuff

        if(m_instance != null) throw new System.Exception("GameController instance already exists!");

        m_instance = this;
    }

    void Start()
    {    
        m_ball.SetCurrentDirectionFromTargetPosition(m_opponent.GetCurrentMoveTargetTransform());
    }

    void Update()
    {
        if (m_ball.IsMovingTowardsOpponent())
        {
            if(m_ball.GetProjectedPosition().y > m_centerLineHorizontal)
            {
                m_opponent.SetCurrentTarget(GenericTennisGuyBehaviour.Target.Upper);
            } else
            {
                m_opponent.SetCurrentTarget(GenericTennisGuyBehaviour.Target.Lower);
            }

            if(m_ball.WouldBeHitIfGuySwung(m_opponent))
            {
                m_opponent.Swing();

                Transform newTarget = m_player.GetMoveTargetTransform(GenericTennisGuyBehaviour.GetRandomTarget());

                m_opponent.HitBallToTargetIfSwingConditionsAreMet(newTarget);
            }
        } else
        {
            if (m_player.IsSwinging())
            {
                m_player.HitBallToTargetIfSwingConditionsAreMet(m_opponent.GetOppositeMoveTargetTransform());
            }
        }
    }
}
