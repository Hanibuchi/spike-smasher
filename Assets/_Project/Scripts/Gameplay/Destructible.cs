using System.Collections;
using System.Collections.Generic;
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

    [Header("Effects & Sounds")]
    public GameObject destructionEffectPrefab;
    public GameObject floatingTextPrefab;
    [Tooltip("破壊時に再生されるSE（複数設定するとランダムで再生）")]
    public AudioClip[] destroySEs;

    [System.Serializable]
    public class ScoreBallSetting
    {
        public GameObject prefab;
        [Tooltip("飛び散るスコアボールへ与える力の最小値")]
        public float flyForceMin = 5f;
        [Tooltip("飛び散るスコアボールへ与える力の最大値")]
        public float flyForceMax = 12f;
    }

    [Header("Score Ball Settings")]
    public ScoreBallSetting smallScoreBall = new ScoreBallSetting { flyForceMin = 5f, flyForceMax = 12f };
    public ScoreBallSetting mediumScoreBall = new ScoreBallSetting { flyForceMin = 10f, flyForceMax = 20f };
    public ScoreBallSetting largeScoreBall = new ScoreBallSetting { flyForceMin = 20f, flyForceMax = 40f };

    [Tooltip("生成されるスコアボールの基準色。指定した色の明度(V)と彩度(S)が使用され、色相(H)はランダムになります。")]
    public Color scoreBallBaseColor = new Color(0.9f, 0.18f, 0.18f);

    [Header("Lifetime Settings")]
    [Tooltip("時間経過で自動で消滅するかどうか")]
    public bool hasLifetime = true;
    [Tooltip("生成されてから消滅するまでの時間（秒）")]
    public float lifetimeDuration = 20f;

    private void Start()
    {
        if (SpikeBall.Instance != null)
        {
            SpikeBall.Instance.OnSizeLevelChanged += CheckCollision;
        }

        // オブジェクトが初めからシーンに配置されている場合の初期化
        Initialize(transform.localScale.x);

        if (hasLifetime)
        {
            StartCoroutine(LifetimeRoutine());
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        // 縮小アニメーションにかける時間
        float disappearAnimDuration = 0.5f;
        // アニメーションを開始するまでの待機時間（最低0秒を担保）
        float waitTime = Mathf.Max(0f, lifetimeDuration - disappearAnimDuration);
        
        yield return new WaitForSeconds(waitTime);

        // スケールを徐々に小さくするアニメーション
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < disappearAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / disappearAnimDuration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(transform.root.gameObject);
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
        if (SoundManager.Instance != null && destroySEs != null && destroySEs.Length > 0)
        {
            SoundManager.Instance.PlayRandomSE(destroySEs);
        }

        int remainingScore = scoreValue;
        
        List<ScoreBallSetting> settings = new List<ScoreBallSetting>();
        if (largeScoreBall != null && largeScoreBall.prefab != null) settings.Add(largeScoreBall);
        if (mediumScoreBall != null && mediumScoreBall.prefab != null) settings.Add(mediumScoreBall);
        if (smallScoreBall != null && smallScoreBall.prefab != null) settings.Add(smallScoreBall);

        // スコアが大きい順にソート（大きいボールから優先して生成するため）
        settings.Sort((a, b) => 
        {
            int scoreA = Mathf.Max(1, a.prefab.GetComponent<ScoreBall>()?.scoreAmount ?? 1);
            int scoreB = Mathf.Max(1, b.prefab.GetComponent<ScoreBall>()?.scoreAmount ?? 1);
            return scoreB.CompareTo(scoreA); // 降順
        });

        foreach (var setting in settings)
        {
            int scoreAmount = Mathf.Max(1, setting.prefab.GetComponent<ScoreBall>()?.scoreAmount ?? 1);
            int spawnCount = remainingScore / scoreAmount;
            remainingScore %= scoreAmount;

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject ballObj = Instantiate(setting.prefab, transform.position + (Vector3.up * 1.5f), Quaternion.identity);
                
                // 飛び散る処理
                Rigidbody ballRb = ballObj.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector3 randomDir = Random.onUnitSphere;
                    if (randomDir.y <= 0.2f) randomDir.y = Random.Range(0.5f, 1f);
                    randomDir.Normalize();
                    
                    float force = Random.Range(setting.flyForceMin, setting.flyForceMax);
                    ballRb.AddForce(randomDir * force, ForceMode.Impulse);
                }

                // 色の変更処理
                Renderer ballRenderer = ballObj.GetComponentInChildren<Renderer>();
                if (ballRenderer != null)
                {
                    float randomHue = Random.Range(0f, 1f);
                    Color.RGBToHSV(scoreBallBaseColor, out float _, out float s, out float v);
                    Color newColor = Color.HSVToRGB(randomHue, s, v);
                    ballRenderer.material.color = newColor;
                }
            }
        }

        if (remainingScore > 0 || settings.Count == 0)
        {
            // 端数のスコア、またはプレハブが未設定の場合は直接加算
            GameManager.Instance.AddScore(settings.Count == 0 ? scoreValue : remainingScore);
        }

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

            // スコアの値に応じてテキストの大きさを変更（基本スコア10をスケール1.0の基準とする）
            float textScale = Mathf.Max(0.5f, scoreValue / 10f);
            floatingText.transform.localScale = Vector3.one * textScale;
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