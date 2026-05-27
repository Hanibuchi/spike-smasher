using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ScoreBall : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("このボールを取得した際に得られるスコア")]
    public int scoreAmount = 1;
    [Tooltip("スポーンしてから引き寄せられるようになるまでの時間(秒)")]
    public float attractDelay = 1.0f;
    [Tooltip("コレクターに向けて引き寄せられる力")]
    public float attractSpeed = 50f;
    
    private Rigidbody rb;
    private float timer = 0f;
    private bool isAttractable = false;
    private Transform collectorTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 一定時間経過するまで引き寄せられないようにする
        if (!isAttractable)
        {
            timer += Time.deltaTime;
            if (timer >= attractDelay)
            {
                isAttractable = true;
            }
        }
    }

    private void FixedUpdate()
    {
        // ターゲットが設定されていれば引き寄せる
        if (isAttractable && collectorTarget != null)
        {
            Vector3 direction = (collectorTarget.position - transform.position).normalized;
            rb.AddForce(direction * attractSpeed, ForceMode.Acceleration);
        }
    }

    public void SetCollectorTarget(Transform target)
    {
        collectorTarget = target;
    }

    public int GetScore()
    {
        return scoreAmount;
    }
}
