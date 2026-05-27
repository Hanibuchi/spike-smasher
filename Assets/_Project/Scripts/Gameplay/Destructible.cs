using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float requiredSizeLevel = 1.0f;
    public int scoreValue = 100;
    
    [Header("Collision Settings")]
    public GameObject collisionObject;
    [Tooltip("インスペクタのプルダウンから「1つだけ」チェックを入れてください")]
    public LayerMask collideLayer;
    [Tooltip("インスペクタのプルダウンから「1つだけ」チェックを入れてください")]
    public LayerMask noCollideLayer;

    public GameObject destructionEffectPrefab;
    public GameObject floatingTextPrefab;

    private void Start()
    {
        // 自身のスケールからrequiredSizeLevelを設定 (ここではXスケールを使用)
        requiredSizeLevel = transform.localScale.x;

        if (SpikeBall.Instance != null)
        {
            SpikeBall.Instance.OnSizeLevelChanged += CheckCollision;
            // 初期状態のチェック
            CheckCollision(SpikeBall.Instance.CurrentSizeLevel);
        }
    }

    private void OnDestroy()
    {
        if (SpikeBall.Instance != null)
        {
            SpikeBall.Instance.OnSizeLevelChanged -= CheckCollision;
        }
    }

    private void CheckCollision(float spikeBallSizeLevel)
    {
        if (spikeBallSizeLevel >= requiredSizeLevel)
        {
            SetCollisionEnabled(true);
        }
        else
        {
            SetCollisionEnabled(false);
        }
    }

    public void SetCollisionEnabled(bool canCollide)
    {
        if (collisionObject != null)
        {
            LayerMask targetLayerMask = canCollide ? collideLayer : noCollideLayer;
            
            // LayerMaskで何も選択されていない場合 (0) のエラーを防ぐ
            if (targetLayerMask.value != 0)
            {
                // 選択されたLayerMaskのビットからレイヤーインデックス（0～31）に変換 (※インスペクタで単一の指定が前提)
                int layerIndex = (int)Mathf.Log(targetLayerMask.value, 2);
                collisionObject.layer = layerIndex;
            }
            else
            {
                Debug.LogWarning("レイヤーが設定されていません。インスペクタの LayerMask の設定を確認してください。");
            }
        }
    }

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

        Destroy(transform.root.gameObject);
    }

    [ContextMenu("Test Enable Collision")]
    public void TestEnableCollision()
    {
        SetCollisionEnabled(true);
    }

    [ContextMenu("Test Disable Collision")]
    public void TestDisableCollision()
    {
        SetCollisionEnabled(false);
    }
}