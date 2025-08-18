using UnityEngine;

public class StepResistance : MonoBehaviour
{
    [Header("攀爬设置")]
    public float climbSpeed = 1.5f;               // 攀爬速度
    public float movementDrag = 0.5f;             // 移动阻力

    [Header("调试")]
    public bool showDebugInfo = true;

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
            Transform wheelchairPlayer = collision.transform;
            Rigidbody wheelchairRb = collision.rigidbody;

            // 计算轮椅及其所有子物体的最低Y值（真正的底部）
            float playerBottomY = GetLowestYPosition(wheelchairPlayer);

            // 检查是否在向前移动 且 底部还没达到台阶顶部
            if (wheelchairRb.velocity.z > 0.1f && playerBottomY < stepTopY)
            {
                // 像猴子爬树一样平滑上升
                Vector3 newPos = wheelchairPlayer.position;
                newPos.y += climbSpeed * Time.deltaTime;
                wheelchairPlayer.position = newPos;

                // 添加阻力感
                Vector3 velocity = wheelchairRb.velocity;
                velocity.x *= movementDrag;
                velocity.z *= movementDrag;
                wheelchairRb.velocity = velocity;

                if (showDebugInfo)
                {
                    Debug.Log($"真实底部Y={playerBottomY:F2}, 台阶顶部Y={stepTopY:F2}, 还需爬升={stepTopY - playerBottomY:F2}");
                }
            }
        }
    }

    float GetLowestYPosition(Transform parent)
    {
        float lowestY = parent.position.y;

        // 递归检查所有子物体
        foreach (Transform child in parent)
        {
            float childLowestY = GetLowestYPosition(child);
            if (childLowestY < lowestY)
            {
                lowestY = childLowestY;
            }
        }

        return lowestY;
    }

    void OnDrawGizmosSelected()
    {
        // 显示台阶顶部线
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 stepTop = transform.position;
            stepTop.y = stepTopY;
            Gizmos.DrawLine(stepTop + Vector3.left, stepTop + Vector3.right);
        }
    }
}