using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;


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
        Lose,
        FinalHitImpactOpponent,
        Won
    }


    // serialized fields //

    [Header("Public Components")]

    [SerializeField] private ScreenShakeManager m_screenShakeManager;

    [SerializeField] private PlayerBehaviour m_player;
    [SerializeField] private OpponentBehaviour m_opponent;
    [SerializeField] private BallBehaviour m_ball;
    [SerializeField] private LoseScreenBehaviour m_loseScreen;

    [SerializeField] private TimerBehaviour m_timer;

    // [Header("Misc. Components")]

    // [SerializeField] private Animator m_titleHeader;

    // [AnimatorParam("m_titleHeader")]
    // [SerializeField] private string m_titleMoveUpTrigger;

    // [Space(10)]

    // [SerializeField] private Animator m_timerAnimator;

    // [AnimatorParam("m_timerAnimator")]
    // [SerializeField] private string m_timerSlideInTrigger;

    [Header("General Settings")]

    [SerializeField] private float m_centerLineHorizontal;

    [SerializeField] private float m_hitstopDurationRemainingBeforeOpponentMoves = 0.5f;

    [Layer]
    [SerializeField] private int m_swingHitboxLayer;

    [Tooltip("When the ball passes this x pos, the player loses")]
    [SerializeField] private float m_gameOverVerticalBoundary;

    [Tooltip("After this many hits on the ball, the opponent loses the next time the ball goes their way.")]
    public int numHitsToWin = 33;

    [Header("Hitstop Settings")]

    [Tooltip("Minimum and maximum impact duration is mapped to this range.  "
     + "Anything below minimum is set to 0, and anything above is clamped to max duration.")]
    public Vector2 impactSpeedRange;

    public AnimationCurve impactDurationCurve;


    [Header("Screen Shake Settings")]

    [Tooltip("Controls the magnitude of screen shake relative to the ball speed range given by Impact Speed Range above.")]
    public AnimationCurve screenShakeCurve;

    [Header("Debugging")]

    [SerializeField] private bool m_debugAutoMovePlayer = false;

    [Header("Final Hit Settings")]

    public float finalHitImpactDuration = 0.1f;

    [Tooltip("Time stops for a second when the ball contacts the opponent on the final hit.")]
    public float finalHitOpponentTimeStopDuration = 0.1f;

    public float finalHitImpactOpponentTimeScale = 0f;

    public float finalHitScreenShakeMagnitude = 1f;

    public float winDelay = 0.25f;

    [Space(10)]

    [Tooltip("Which way the ball goes after the final impact")]
    public Vector3 ballPostHitTarget = Vector3.up;

    [Space(10)]

    public UnityEvent onFirstGameStart;

    public UnityEvent onTwentySecondWin;

    public UnityEvent onInfiniteMode;

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

    public TimerBehaviour Timer
    {
        get
        {
            return m_timer;
        }
    }

    public int SwingHitboxLayer
    {
        get
        {
            return m_swingHitboxLayer;
        }
    }

    public float GameOverVerticalBoundary
    {
        get
        {
            return m_gameOverVerticalBoundary;
        }
    }


    // event listeners //

    void Awake()
    {
        // singleton stuff

        if(m_instance != null) throw new System.Exception("GameController instance already exists!");

        m_instance = this;
    }

    private PlayerWinManager m_playerWinManager = null;
    private HitstopManager m_currentImpactFrameManager = null;
    private bool m_isHitstopTriggered = false;

    private GameState m_currentGameState = GameState.Playing;

    private bool m_hasGameStartedEver = false;

    [Tooltip("THIS SHOULD BE OFF BY DEFAULT")]
    [SerializeField] private bool m_inInfiniteMode = false;

    public GameState GetCurrentGameState()
    {
        return m_currentGameState;
    }

    public float GetCurrentImpactDuration()
    {
        float t_p = m_ball.GetCurrentFlySpeed().Map(impactSpeedRange, new(0, 1));

        if(!m_inInfiniteMode && m_ball.GetCurrentNumberOfHits() == numHitsToWin)
        {
            return finalHitImpactDuration;
        }

        return impactDurationCurve.Evaluate(t_p);
    }

    void Update()
    {
        if(m_currentGameState == GameState.Playing)
        {
            PlayStateUpdate();
        } else if(m_currentGameState == GameState.Lose)
        {
            LoseStateUpdate();
        } else if(m_currentGameState == GameState.FinalHitImpactOpponent)
        {
            FinalHitImpactOpponentUpdate();
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
            RunOpponentLogic(m_opponent, m_player);
        } else
        {
            if (m_debugAutoMovePlayer)
            {
                RunOpponentLogic(m_player, m_opponent);
            } else
            {
                // normal movement
                if (m_player.AreSwingConditionsMet(out float distanceFromCollider))
                {
                    Transform target = m_opponent.GetOppositeMoveTargetTransform();

                    int t_numHits = m_ball.GetCurrentNumberOfHits();

                    if (!m_inInfiniteMode)
                    {
                        // NOTE: minus one because we haven't hit the ball yet
                        if(t_numHits == numHitsToWin - 1)
                        {
                            // nail that suckah
                            target = m_opponent.GetCurrentMoveTargetTransform();
                        } else if(t_numHits == numHitsToWin - 3)
                        {   
                            // on the second to last hit, scale the timer so that it doesn't go over 20
                            
                            // NOTE: assume that the next hit has the same duration as well
                            // also eyeball the flight speed (overestimate is better)
                            float timeLeft = GetCurrentImpactDuration()*2 + finalHitImpactDuration + 0.1f;
                            
                            float timerTimeLeft = 20 - m_timer.GetCurrentTime();

                            m_timer.internalTimeScale = Mathf.Min(1, timerTimeLeft / timeLeft);  
                        }
                    }
                    
                    if(t_numHits == 0)
                    {
                        if (!m_hasGameStartedEver)
                        {
                            onFirstGameStart.Invoke();
                            m_hasGameStartedEver = true;
                        }

                        // first hit, start timer
                        m_timer.StartTiming();
                    }

                    HitBallAs(m_player, target, distanceFromCollider);
                }
            }

            // check if player loses
            if(m_ball.transform.position.x < m_gameOverVerticalBoundary)
            {
                GoToLoseState();
            }
        }
    }

    void RunOpponentLogic(GenericTennisGuyBehaviour swinger, GenericTennisGuyBehaviour receiver)
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
                swinger.SetCurrentMoveTarget(GenericTennisGuyBehaviour.Target.Upper);
            } else
            {
                swinger.SetCurrentMoveTarget(GenericTennisGuyBehaviour.Target.Lower);
            }
        }
        
        if(m_ball.WouldBeHitIfGuySwung(swinger, out float distanceFromCollider))
        {
            if(m_inInfiniteMode || m_ball.GetCurrentNumberOfHits() < numHitsToWin)
            {
                GenericTennisGuyBehaviour.Target t_swingTarget = GenericTennisGuyBehaviour.GetRandomTarget();

                swinger.TrySwing(t_swingTarget);

                Transform t_targetTransform = receiver.GetMoveTargetTransform(t_swingTarget);

                // NOTE: verifies that the swing wasn't on cooldown
                if(swinger.IsSwinging()) HitBallAs(swinger, t_targetTransform, distanceFromCollider);
            } else
            {
                GoToFinalHitImpactOpponentState(winner: receiver, loser: swinger, distanceFromCollider: distanceFromCollider);
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

    void GoToFinalHitImpactOpponentState(GenericTennisGuyBehaviour winner, GenericTennisGuyBehaviour loser, float distanceFromCollider)
    {
        m_playerWinManager = new PlayerWinManager(this, winner, loser);

        m_playerWinManager.InitialImpact(distanceFromCollider);

        m_currentGameState = GameState.FinalHitImpactOpponent;
    }

    void FinalHitImpactOpponentUpdate()
    {
        m_playerWinManager.Update();

        if (m_playerWinManager.IsDone())
        {
            // listen for infinite mode
            if (m_loseScreen.GetResetInputAction().WasPressedThisFrame())
            {
                m_inInfiniteMode = true;
                onInfiniteMode.Invoke();
                ResetGame();
            }
        }
    }

    public void ResetGame()
    {
        m_timer.internalTimeScale = 1.0f;
        m_timer.SetText(0.0f);  
        m_player.Reset();
        m_opponent.Reset();
        m_ball.Reset();

        m_currentGameState = GameState.Playing;
    }

    void HitBallAs(GenericTennisGuyBehaviour tennisGuy, Transform target, float distanceFromCollider)
    {
        m_currentImpactFrameManager = new(this, tennisGuy);

        m_currentImpactFrameManager.InitialHit(target, distanceFromCollider);
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

    public void InitialHit(Transform target, float distanceFromCollider)
    {
        tennisGuy.ForceHitBall(target, distanceFromCollider);

        BallBehaviour ball = gameController.Ball;

        if(ball.GetCurrentFlySpeed() < gameController.impactSpeedRange.x)
        {
            // no impact frames needed
            m_isDone = true;
        } else
        {
            m_impactDuration = gameController.GetCurrentImpactDuration();
            
            float t_p = ball.GetCurrentFlySpeed().MapClamped(gameController.impactSpeedRange, new(0, 1));
            float t_screenShakeMagnitude = gameController.screenShakeCurve.Evaluate(t_p);

            gameController.ScreenShakeManager.Shake(t_screenShakeMagnitude);
        }

        m_hitTime = Time.time;

        // ensure update is called once immediately to prevent frame delays causing problems
        Update();
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


public class PlayerWinManager
{
    public GameController gameController;

    
    public GenericTennisGuyBehaviour winner;
    
    public GenericTennisGuyBehaviour loser;

    private float m_startTime = -1f;

    private bool m_exploded = false;

    private bool m_isDone = false;

    public PlayerWinManager(GameController gameController, GenericTennisGuyBehaviour winner, GenericTennisGuyBehaviour loser)
    {
        this.gameController = gameController;
        this.winner = winner;
        this.loser = loser;
    }

    public bool IsDone()
    {
        return m_isDone;
    }

    public void InitialImpact(float distanceFromCollider)
    {
        BallBehaviour ball = gameController.Ball;

        // NOTE: added slight margin so that we see the ball actually collide with the opponent
        ball.StepInCurrentDirection(distanceFromCollider + 1f);

        // override timer
        gameController.Timer.SetTime(20f);
        gameController.Timer.StopTiming();

        m_startTime = Time.unscaledTime;

        Time.timeScale = gameController.finalHitImpactOpponentTimeScale;

        // prevent frame delays causing problems
        Update();
    }

    public void Update()
    {
        if(IsDone()) return;

        // NOTE: the ball still needs to be frozen or else it goes offscreen for a frame
        // ('cause deltaTime is already calculated when we set timeScale to zero, and doesn't update till next frame)
        if(!m_exploded) gameController.Ball.Freeze();

        float timeSinceStart = Time.unscaledTime - m_startTime;

        if(timeSinceStart > gameController.finalHitOpponentTimeStopDuration)
        {
            if (!m_exploded)
            {
                // play final explosion
                Time.timeScale = 1f;

                gameController.ScreenShakeManager.Shake(gameController.finalHitScreenShakeMagnitude);
                
                gameController.Ball.Unfreeze();
                gameController.Ball.ResetHits();
                gameController.Ball.HitToTarget(gameController.ballPostHitTarget);

                loser.Explode();

                m_exploded = true;
            } else
            {
                if(timeSinceStart > gameController.finalHitOpponentTimeStopDuration + gameController.winDelay)
                {
                    gameController.onTwentySecondWin.Invoke();

                    m_isDone = true;
                }
            }
        }

    }
}