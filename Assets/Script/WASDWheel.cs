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
    public float maxMotorTorque = 1500f;   // 增加默认值
    public float brakeTorque = 1000f;      // 制动力矩
    public float maxSpeed = 10f;           // 最大速度限制
    public float turningMotorTorque = 800f;
    
    [Header("Slope Handling")]
    public float slopeTorqueMultiplier = 2.5f;  // 坡道扭矩倍增
    public float minSlopeAngle = 5f;            // 最小斜坡角度
    public float maxSlopeAngle = 45f;           // 最大可攀爬角度
    public float slopeDetectionDistance = 1.0f; // 斜坡检测距离

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundRayLength = 1.0f;

    private float moveInput;
    private float turnInput;
    private Rigidbody rb;
    [SerializeField]
    private float currentSlopeAngle = 0f;
    private Vector3 slopeNormal = Vector3.up;
    private bool isGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 保留Y轴旋转，冻结X和Z旋转防止翻车
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true; // 保持重力启用
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        
        // 更新接地状态
        isGrounded = IsGrounded();
        
        // 检测斜坡角度
        if (isGrounded)
        {
            DetectSlope();
        }
    }

    void FixedUpdate()
    {
        ApplyDifferentialDrive();
        UpdateWheelVisual(wheelL, meshL);
        UpdateWheelVisual(wheelR, meshR);
        ClampMaxSpeed();
        ApplySlopeGravity(); // 应用斜坡重力补偿
    }

    private void DetectSlope()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        
        // 向前方检测斜坡
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, slopeDetectionDistance, groundLayer))
        {
            slopeNormal = hit.normal;
            currentSlopeAngle = Vector3.Angle(Vector3.up, slopeNormal);
        }
        // 向下方检测当前地面
        else if (Physics.Raycast(rayOrigin + Vector3.up * 0.5f, Vector3.down, out hit, 1.0f, groundLayer))
        {
            slopeNormal = hit.normal;
            currentSlopeAngle = Vector3.Angle(Vector3.up, slopeNormal);
        }
        else
        {
            currentSlopeAngle = 0f;
            slopeNormal = Vector3.up;
        }
    }

    private void ApplyDifferentialDrive()
    {
        float slopeMultiplier = 1f;
        
        // 在斜坡上时增加扭矩
        if (currentSlopeAngle > minSlopeAngle && currentSlopeAngle <= maxSlopeAngle)
        {
            // 计算前进方向与斜坡法线的夹角，判断是上坡还是下坡
            float forwardDot = Vector3.Dot(transform.forward, slopeNormal);
            if (forwardDot < -0.1f && moveInput > 0.1f) // 上坡且向前行驶
            {
                slopeMultiplier = slopeTorqueMultiplier * (currentSlopeAngle / maxSlopeAngle);
            }
        }

        float leftTorque = (moveInput + turnInput) * turningMotorTorque * slopeMultiplier;
        float rightTorque = (moveInput - turnInput) * turningMotorTorque * slopeMultiplier;

        // 限制扭矩在合理范围内
        leftTorque = Mathf.Clamp(leftTorque, -maxMotorTorque, maxMotorTorque);
        rightTorque = Mathf.Clamp(rightTorque, -maxMotorTorque, maxMotorTorque);

        wheelL.motorTorque = leftTorque;
        wheelR.motorTorque = rightTorque;

        // 优化刹车逻辑
        bool isIdle = Mathf.Abs(moveInput) < 0.1f && Mathf.Abs(turnInput) < 0.1f;
        
        if (isIdle)
        {
            // 在斜坡上时，根据坡度调整刹车
            if (currentSlopeAngle > minSlopeAngle)
            {
                // 计算斜坡上的重力分量
                float gravityForce = Mathf.Sin(currentSlopeAngle * Mathf.Deg2Rad) * rb.mass * Physics.gravity.magnitude;
                float requiredBrake = gravityForce * 0.5f; // 使用部分刹车力保持稳定
                
                wheelL.brakeTorque = Mathf.Min(requiredBrake, brakeTorque);
                wheelR.brakeTorque = Mathf.Min(requiredBrake, brakeTorque);
            }
            else
            {
                wheelL.brakeTorque = brakeTorque;
                wheelR.brakeTorque = brakeTorque;
            }
        }
        else
        {
            // 行驶时释放刹车
            wheelL.brakeTorque = 0f;
            wheelR.brakeTorque = 0f;
        }
    }

    private void ApplySlopeGravity()
    {
        if (!isGrounded || currentSlopeAngle < minSlopeAngle) return;
        
        // 计算斜坡方向的重力分量
        Vector3 gravityForce = Physics.gravity * rb.mass;
        Vector3 slopeDirection = Vector3.ProjectOnPlane(gravityForce, slopeNormal).normalized;
        
        // 在斜坡上施加额外的力，帮助滑下或稳定
        float slopeFactor = Mathf.Sin(currentSlopeAngle * Mathf.Deg2Rad);
        Vector3 slopeForce = slopeDirection * slopeFactor * rb.mass * 2f;
        
        rb.AddForce(slopeForce, ForceMode.Force);
    }

    private void UpdateWheelVisual(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;

        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
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

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, 
                              Vector3.down, groundRayLength, groundLayer);
    }

    // 调试显示
    void OnDrawGizmosSelected()
    {
        // 绘制斜坡检测线
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * slopeDetectionDistance);
        
        // 绘制地面检测线
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * groundRayLength);
    }
}