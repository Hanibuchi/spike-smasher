using UnityEngine;

public class SpikeBall : MonoBehaviour
{
    public float minVelocityToDestroy = 1.0f;
    public float sizeGrowthFactor = 0.001f;
    
    private Rigidbody rb;
    private float currentSizeLevel = 1.0f;
    private Vector3 initialScale;
    private float initialMass;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialScale = transform.localScale;
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
        
        transform.localScale = initialScale * currentSizeLevel;
        if (rb != null)
        {
            rb.mass = initialMass * currentSizeLevel;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        if (rb.linearVelocity.magnitude >= minVelocityToDestroy)
        {
            Destructible target = collision.gameObject.GetComponent<Destructible>();
            if (target != null)
            {
                if (currentSizeLevel >= target.requiredSizeLevel)
                {
                    target.DestroyObject();
                }
            }
        }
    }
}