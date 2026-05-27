using UnityEngine;

public class ScoreBallCollector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("スコアボールを引き寄せ始める半径")]
    public float attractRadius = 3f;
    [Tooltip("スコアボールを取得してスコアを加算する半径")]
    public float collectRadius = 1f;

    private void Update()
    {
        // スコア（ボールのサイズレベル）に応じて半径を拡張
        float currentAttractRadius = SpikeBall.Instance != null ? SpikeBall.Instance.GetScaledValue(attractRadius) : attractRadius;
        float currentCollectRadius = SpikeBall.Instance != null ? SpikeBall.Instance.GetScaledValue(collectRadius) : collectRadius;

        // 周囲にあるScoreBallを検知
        Collider[] colliders = Physics.OverlapSphere(transform.position, currentAttractRadius);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<ScoreBall>(out ScoreBall sb))
            {
                // 引き寄せのターゲットを自身に設定
                sb.SetCollectorTarget(transform);

                // 回収半径内に入ったらスコアを加算してボールを消す
                float distance = Vector3.Distance(transform.position, sb.transform.position);
                if (distance <= currentCollectRadius)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddScore(sb.GetScore());
                    }
                    Destroy(sb.gameObject);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        float currentAttractRadius = attractRadius;
        float currentCollectRadius = collectRadius;
        
        // 実行中の場合は現在のスケールを反映
        if (Application.isPlaying && SpikeBall.Instance != null)
        {
            currentAttractRadius = SpikeBall.Instance.GetScaledValue(attractRadius);
            currentCollectRadius = SpikeBall.Instance.GetScaledValue(collectRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentAttractRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, currentCollectRadius);
    }
}
