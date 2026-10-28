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


    // constants and such //

    public enum GameState
    {
        Playing,
        Lose
    }


    // serialized fields //

    [Header("Components")]

    [SerializeField] private ScreenShakeManager m_screenShakeManager;

    [SerializeField] private PlayerBehaviour m_player;
    [SerializeField] private OpponentBehaviour m_opponent;
    [SerializeField] private BallBehaviour m_ball;
    [SerializeField] private LoseScreenBehaviour m_loseScreen;


    [Header("General Settings")]

    [SerializeField] private float m_centerLineHorizontal;

    [SerializeField] private float m_hitstopDurationRemainingBeforeOpponentMoves = 0.5f;

    [Layer]
    [SerializeField] private int m_swingHitboxLayer;

    [Tooltip("When the ball passes this x pos, the player loses")]
    [SerializeField] private float m_gameOverVerticalBoundary;

    [Header("Hitstop Settings")]

    [Tooltip("Minimum and maximum impact duration is mapped to this range.  "
     + "Anything below minimum is set to 0, and anything above is clamped to max duration.")]
    public Vector2 impactSpeedRange;

    public AnimationCurve impactDurationCurve;

    [Header("Screen Shake Settings")]

    [Tooltip("Controls the magnitude of screen shake relative to the ball speed range given by Impact Speed Range above.")]
    public AnimationCurve screenShakeCurve;


    // public properties //

    public ScreenShakeManager ScreenShakeManager
    {
        get
        {
            return m_screenShakeManager;
        }
    }

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

    private HitstopManager m_currentImpactFrameManager = null;
    private bool m_isHitstopTriggered = false;

    private GameState m_currentGameState = GameState.Playing;

    public GameState GetCurrentGameState()
    {
        return m_currentGameState;
    }

    void Update()
    {
        if(m_currentGameState == GameState.Playing)
        {
            PlayStateUpdate();
        } else if(m_currentGameState == GameState.Lose)
        {
            LoseStateUpdate();
        }
        
    }
    
    void PlayStateUpdate()
    {
        if(m_currentImpactFrameManager != null)
        {
            m_currentImpactFrameManager.Update();

            if (m_currentImpactFrameManager.IsDone())
            {
                m_currentImpactFrameManager = null;
            } else {
                // if we last a frame
                m_isHitstopTriggered = true;
            }
        }

        if (m_ball.IsMovingTowardsOpponent())
        {
            // in non-hitstop, have reactive behaviour
            Vector3 t_ballProjectedPosition = m_ball.GetProjectedPosition();

            // otherwise be more predictive
            if (m_isHitstopTriggered)
            {
                t_ballProjectedPosition = m_ball.GetCurrentDirection()*10 + m_ball.transform.position;
            }

            // if we're hitstopping, wait a second just for fun
            if(m_currentImpactFrameManager == null || m_currentImpactFrameManager.GetHitstopDurationRemaining() < m_hitstopDurationRemainingBeforeOpponentMoves)
            {
                if(t_ballProjectedPosition.y > m_centerLineHorizontal)
                {
                    m_opponent.SetCurrentMoveTarget(GenericTennisGuyBehaviour.Target.Upper);
                } else
                {
                    m_opponent.SetCurrentMoveTarget(GenericTennisGuyBehaviour.Target.Lower);
                }
            }
            
            if(m_ball.WouldBeHitIfGuySwung(m_opponent))
            {
                GenericTennisGuyBehaviour.Target t_swingTarget = GenericTennisGuyBehaviour.GetRandomTarget();

                m_opponent.Swing(t_swingTarget);

                Transform t_targetTransform = m_player.GetMoveTargetTransform(t_swingTarget);

                // we just assume that opponent hit ball (valid assumption because we satisfy every condition)
                HitBallAs(m_opponent, t_targetTransform);
            }
        } else
        {
            if (m_player.AreSwingConditionsMet())
            {
                HitBallAs(m_player, m_opponent.GetOppositeMoveTargetTransform());
            } else
            {
                // check if player loses
                if(m_ball.transform.position.x < m_gameOverVerticalBoundary)
                {
                    GoToLoseState();
                }
            }
        }
    }

    void GoToLoseState()
    {
        m_loseScreen.Show();

        m_currentGameState = GameState.Lose;    
    }

    void LoseStateUpdate()
    {
        
    }

    public void ResetGame()
    {
        m_player.Reset();
        m_opponent.Reset();
        m_ball.Reset();

        m_currentGameState = GameState.Playing;
    }

    void HitBallAs(GenericTennisGuyBehaviour tennisGuy, Transform target)
    {
        m_currentImpactFrameManager = new(this, tennisGuy);

        m_currentImpactFrameManager.InitialHit(target);
    }
}


public class HitstopManager
{
    public GameController gameController;

    public GenericTennisGuyBehaviour tennisGuy;

    private bool m_isDone = false;

    private float m_impactDuration = 0f;
    private float m_hitTime = -1f;

    public HitstopManager(GameController gameController, GenericTennisGuyBehaviour tennisGuy)
    {
        this.gameController = gameController;
        this.tennisGuy = tennisGuy;
    }

    public bool IsDone()
    {
        return m_isDone;
    }

    public float GetHitstopDurationRemaining()
    {
        return m_hitTime + m_impactDuration - Time.time;
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

            float t_p = ball.GetCurrentFlySpeed().MapClamped(impactSpeedRange, new(0, 1));

            m_impactDuration = gameController.impactDurationCurve.Evaluate(t_p);

            // do screen shake
            float t_screenShakeMagnitude = gameController.screenShakeCurve.Evaluate(t_p);

            gameController.ScreenShakeManager.Shake(t_screenShakeMagnitude);
        }

        m_hitTime = Time.time;
    }

    public void Update()
    {
        if(m_isDone) return;

        if(GetHitstopDurationRemaining() > 0)
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