using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float requiredSizeLevel = 1.0f;
    public int scoreValue = 100;
    
    public GameObject destructionEffectPrefab;
    public GameObject floatingTextPrefab;

    public void DestroyObject()
    {
        GameManager.Instance.AddScore(scoreValue);

        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }

        if (floatingTextPrefab != null)
        {
            GameObject floatingText = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            FloatingText ft = floatingText.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.SetText("+" + scoreValue);
            }
        }

        Destroy(gameObject);
    }
}