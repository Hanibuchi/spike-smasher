using System.Collections.Generic;
using UnityEngine;

public class InfiniteGroundSpawner : MonoBehaviour
{
    [Header("Ground Settings")]
    [Tooltip("生成する地面のプレハブ")]
    public GameObject groundPrefab;
    
    [Tooltip("地面プレハブ1つあたりのサイズ（縦・横が同じスケール前提）")]
    public float tileSize = 10f;
    
    [Tooltip("プレイヤーを中心に、周囲何マス分まで地面を生成するか（基本値）。SpikeBallのスケールに応じて広がります")]
    public int baseViewDistanceInTiles = 3;

    [Tooltip("生成される地面の高さ（Y座標）")]
    public float spawnHeight = 0f;

    [Header("Target")]
    [Tooltip("追従する対象（空欄の場合はこのコンポーネントがアタッチされているオブジェクトが対象になります）")]
    public Transform target;

    // 現在生成されている地面タイルを管理する辞書（キー：グリッド座標、値：生成されたオブジェクト）
    private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();
    
    // 前回のターゲットのグリッド座標
    private Vector2Int currentGridPos;
    // 前回の描画距離
    private int currentViewDistance;

    private void Start()
    {
        if (target == null)
        {
            target = transform;
        }

        currentViewDistance = baseViewDistanceInTiles;
        if (SpikeBall.Instance != null)
        {
            currentViewDistance = Mathf.CeilToInt(baseViewDistanceInTiles * SpikeBall.Instance.CurrentSizeLevel);
        }

        // 初期スポーン
        currentGridPos = GetGridPosition(target.position);
        UpdateTerrain(currentViewDistance);
    }

    private void Update()
    {
        if (target == null) return;

        // ターゲットの現在のグリッド座標を計算
        Vector2Int newGridPos = GetGridPosition(target.position);

        // SpikeBallのスケールを考慮した現在の生成範囲を計算
        int newViewDistance = baseViewDistanceInTiles;
        if (SpikeBall.Instance != null)
        {
            newViewDistance = Mathf.CeilToInt(baseViewDistanceInTiles * SpikeBall.Instance.CurrentSizeLevel);
        }

        // グリッド座標をまたいだ（別のタイルに移動した）、または生成範囲が変化した場合に更新を行う
        if (newGridPos != currentGridPos || newViewDistance != currentViewDistance)
        {
            currentGridPos = newGridPos;
            currentViewDistance = newViewDistance;
            UpdateTerrain(currentViewDistance);
        }
    }

    /// <summary>
    /// ワールド座標からグリッド座標に変換する
    /// </summary>
    private Vector2Int GetGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int z = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 地面の生成と削除を行う
    /// </summary>
    private void UpdateTerrain(int viewDistance)
    {
        // 今回の更新で存在すべきタイルの座標のリスト
        List<Vector2Int> tilesToKeep = new List<Vector2Int>();

        // プレイヤーの周囲に必要なタイルを生成する
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int gridPos = new Vector2Int(currentGridPos.x + x, currentGridPos.y + z);
                tilesToKeep.Add(gridPos);

                // まだその座標に地面が生成されていなければ生成
                if (!activeTiles.ContainsKey(gridPos))
                {
                    Vector3 worldPos = new Vector3(gridPos.x * tileSize, spawnHeight, gridPos.y * tileSize);
                    GameObject newTile = Instantiate(groundPrefab, worldPos, Quaternion.identity);
                    newTile.transform.SetParent(transform); // 整理のために子オブジェクトにする
                    activeTiles.Add(gridPos, newTile);
                }
            }
        }

        // 遠くなって不要になったタイルを削除する
        List<Vector2Int> tilesToRemove = new List<Vector2Int>();
        foreach (var kvp in activeTiles)
        {
            if (!tilesToKeep.Contains(kvp.Key))
            {
                Destroy(kvp.Value);
                tilesToRemove.Add(kvp.Key);
            }
        }

        // 辞書からも削除
        foreach (var key in tilesToRemove)
        {
            activeTiles.Remove(key);
        }
    }
}
