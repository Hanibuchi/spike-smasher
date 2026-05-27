using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float requiredSizeLevel = 1.0f;
    public int scoreValue = 10;

    [Header("Visual & Scaling Settings")]
    public Renderer targetRenderer;
    [Tooltip("このカラープロパティの彩度(S)・明度(V)・透明度(A)が使用されます。色相(H)はスケールに応じて自動で変化します。")]
    public Color baseColor = new Color(0.9f, 0.18f, 0.18f); // 初期値をS=0.8, V=0.9付近に設定
    [Tooltip("色相変化やスコアの基準となる最小スケール")]
    public float minScale = 1.0f;
    [Tooltip("色相変化やスコアの基準となる最大スケール")]
    public float maxScale = 5.0f;
    [Tooltip("スケール1.0の時の基本スコア")]
    public int baseScore = 10;
    
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
        if (SpikeBall.Instance != null)
        {
            SpikeBall.Instance.OnSizeLevelChanged += CheckCollision;
        }

        // オブジェクトが初めからシーンに配置されている場合の初期化
        Initialize(transform.localScale.x);
    }

    public void Initialize(float scale)
    {
        // スケールと要求サイズの更新
        transform.localScale = new Vector3(scale, scale, scale);
        requiredSizeLevel = scale;

        // Scaleに基づくスコアの変更
        scoreValue = Mathf.RoundToInt(baseScore * scale);

        // Rendererの色相の変更
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            // スケールをminScale～maxScaleの範囲で0.0～1.0に正規化
            float t = Mathf.InverseLerp(minScale, maxScale, scale);
            
            // 色相 (0.66が寒色の青、0.0が暖色の赤)
            float hue = Mathf.Lerp(0.66f, 0.0f, t);
            
            // baseColorから彩度と明度を抽出
            Color.RGBToHSV(baseColor, out float _, out float s, out float v);
            
            // 抽出した彩度・明度、および元のアルファ値を使用して新しい色を作成
            Color finalColor = Color.HSVToRGB(hue, s, v);
            finalColor.a = baseColor.a;

            targetRenderer.material.color = finalColor;
        }

        // 生成時のサイズの当たり判定チェック
        if (SpikeBall.Instance != null)
        {
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
            GameObject effect = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            
            float maxDuration = 2f;

            foreach (var ps in particleSystems)
            {
                var main = ps.main;

                if (targetRenderer != null && targetRenderer.material != null)
                {
                    main.startColor = targetRenderer.material.color;
                }

                main.startSizeMultiplier *= transform.localScale.x;

                float lifeTime = main.duration + main.startLifetime.constantMax;
                if (lifeTime > maxDuration && lifeTime < 100f)
                {
                    maxDuration = lifeTime;
                }
            }

            Destroy(effect, maxDuration);
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