using UnityEngine;

public class SpikeBall : MonoBehaviour
{
    public static SpikeBall Instance { get; private set; }

    public float minVelocityToDestroy = 1.0f;
    public float sizeGrowthFactor = 0.002f;
    
    private Rigidbody rb;
    [SerializeField] float currentSizeLevel = 1.0f;
    public float CurrentSizeLevel => currentSizeLevel;
    [SerializeField] private Transform targetTransform;
    private Vector3 initialScale;
    private float initialMass;

    public event System.Action<float> OnSizeLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (targetTransform == null)
        {
            targetTransform = transform;
        }
        initialScale = targetTransform.localScale;
        if (rb != null)
        {
            initialMass = rb.mass;
        }

        GameManager.Instance.OnScoreChanged += UpdateSize;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateSize;
        }
    }

    private void UpdateSize(int currentScore)
    {
        currentSizeLevel = 1.0f + (currentScore * sizeGrowthFactor);
        
        if (targetTransform != null)
        {
            targetTransform.localScale = initialScale * currentSizeLevel;
        }
        if (rb != null)
        {
            rb.mass = initialMass * currentSizeLevel;
        }
        
        OnSizeLevelChanged?.Invoke(currentSizeLevel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        if (rb.linearVelocity.magnitude >= minVelocityToDestroy)
        {
            Destructible target = other.gameObject.GetComponentInParent<Destructible>();
            if (target != null)
            {
                if (currentSizeLevel >= target.requiredSizeLevel)
                {
                    target.DestroyObject();
                }
            }
        }
    }

    /// <summary>
    /// SpikeBallの現在のスケールレベルに応じて値を線形に変化させて返す専用メソッド
    /// </summary>
    public float GetScaledValue(float baseValue)
    {
        return baseValue * currentSizeLevel;
    }
}