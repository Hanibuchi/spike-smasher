using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class ChainBase : MonoBehaviour
{
    [Tooltip("DragControllerが動かしている対象オブジェクトのTransform")]
    public Transform targetHandle;

    [Tooltip("移動の速度")]
    public float moveSpeed = 10f;

    private Rigidbody rb;

    private struct ChainLinkInfo
    {
        public Transform linkTransform;
        public Vector3 initialScale;
    }
    private List<ChainLinkInfo> chainLinks = new List<ChainLinkInfo>();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InitializeChainLinks();
    }

    private void InitializeChainLinks()
    {
        Rigidbody currentRb = rb;
        
        while (currentRb != null)
        {
            // SpikeBallは自身のスケール管理を持っているので除外して終了
            if (currentRb.GetComponent<SpikeBall>() != null)
            {
                break;
            }

            chainLinks.Add(new ChainLinkInfo 
            { 
                linkTransform = currentRb.transform, 
                initialScale = currentRb.transform.localScale 
            });

            Joint joint = currentRb.GetComponent<Joint>();
            if (joint != null && joint.connectedBody != null)
            {
                // 無限ループ防止
                if (chainLinks.Exists(x => x.linkTransform == joint.connectedBody.transform))
                {
                    break;
                }
                currentRb = joint.connectedBody;
            }
            else
            {
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        // 鎖の各リンクのスケールをSpikeBallに合わせて更新
        if (SpikeBall.Instance != null && chainLinks.Count > 0)
        {
            float sizeLevel = SpikeBall.Instance.GetScaledValue(1f);
            foreach (var link in chainLinks)
            {
                if (link.linkTransform != null)
                {
                    link.linkTransform.localScale = link.initialScale * sizeLevel;
                }
            }
        }

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
