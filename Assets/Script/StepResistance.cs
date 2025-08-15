using UnityEngine;

public class StepResistance : MonoBehaviour
{
    [Header("攀爬设置")]
    public float climbSpeed = 3.0f;               // Y增加速度
    public float movementDrag = 0.4f;             // 移动阻力（0.4 = 减慢到40%）

    [Header("调试")]
    public bool showDebugInfo = true;

    private Transform wheelchairPlayer;
    private Rigidbody wheelchairRb;
    private float stepTopY;                       // 台阶顶部Y坐标

    void Start()
    {
        // 计算台阶顶部Y坐标
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            stepTopY = transform.position.y + col.size.y / 2;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.name == "WheelChair_Player")
        {
            wheelchairPlayer = collision.transform;
            wheelchairRb = collision.rigidbody;

            // 检查是否在向前移动
            if (wheelchairRb.velocity.z > 0.1f)
            {
                // 如果轮椅还没到台阶顶部，就抬高Y
                if (wheelchairPlayer.position.y < stepTopY)
                {
                    Vector3 newPos = wheelchairPlayer.position;
                    newPos.y += climbSpeed * Time.deltaTime;
                    wheelchairPlayer.position = newPos;

                    // 添加移动阻力
                    Vector3 velocity = wheelchairRb.velocity;
                    velocity.x *= movementDrag;
                    velocity.z *= movementDrag;
                    wheelchairRb.velocity = velocity;

                    if (showDebugInfo)
                    {
                        Debug.Log($"轮椅攀爬中，当前Y: {wheelchairPlayer.position.y:F2}, 目标Y: {stepTopY:F2}");
                    }
                }
            }
        }
    }
}