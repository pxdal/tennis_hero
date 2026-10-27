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

    [Header("General Settings")]

    [SerializeField] private float m_centerLineHorizontal;

    [Tooltip("Once the ball is past this point, the opponent hits it")]
    [SerializeField] private float m_opponentHitBallXPosition;

    [Layer]
    [SerializeField] private int m_swingHitboxLayer;

    [Header("Impact Frame Settings")]

    [Tooltip("Minimum and maximum impact duration is mapped to this range.  "
     + "Anything below minimum is set to 0, and anything above is clamped to max duration.")]
    public Vector2 impactSpeedRange;

    [Tooltip("Minimum and maximum impact duration.")]
    [MinMaxSlider(0.0f, 5f)]
    public Vector2 impactDurationRange;

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


    private ImpactFrameManager m_currentImpactFrameManager = null;

    void Update()
    {
        if(m_currentImpactFrameManager != null)
        {
            m_currentImpactFrameManager.Update();

            if (m_currentImpactFrameManager.IsDone())
            {
                m_currentImpactFrameManager = null;
            } 
        }

        if (m_ball.IsMovingTowardsOpponent())
        {
            if(m_ball.GetProjectedPosition().y > m_centerLineHorizontal)
            {
                m_opponent.SetCurrentMoveTarget(GenericTennisGuyBehaviour.Target.Upper);
            } else
            {
                m_opponent.SetCurrentMoveTarget(GenericTennisGuyBehaviour.Target.Lower);
            }

            if(m_ball.WouldBeHitIfGuySwung(m_opponent))
            {
                m_opponent.Swing();

                Transform newTarget = m_player.GetMoveTargetTransform(GenericTennisGuyBehaviour.GetRandomTarget());

                // we just assume that opponent hit ball (valid assumption because we satisfy every condition)
                HitBallAs(m_opponent, newTarget);
            }
        } else
        {
            if (m_player.AreSwingConditionsMet())
            {
                HitBallAs(m_player, m_opponent.GetOppositeMoveTargetTransform());
            }
        }
    }
    

    void HitBallAs(GenericTennisGuyBehaviour tennisGuy, Transform target)
    {
        m_currentImpactFrameManager = new(this, tennisGuy);

        m_currentImpactFrameManager.InitialHit(target);
    }
}


public class ImpactFrameManager
{
    public GameController gameController;

    public GenericTennisGuyBehaviour tennisGuy;

    private bool m_isDone = false;

    private float m_impactDuration = 0f;
    private float m_hitTime = -1f;

    public ImpactFrameManager(GameController gameController, GenericTennisGuyBehaviour tennisGuy)
    {
        this.gameController = gameController;
        this.tennisGuy = tennisGuy;
    }

    public bool IsDone()
    {
        return m_isDone;
    }

    public void InitialHit(Transform target)
    {
        tennisGuy.ForceHitBall(target);

        BallBehaviour ball = gameController.Ball;

        if(ball.GetCurrentFlySpeed() < gameController.impactSpeedRange.x)
        {
            // no impact frames needed
            m_isDone = true;
        } else
        {
            Vector2 impactSpeedRange = gameController.impactSpeedRange;
            Vector2 impactDurationRange = gameController.impactDurationRange;

            m_impactDuration = ball.GetCurrentFlySpeed().MapClamped(impactSpeedRange, impactDurationRange);
        }

        m_hitTime = Time.time;
    }

    public void Update()
    {
        if(m_isDone) return;

        if(Time.time < m_hitTime + m_impactDuration)
        {
            BallBehaviour ball = gameController.Ball;

            ball.Freeze();
            tennisGuy.Freeze();
        } else
        {
            Finish();
        }
    }

    void Finish()
    {
        gameController.Ball.Unfreeze();
        tennisGuy.Unfreeze();

        m_isDone = true;
    }
}