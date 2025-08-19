using UnityEngine;

public class StepResistance : MonoBehaviour
{
    [Header("攀爬设置")]
    public float climbSpeed = 0.8f;               // 攀爬速度
    public float movementDrag = 0.6f;             // 移动阻力

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

            // 检查是否在向前移动
            if (wheelchairRb.velocity.z > 0.1f)
            {
                // 检查轮椅底部是否还没到台阶顶部
                float wheelchairBottomY = GetRealBottomY(wheelchairPlayer);

                if (wheelchairBottomY < stepTopY)
                {
                    // 简单的Y位置增加
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
                        Debug.Log($"攀爬：底部Y={wheelchairBottomY:F2}, 目标Y={stepTopY:F2}");
                    }
                }
            }
        }
    }

    float GetRealBottomY(Transform player)
    {
        // 获取轮椅及其子物体的最低Y点
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        float lowestY = player.position.y;

        foreach (Renderer renderer in renderers)
        {
            if (renderer.bounds.min.y < lowestY)
            {
                lowestY = renderer.bounds.min.y;
            }
        }

        return lowestY;
    }
}