using UnityEngine;

public class StepResistance : MonoBehaviour
{
    [Header("攀爬设置")]
    public float climbSpeed = 0.8f;               // 攀爬速度
    public float movementDrag = 0.6f;             // 移动阻力
    public float forwardBoost = 3f;               // 攀爬完成后的前进推力
    public float extraClimbHeight = 0.3f;         // 额外攀爬高度，确保后轮也能通过

    [Header("调试")]
    public bool showDebugInfo = true;

    private float stepTopY;                       // 台阶顶部Y坐标
    private bool isClimbingComplete = false;      // 攀爬是否完成

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
            if (wheelchairRb.velocity.z > 0.05f)
            {
                // 检查轮椅底部是否还需要继续攀爬
                float wheelchairBottomY = GetRealBottomY(wheelchairPlayer);
                float targetHeight = stepTopY + extraClimbHeight; // 爬到台阶顶部再加额外高度

                if (wheelchairBottomY < targetHeight)
                {
                    // 还在攀爬阶段
                    isClimbingComplete = false;

                    // Y位置增加
                    Vector3 newPos = wheelchairPlayer.position;
                    newPos.y += climbSpeed * Time.deltaTime;
                    wheelchairPlayer.position = newPos;

                    // 攀爬时保持最小前进速度，避免完全停止
                    Vector3 velocity = wheelchairRb.velocity;
                    velocity.x *= movementDrag;
                    velocity.z = Mathf.Max(velocity.z * movementDrag, 0.3f); // 增加最小前进速度
                    wheelchairRb.velocity = velocity;

                    if (showDebugInfo)
                    {
                        Debug.Log($"攀爬中：底部Y={wheelchairBottomY:F2}, 目标Y={targetHeight:F2}");
                    }
                }
                else if (!isClimbingComplete)
                {
                    // 刚完成攀爬，给一个强力前进推力确保完全通过台阶
                    isClimbingComplete = true;
                    wheelchairRb.AddForce(Vector3.forward * forwardBoost, ForceMode.Impulse);

                    if (showDebugInfo)
                    {
                        Debug.Log("攀爬完成！给予强力推力确保通过台阶");
                    }
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.name == "WheelChair_Player")
        {
            // 轮椅离开台阶，重置状态
            isClimbingComplete = false;

            if (showDebugInfo)
            {
                Debug.Log("轮椅已离开台阶，重置状态");
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