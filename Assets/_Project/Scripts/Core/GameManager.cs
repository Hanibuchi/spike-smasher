using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Title, Playing, Result }
    public GameState CurrentState { get; private set; } = GameState.Title;

    public float gameDuration = 60f;
    public float timeRemaining { get; private set; }
    public int currentScore { get; private set; }

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
}