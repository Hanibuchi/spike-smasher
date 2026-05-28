using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject gamePanel;
    public GameObject resultPanel;
    public CanvasGroup currentPanelGroup;

    [Header("Game UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public Color normalTextColor = Color.black;
    public Color warningTimerColor = Color.red;

    [Header("Result UI")]
    public TextMeshProUGUI resultScoreText;
    public Button startButton;
    public Button restartButton;
    public Button tweetButton;

    [Header("Sounds")]
    public AudioClip countDownSE;
    public AudioClip scoreCountUpSE;
    public AudioClip resultShowSE;

    private Coroutine scoreAnimationCoroutine;
    private Coroutine panelTransitionCoroutine;
    private Vector3 originalScoreScale;
    private int lastCountdownSecond = -1;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += HandleGameStateChanged;
        GameManager.Instance.OnScoreChanged += UpdateScoreUI;
        
        startButton.onClick.AddListener(() => GameManager.Instance.StartGame());
        restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());
        tweetButton.onClick.AddListener(TweetResult);
        
        if (scoreText != null)
        {
            originalScoreScale = scoreText.transform.localScale;
        }

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
            float timeRemain = GameManager.Instance.timeRemaining;
            int currentSecond = Mathf.CeilToInt(timeRemain);
            timerText.text = currentSecond.ToString();

            // 残り時間が少ない場合の演出 (赤く点滅)
            if (timeRemain <= 10f && timeRemain > 0f)
            {
                timerText.color = Color.Lerp(normalTextColor, warningTimerColor, Mathf.PingPong(Time.time * 4f, 1f));
                timerText.transform.localScale = Vector3.one * (1f + 0.1f * Mathf.Sin(Time.time * 20f));

                // 1秒ごとにカウントダウンSEを鳴らす
                if (currentSecond != lastCountdownSecond)
                {
                    lastCountdownSecond = currentSecond;
                    if (SoundManager.Instance != null && countDownSE != null)
                    {
                        SoundManager.Instance.PlaySE(countDownSE);
                    }
                }
            }
            else
            {
                timerText.color = normalTextColor;
                timerText.transform.localScale = Vector3.one;
                lastCountdownSecond = -1; // リセット
            }
        }
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        if (panelTransitionCoroutine != null) StopCoroutine(panelTransitionCoroutine);

        titlePanel.SetActive(state == GameManager.GameState.Title);
        gamePanel.SetActive(state == GameManager.GameState.Playing);
        resultPanel.SetActive(state == GameManager.GameState.Result);

        // アクティブなパネルを少しアニメーションさせる
        GameObject activePanel = null;
        if (state == GameManager.GameState.Title) activePanel = titlePanel;
        if (state == GameManager.GameState.Playing) activePanel = gamePanel;
        if (state == GameManager.GameState.Result) activePanel = resultPanel;

        if (activePanel != null)
        {
            panelTransitionCoroutine = StartCoroutine(AnimatePanelIn(activePanel.transform));
        }

        if (state == GameManager.GameState.Result)
        {
            resultScoreText.text = "スコア: " + GameManager.Instance.currentScore;
            StartCoroutine(AnimateScoreCountUp(resultScoreText, GameManager.Instance.currentScore));
        }
    }

    private IEnumerator AnimatePanelIn(Transform panelTransform)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 initialScale = Vector3.one * 0.8f;
        panelTransform.localScale = initialScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // イーズアウトのアニメーション
            t = 1f - (1f - t) * (1f - t);
            panelTransform.localScale = Vector3.Lerp(initialScale, Vector3.one, t);
            yield return null;
        }
        panelTransform.localScale = Vector3.one;
    }

    private void UpdateScoreUI(int score)
    {
        scoreText.text = "スコア: " + score;
        if (gameObject.activeInHierarchy)
        {
            if (scoreAnimationCoroutine != null) StopCoroutine(scoreAnimationCoroutine);
            scoreAnimationCoroutine = StartCoroutine(AnimateScorePop());
        }
    }

    private IEnumerator AnimateScorePop()
    {
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 targetScale = originalScoreScale * 1.5f;

        // スケールアップ
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            scoreText.transform.localScale = Vector3.Lerp(originalScoreScale, targetScale, elapsed / duration);
            scoreText.color = Color.Lerp(Color.yellow, normalTextColor, elapsed / duration);
            yield return null;
        }

        // スケールダウン
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScoreScale, elapsed / duration);
            yield return null;
        }
        
        scoreText.transform.localScale = originalScoreScale;
    }

    private IEnumerator AnimateScoreCountUp(TextMeshProUGUI tmpText, int targetScore)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        int lastCurrent = -1;
        float lastSETime = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int current = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, elapsed / duration));
            
            if (current != lastCurrent)
            {
                lastCurrent = current;
                tmpText.text = "スコア: " + current;
                
                // 音が重なりすぎてチャンネルが枯渇し、最後の再生音が消えるのを防ぐため再生間隔を制限
                if (SoundManager.Instance != null && scoreCountUpSE != null)
                {
                    if (Time.unscaledTime - lastSETime > 0.05f)
                    {
                        SoundManager.Instance.PlaySE(scoreCountUpSE);
                        lastSETime = Time.unscaledTime;
                    }
                }
            }
            yield return null;
        }
        tmpText.text = "スコア: " + targetScore;
        
        // カウントアップ音と被らないように少しだけ待機
        yield return new WaitForSeconds(0.1f);
        
        // 結果発表時の音
        if (SoundManager.Instance != null && resultShowSE != null)
        {
            SoundManager.Instance.PlaySE(resultShowSE);
        }
    }

    private void TweetResult()
    {
        int score = GameManager.Instance.currentScore;
        string text = $"スパイク スマッシャー で {score} 点を獲得した！ #スパイクスマッシャー";
        string url = "https://twitter.com/intent/tweet?text=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(text);
        Application.OpenURL(url);
    }
}