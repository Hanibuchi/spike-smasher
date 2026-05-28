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
    
    [Tooltip("一定範囲内に存在できる生成済みDestructibleオブジェクトの最大数")]
    public int maxDestructiblesInRadius = 40;
    
    [Header("Scale Settings")]
    [Tooltip("SpikeBallの大きさに応じた、生成されるオブジェクトの最小サイズの倍率")]
    public float minScaleMultiplier = 0.5f;
    
    [Tooltip("SpikeBallの大きさに応じた、生成されるオブジェクトの最大サイズの倍率")]
    public float maxScaleMultiplier = 1.2f;

    [Header("Collision Settings")]
    [Tooltip("生成数をカウントするためのレイヤー（Destructibleオブジェクトが持つレイヤーを指定してください）")]
    public LayerMask destructibleLayer;

    private float spawnTimer = 0f;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (destructiblePrefab == null || SpikeBall.Instance == null) return;

        float currentScaleLevel = SpikeBall.Instance.CurrentSizeLevel;
        // SpikeBallのスケールに応じて半径を変更
        float currentSpawnRadius = SpikeBall.Instance.GetScaledValue(spawnRadiusBase);

        // 範囲内のオブジェクトをカウント
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentSpawnRadius, destructibleLayer);
        int currentCount = 0;
        foreach (var hit in hitColliders)
        {
            // 親や自身にDestructibleがついているか確認（不要なオブジェクトをカウントしないため）
            if (hit.GetComponentInParent<Destructible>() != null)
            {
                currentCount++;
            }
        }

        // 最大数に達していない場合のみ生成
        if (currentCount < maxDestructiblesInRadius)
        {
            SpawnDestructible(currentScaleLevel, currentSpawnRadius);
        }
    }

    private void SpawnDestructible(float spikeBallScale, float spawnRadius)
    {
        // ランダムな位置を計算（今回はXZ平面上を想定。必要に応じて球状（insideUnitSphere）に変更してください）
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        GameObject obj = Instantiate(destructiblePrefab, spawnPos, Quaternion.identity);
        Destructible destructible = obj.GetComponent<Destructible>();

        if (destructible != null)
        {
            // SpikeBallのスケールを基準に最小・最大サイズを算出し、その間でランダム化
            float minVal = spikeBallScale * minScaleMultiplier;
            float maxVal = spikeBallScale * maxScaleMultiplier;
            float randomScale = Random.Range(minVal, maxVal);

            destructible.Initialize(randomScale);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // インスペクタ（エディタ）で選択された際に、スポーン範囲を水色のワイヤーフレームで可視化します
        Gizmos.color = Color.cyan;
        
        float currentRadius = spawnRadiusBase;
        if (Application.isPlaying && SpikeBall.Instance != null)
        {
            currentRadius = SpikeBall.Instance.GetScaledValue(spawnRadiusBase);
        }
        
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}
