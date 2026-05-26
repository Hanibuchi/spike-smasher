using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject gamePanel;
    public GameObject resultPanel;

    [Header("Game UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Result UI")]
    public TextMeshProUGUI resultScoreText;
    public Button startButton;
    public Button restartButton;
    public Button tweetButton;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += HandleGameStateChanged;
        GameManager.Instance.OnScoreChanged += UpdateScoreUI;
        
        startButton.onClick.AddListener(() => GameManager.Instance.StartGame());
        restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());
        tweetButton.onClick.AddListener(TweetResult);
        
        HandleGameStateChanged(GameManager.Instance.CurrentState);
        UpdateScoreUI(0);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
            GameManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            timerText.text = Mathf.CeilToInt(GameManager.Instance.timeRemaining).ToString();
        }
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        titlePanel.SetActive(state == GameManager.GameState.Title);
        gamePanel.SetActive(state == GameManager.GameState.Playing);
        resultPanel.SetActive(state == GameManager.GameState.Result);

        if (state == GameManager.GameState.Result)
        {
            resultScoreText.text = "Score: " + GameManager.Instance.currentScore;
        }
    }

    private void UpdateScoreUI(int score)
    {
        scoreText.text = "Score: " + score;
    }

    private void TweetResult()
    {
        int score = GameManager.Instance.currentScore;
        string text = $"街破壊ゲームで {score} 点を獲得した！ #街破壊スパイクボール";
        string url = "https://twitter.com/intent/tweet?text=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(text);
        Application.OpenURL(url);
    }
}