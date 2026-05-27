using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChainBase : MonoBehaviour
{
    [Tooltip("DragControllerが動かしている対象オブジェクトのTransform")]
    public Transform targetHandle;

    [Tooltip("移動の速度")]
    public float moveSpeed = 10f;

    private Rigidbody rb;
    private float constantY;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        constantY = transform.position.y; // 高さを一定にする場合は使用
    }

    private void FixedUpdate()
    {
        if (targetHandle == null) return;

        // ターゲットに向かう方向を計算（高さは無視）
        Vector3 targetPos = targetHandle.position;
        targetPos.y = transform.position.y;
        
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance > 0.01f) // 一定距離近づいたら移動を止めてピクつきを防止
        {
            Vector3 direction = (targetPos - transform.position).normalized;

            // スケールに応じた変動スピードを取得
            float currentSpeed = SpikeBall.Instance != null ? SpikeBall.Instance.GetScaledValue(moveSpeed) : moveSpeed;

            // 一定の速度で移動する量を計算
            Vector3 step = direction * currentSpeed * Time.fixedDeltaTime;

            // ターゲットを通り越してしまう場合は、移動量を調整
            if (step.magnitude > distance)
            {
                step = direction * distance;
            }

            // RigidbodyのMovePositionを使って一定速度で移動
            rb.MovePosition(rb.position + step);
        }
    }
}
