using UnityEngine;

/// <summary>
/// 虚拟坐垫：控制坐姿高度、头部抖动与上坡倾斜效果。
/// 将 XR 相机层级放在此对象下方，使所有效果影响整体玩家视角。
/// </summary>
public class WheelchairSeat : MonoBehaviour
{
    [Header("Seat Settings")]
    [Tooltip("虚拟坐垫高度（相对于轮椅根对象的局部 Y 偏移）。")]
    public float seatHeight = 0.6f;

    [Header("Head Bob Settings")]
    public float headBobAmount = 0.05f;
    public float headBobSpeed = 2f;

    [Header("Slope Simulation")]
    [Tooltip("当前坡度角度，单位为度，正值为上坡（向后倾斜）。")]
    public float slopeAngle = 0f; // -10 ~ +10 degrees typical range
    public float maxTiltAngle = 10f; // 最大倾斜角
    public float tiltSmoothing = 2f;
    public float slopeOffsetZ = 0.1f; // 上坡时座椅向后滑的距离

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float headBobTimer = 0f;

    void Awake()
    {
        baseLocalPosition = transform.localPosition;
        baseLocalPosition.y = seatHeight;
        baseLocalRotation = transform.localRotation;

        transform.localPosition = baseLocalPosition;
    }

    void Update()
    {
        ApplySlopeTilt();
    }

    /// <summary>
    /// 应用头部抖动（行进时调用）
    /// </summary>
    public void ApplyHeadBob(float inputMagnitude)
    {
        if (inputMagnitude > 0.1f)
        {
            headBobTimer += Time.deltaTime * headBobSpeed;
            float bobY = Mathf.Sin(headBobTimer) * headBobAmount * inputMagnitude;

            transform.localPosition = new Vector3(
                baseLocalPosition.x,
                seatHeight + bobY,
                baseLocalPosition.z
            );
        }
        else
        {
            ResetSeatPosition();
        }
    }

    /// <summary>
    /// 重置抖动状态
    /// </summary>
    public void ResetSeatPosition()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            new Vector3(baseLocalPosition.x, seatHeight, baseLocalPosition.z),
            Time.deltaTime * headBobSpeed
        );
        headBobTimer = 0f;
    }

    /// <summary>
    /// 更新座椅高度（外部可调用）
    /// </summary>
    public void SetSeatHeight(float newHeight)
    {
        seatHeight = newHeight;
        baseLocalPosition.y = seatHeight;
    }

    /// <summary>
    /// 应用上坡倾斜模拟
    /// </summary>
    private void ApplySlopeTilt()
    {
        // Clamp 坡度
        float clampedSlope = Mathf.Clamp(slopeAngle, -maxTiltAngle, maxTiltAngle);

        // 计算目标旋转（绕 X 轴仰角）
        Quaternion targetRotation = Quaternion.Euler(clampedSlope, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * tiltSmoothing);

        // 上坡时略微向后偏移（模仿重力）
        float zOffset = Mathf.Sin(clampedSlope * Mathf.Deg2Rad) * slopeOffsetZ;
        Vector3 offsetPosition = baseLocalPosition + new Vector3(0f, 0f, -zOffset);
        transform.localPosition = Vector3.Lerp(transform.localPosition, offsetPosition, Time.deltaTime * tiltSmoothing);
    }
}