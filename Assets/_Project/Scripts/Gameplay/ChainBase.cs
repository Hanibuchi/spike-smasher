using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChainBase : MonoBehaviour
{
    [Tooltip("DragControllerが動かしている対象オブジェクトのTransform")]
    public Transform targetHandle;

    [Tooltip("移動の速度")]
    public float moveSpeed = 10f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (targetHandle == null) return;

        // Y座標をtargetHandleと同じ高さに固定
        Vector3 pos = transform.position;
        pos.y = targetHandle.position.y;
        transform.position = pos;

        // ターゲットに向かう方向を計算（高さは無視）
        Vector3 targetPos = targetHandle.position;
        
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance > 0.01f) // 一定距離近づいたら移動を止めてピクつきを防止
        {
            Vector3 direction = (targetPos - transform.position).normalized;

            // スケールに応じた変動スピードを取得
            float currentSpeed = SpikeBall.Instance != null ? SpikeBall.Instance.GetScaledValue(moveSpeed) : moveSpeed;

            // 1フレームの移動距離で通り過ぎてしまう場合は速度を調整
            if (currentSpeed * Time.fixedDeltaTime > distance)
            {
                currentSpeed = distance / Time.fixedDeltaTime;
            }

            // RigidbodyのlinearVelocityを使って移動
            rb.linearVelocity = direction * currentSpeed;
        }
        else
        {
            // 目標位置に到達したら停止
            rb.linearVelocity = Vector3.zero;
        }
    }
}
