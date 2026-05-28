using UnityEngine;
using System;
using unityroom.Api;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Title, Playing, Result }
    public GameState CurrentState { get; private set; } = GameState.Title;

    public float gameDuration = 60f;
    public float timeRemaining { get; private set; }
    public int currentScore { get; private set; }

    [Header("Sounds")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip gameStartSE;
    [SerializeField] private AudioClip gameEndSE;

    public event Action<GameState> OnStateChanged;
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (SoundManager.Instance != null && bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip);
        }
        ChangeState(GameState.Title);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                
                if (SoundManager.Instance != null && gameEndSE != null)
                {
                    SoundManager.Instance.PlaySE(gameEndSE);
                }
                
                UnityroomApiClient.Instance.SendScore(1, currentScore, ScoreboardWriteMode.HighScoreDesc);
                
                ChangeState(GameState.Result);
            }
        }
    }

    public void StartGame()
    {
        if (CurrentState == GameState.Title)
        {
            timeRemaining = gameDuration;
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
            
            if (SoundManager.Instance != null && gameStartSE != null)
            {
                SoundManager.Instance.PlaySE(gameStartSE);
            }
            
            ChangeState(GameState.Playing);
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void AddScore(int amount)
    {
        if (CurrentState != GameState.Playing) return;
        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    [Header("Debug")]
    [SerializeField] private int debugScoreToAdd = 100;

    [ContextMenu("Debug Add Score")]
    private void DebugAddScore()
    {
        if (Application.isPlaying)
        {
            AddScore(debugScoreToAdd);
        }
    }
}