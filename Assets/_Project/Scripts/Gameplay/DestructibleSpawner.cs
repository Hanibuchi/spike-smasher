using UnityEngine;

public class DestructibleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("生成するDestructibleのプレハブ")]
    public GameObject destructiblePrefab;
    
    [Tooltip("生成を行う間隔（秒）")]
    public float spawnInterval = .5f;
    
    [Tooltip("生成を行う基本半径（SpikeBallのScaleの値が掛けられます）")]
    public float spawnRadiusBase = 20f;
    
    [Tooltip("生成を行わない最小基本半径（プレイヤー周辺の安全圏。この値もSpikeBallのScaleの値が掛けられます）")]
    public float minSpawnRadiusBase = 5f;
    
    [Tooltip("一定範囲内に存在できる生成済みDestructibleオブジェクトの最大数")]
    public int maxDestructiblesInRadius = 40;
    
    [System.Serializable]
    public class SpawnProfile
    {
        [Tooltip("これが選ばれる確率の重み（例: 80と20なら、およそ8:2の割合）")]
        public float weight;
        [Tooltip("このサイズの最小の倍率")]
        public float minScaleMultiplier;
        [Tooltip("このサイズの最大の倍率")]
        public float maxScaleMultiplier;
    }

    [Header("Spawn Profiles")]
    [Tooltip("よくスポーンする小さめのオブジェクトの設定")]
    public SpawnProfile smallProfile = new SpawnProfile { weight = 80f, minScaleMultiplier = 0.8f, maxScaleMultiplier = 1.0f };
    
    [Tooltip("そこそこスポーンする中くらいのオブジェクトの設定")]
    public SpawnProfile mediumProfile = new SpawnProfile { weight = 16f, minScaleMultiplier = 1.5f, maxScaleMultiplier = 2.0f };

    [Tooltip("たまにしかスポーンしない大きなオブジェクトの設定")]
    public SpawnProfile largeProfile = new SpawnProfile { weight = 4f, minScaleMultiplier = 3f, maxScaleMultiplier = 5f };

    [Header("Collision Settings")]
    [Tooltip("生成数をカウントするためのレイヤー（Destructibleオブジェクトが持つレイヤーを指定してください）")]
    public LayerMask destructibleLayer;

    private float spawnTimer = 0f;

    private void Start()
    {
        SpawnUpToMax();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawn();
        }
    }

    private void SpawnUpToMax()
    {
        if (destructiblePrefab == null || SpikeBall.Instance == null) return;

        float currentScaleLevel = SpikeBall.Instance.CurrentSizeLevel;
        float currentSpawnRadius = SpikeBall.Instance.GetScaledValue(spawnRadiusBase);
        float currentMinSpawnRadius = SpikeBall.Instance.GetScaledValue(minSpawnRadiusBase);

        int currentCount = GetCurrentDestructibleCount(currentSpawnRadius);
        int spawnAmount = maxDestructiblesInRadius - currentCount;

        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnDestructible(currentScaleLevel, currentMinSpawnRadius, currentSpawnRadius);
        }
    }

    private void TrySpawn()
    {
        if (destructiblePrefab == null || SpikeBall.Instance == null) return;

        float currentScaleLevel = SpikeBall.Instance.CurrentSizeLevel;
        // SpikeBallのスケールに応じて半径を変更
        float currentSpawnRadius = SpikeBall.Instance.GetScaledValue(spawnRadiusBase);
        float currentMinSpawnRadius = SpikeBall.Instance.GetScaledValue(minSpawnRadiusBase);

        int currentCount = GetCurrentDestructibleCount(currentSpawnRadius);

        // 最大数に達していない場合のみ生成
        if (currentCount < maxDestructiblesInRadius)
        {
            SpawnDestructible(currentScaleLevel, currentMinSpawnRadius, currentSpawnRadius);
        }
    }

    private int GetCurrentDestructibleCount(float radius)
    {
        // 範囲内のオブジェクトをカウント
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, destructibleLayer);
        int currentCount = 0;
        foreach (var hit in hitColliders)
        {
            // 親や自身にDestructibleがついているか確認（不要なオブジェクトをカウントしないため）
            if (hit.GetComponentInParent<Destructible>() != null)
            {
                currentCount++;
            }
        }
        return currentCount;
    }

    private void SpawnDestructible(float spikeBallScale, float minSpawnRadius, float maxSpawnRadius)
    {
        // ドーナツ状の範囲内にランダムな位置を計算（均等に分布するように平方根を使用）
        float r = Mathf.Sqrt(Random.Range(minSpawnRadius * minSpawnRadius, maxSpawnRadius * maxSpawnRadius));
        float angle = Random.Range(0f, Mathf.PI * 2);
        Vector2 randomCircle = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        GameObject obj = Instantiate(destructiblePrefab, spawnPos, Quaternion.identity);
        Destructible destructible = obj.GetComponent<Destructible>();

        if (destructible != null)
        {
            // 確率の重み（Weight）からどのサイズを使用するか決定
            float totalWeight = smallProfile.weight + mediumProfile.weight + largeProfile.weight;
            float randomWeight = Random.Range(0f, totalWeight);
            
            SpawnProfile selectedProfile;
            if (randomWeight <= smallProfile.weight)
            {
                selectedProfile = smallProfile;
            }
            else if (randomWeight <= smallProfile.weight + mediumProfile.weight)
            {
                selectedProfile = mediumProfile;
            }
            else
            {
                selectedProfile = largeProfile;
            }

            // 選択されたプロファイルの倍率から実際のスケール幅を算出
            float minVal = spikeBallScale * selectedProfile.minScaleMultiplier;
            float maxVal = spikeBallScale * selectedProfile.maxScaleMultiplier;
            float randomScale = Random.Range(minVal, maxVal);

            destructible.Initialize(randomScale);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // インスペクタ（エディタ）で選択された際に、スポーン範囲を可視化します
        float currentRadius = spawnRadiusBase;
        float currentMinRadius = minSpawnRadiusBase;
        if (Application.isPlaying && SpikeBall.Instance != null)
        {
            currentRadius = SpikeBall.Instance.GetScaledValue(spawnRadiusBase);
            currentMinRadius = SpikeBall.Instance.GetScaledValue(minSpawnRadiusBase);
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentMinRadius);
    }
}
