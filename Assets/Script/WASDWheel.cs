using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WASDWheel : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider wheelL;
    public WheelCollider wheelR;

    [Header("Wheel Meshes (Optional)")]
    public Transform meshL;
    public Transform meshR;

    [Header("Movement Settings")]
    public float maxMotorTorque = 150f;   // 最大马达扭矩
    public float brakeTorque = 500f;      // 制动力矩
    public float maxSpeed = 5f;           // 最大速度限制

    [Header("Slope Settings")]
    public float slopeSlideForce = 5f;    // 斜坡下滑力大小
    public LayerMask slopeLayer;         // 斜坡层

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundRayLength = 1.0f;

    private float moveInput;
    private float turnInput;
    private Rigidbody rb;
    private bool isOnSlope = false;
    private Vector3 slopeNormal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false; // 初始禁用重力
    }

    void Start()
    {
        StartCoroutine(AlignToGround());

        // 可选：提高轮子摩擦力（如果需要爬坡时加上）
        var sidewaysFriction = wheelL.sidewaysFriction;
        sidewaysFriction.extremumValue = 1.5f;
        sidewaysFriction.asymptoteValue = 1.2f;
        sidewaysFriction.stiffness = 2.5f;

        var forwardFriction = wheelL.forwardFriction;
        forwardFriction.extremumValue = 2.0f;
        forwardFriction.asymptoteValue = 1.5f;
        forwardFriction.stiffness = 3.0f;

        wheelL.sidewaysFriction = sidewaysFriction;
        wheelL.forwardFriction = forwardFriction;
        wheelR.sidewaysFriction = sidewaysFriction;
        wheelR.forwardFriction = forwardFriction;
    }

    void Update()
    {
        // WASD控制
        moveInput = Input.GetAxis("Vertical");    // W/S for forward/backward
        turnInput = Input.GetAxis("Horizontal");  // A/D for left/right

        // 检测是否在斜坡上
        CheckSlope();
    }

    void FixedUpdate()
    {
        ApplyDifferentialDrive();
        UpdateWheelVisual(wheelL, meshL);
        UpdateWheelVisual(wheelR, meshR);
        ClampMaxSpeed();

        // 如果在斜坡上且没有输入，施加下滑力
        if (isOnSlope && Mathf.Approximately(moveInput, 0f))
        {
            ApplySlopeSlideForce();
        }
    }

    private void CheckSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, groundRayLength, slopeLayer))
        {
            isOnSlope = true;
            slopeNormal = hit.normal;
        }
        else
        {
            isOnSlope = false;
        }
    }

    private void ApplySlopeSlideForce()
    {
        // 计算斜坡方向（斜坡法线的垂直方向）
        Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;

        // 施加下滑力
        rb.AddForce(slopeDirection * slopeSlideForce, ForceMode.Force);
    }

    private void ApplyDifferentialDrive()
    {
        // ✅ 修复转向方向：向左推动手柄会左转
        float leftTorque = (moveInput + turnInput) * maxMotorTorque;
        float rightTorque = (moveInput - turnInput) * maxMotorTorque;

        wheelL.motorTorque = leftTorque;
        wheelR.motorTorque = rightTorque;

        bool isIdle = Mathf.Approximately(moveInput, 0f) && Mathf.Approximately(turnInput, 0f);
        float currentBrake = isIdle ? (isOnSlope ? 0f : brakeTorque) : 0f; // 在斜坡上时不刹车

        wheelL.brakeTorque = currentBrake;
        wheelR.brakeTorque = currentBrake;
    }

    private void UpdateWheelVisual(WheelCollider col, Transform mesh)
    {
        if (mesh == null)
            return;

        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }

    private void ClampMaxSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limited = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
    }

    private IEnumerator AlignToGround()
    {
        yield return null; // 等一帧让场景初始化

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 5f, groundLayer))
        {
            transform.position = hit.point + Vector3.up * 0.1f;
        }

        yield return new WaitForSeconds(0.05f); // 稍微等几帧确保碰撞稳定
        rb.useGravity = true; // 启用重力
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundRayLength, groundLayer);
    }
}