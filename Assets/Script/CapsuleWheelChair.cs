using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleWheelChair : MonoBehaviour
{
    [Header("Wheel Settings")]
    public Transform leftWheel;
    public Transform rightWheel;
    public float wheelRadius = 0.3f;
    public float wheelFriction = 0.8f;

    [Header("Movement Settings")]
    public float maxMotorForce = 2000f;
    public float turnSpeed = 1f; // 提高转向速度以补偿移除扭矩
    public float maxSpeed = 2f;
    public float brakeForce = 1500f;
    public float slopeClimbMultiplier = 2.5f;

    [Header("Ground Detection")]
    public LayerMask groundLayer = -1;
    public float groundCheckDistance = 0.2f;
    public float groundCheckOffset = 0.1f;

    [Header("Capsule Collider")]
    public CapsuleCollider capsuleCollider;

    [Header("Rotation Settings")]
    public float rotationSmoothTime = 5f; // 旋转平滑时间
    public bool lockXRotation = true; // 锁定X轴旋转
    public bool lockZRotation = true; // 锁定Z轴旋转

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;
    private float currentSlopeAngle;
    private float leftWheelRPM;
    private float rightWheelRPM;
    private float leftWheelRotation;
    private float rightWheelRotation;
    private Vector3 lastPosition;
    private float forwardSpeed;
    private float currentRotationVelocity; // 用于平滑旋转
    private Quaternion targetRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 配置刚体
        rb.mass = 150f; // 合适的重量
        rb.drag = 0.1f;
        rb.angularDrag = 5f; // 较高的角阻力防止过度旋转
        rb.useGravity = true;

        // 如果没有指定胶囊碰撞体，获取或创建一个
        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = 0.5f;
                capsuleCollider.height = 1f;
                capsuleCollider.center = Vector3.up * 0.5f;
            }
        }

        // 设置合适的重心位置
        rb.centerOfMass = new Vector3(0, -0.3f, 0);

        lastPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // 获取输入
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // 更新速度计算
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        forwardSpeed = Vector3.Dot(velocity, transform.forward);
        lastPosition = transform.position;

        // 更新轮子视觉旋转
        UpdateWheelRotation();
    }

    void FixedUpdate()
    {
        // 检测地面
        CheckGround();

        // 应用移动力
        ApplyMovement();

        // 应用转向旋转
        ApplyTurnRotation();

        // 应用刹车
        ApplyBrake();

        // 限制速度
        LimitSpeed();

        // 应用斜坡稳定
        ApplySlopeStability();
    }

    void CheckGround()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * groundCheckOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            groundNormal = hit.normal;
            currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            currentSlopeAngle = 0f;
        }
    }

    void ApplyMovement()
    {
        if (!isGrounded) return;

        // 计算前进方向（考虑地面法线）
        Vector3 forwardDir = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

        // 基础驱动力
        float forceMagnitude = moveInput * maxMotorForce;

        // 在斜坡上时，根据坡度调整力
        if (currentSlopeAngle > 5f)
        {
            // 计算斜坡上的重力分量
            float slopeFactor = Mathf.Sin(currentSlopeAngle * Mathf.Deg2Rad);

            // 判断是上坡还是下坡
            float forwardDot = Vector3.Dot(forwardDir, -Physics.gravity.normalized);

            // 如果是上坡，需要额外的力
            if (moveInput > 0.1f && forwardDot > 0.1f)
            {
                float slopeAssist = slopeFactor * maxMotorForce * slopeClimbMultiplier;
                forceMagnitude += slopeAssist;
            }
            // 如果是下坡且没有输入，让重力作用
            else if (Mathf.Abs(moveInput) < 0.1f)
            {
                // 让重力在斜坡方向上起作用
                float gravityForce = slopeFactor * rb.mass * 9.81f;
                Vector3 slopeDirection = Vector3.ProjectOnPlane(-Physics.gravity.normalized, groundNormal).normalized;
                rb.AddForce(slopeDirection * gravityForce * 0.5f, ForceMode.Force);
            }
        }

        // 应用力
        Vector3 force = forwardDir * forceMagnitude;
        rb.AddForce(force);

        // 计算轮子RPM（用于视觉旋转）
        float speed = rb.velocity.magnitude;
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;

        if (Mathf.Abs(turnInput) < 0.1f)
        {
            // 直行时两个轮子转速相同
            leftWheelRPM = (speed / wheelCircumference) * 60f;
            rightWheelRPM = leftWheelRPM;
        }
        else
        {
            // 转向时内外轮转速不同
            float turnRadius = 2f; // 转向半径
            float angularSpeed = turnInput * turnSpeed * 2f;

            // 计算内外轮速度
            float innerWheelSpeed = Mathf.Max(0, speed - angularSpeed * turnRadius * 0.5f);
            float outerWheelSpeed = speed + angularSpeed * turnRadius * 0.5f;

            leftWheelRPM = (turnInput > 0 ? innerWheelSpeed : outerWheelSpeed) / wheelCircumference * 60f;
            rightWheelRPM = (turnInput > 0 ? outerWheelSpeed : innerWheelSpeed) / wheelCircumference * 60f;
        }
    }

    void ApplyTurnRotation()
    {
        if (!isGrounded) return;

        // 计算转向角度变化
        float rotationAmount = turnInput * turnSpeed * 50f * Time.fixedDeltaTime;

        // 调整斜坡上的转向速度
        if (currentSlopeAngle > 10f)
        {
            rotationAmount *= Mathf.Lerp(1f, 0.7f, currentSlopeAngle / 45f);
        }

        // 计算新的旋转角度
        Vector3 currentEulerAngles = transform.eulerAngles;
        currentEulerAngles.y += rotationAmount;

        // 应用平滑旋转
        Quaternion newRotation = Quaternion.Euler(currentEulerAngles);

        // 使用插值实现平滑旋转
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, newRotation, rotationSmoothTime * 10f * Time.fixedDeltaTime));
    }

    void ApplyBrake()
    {
        if (!isGrounded) return;

        bool isIdle = Mathf.Abs(moveInput) < 0.1f && Mathf.Abs(turnInput) < 0.1f;

        if (isIdle)
        {
            // 在斜坡上时，根据坡度调整刹车力
            if (currentSlopeAngle > 5f)
            {
                // 计算防止滑下所需的最小刹车力
                float slopeBrake = Mathf.Lerp(100f, brakeForce * 0.3f, currentSlopeAngle / 30f);

                // 在斜坡上应用反向力防止滑动
                Vector3 slopeDirection = Vector3.ProjectOnPlane(-Physics.gravity.normalized, groundNormal).normalized;
                float slopeForceMagnitude = Mathf.Sin(currentSlopeAngle * Mathf.Deg2Rad) * rb.mass * 9.81f;

                // 只有当重力的斜坡分量大于摩擦力时才滑下
                float frictionForce = wheelFriction * rb.mass * 9.81f * Mathf.Cos(currentSlopeAngle * Mathf.Deg2Rad);

                if (slopeForceMagnitude > frictionForce)
                {
                    // 允许轻微下滑
                    float slideForce = (slopeForceMagnitude - frictionForce) * 0.3f;
                    rb.AddForce(slopeDirection * slideForce, ForceMode.Force);
                }
                else
                {
                    // 施加刹车力防止滑动
                    rb.AddForce(-rb.velocity * slopeBrake * Time.fixedDeltaTime, ForceMode.Impulse);
                }
            }
            else
            {
                // 平地刹车
                rb.AddForce(-rb.velocity * brakeForce * 0.1f * Time.fixedDeltaTime, ForceMode.Impulse);
            }
        }
    }

    void LimitSpeed()
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    void ApplySlopeStability()
    {
        if (!isGrounded) return;

        // 在斜坡上时增加抓地力
        if (currentSlopeAngle > 5f)
        {
            // 将部分速度投影到地面法线方向，增加抓地力
            Vector3 groundVelocity = Vector3.ProjectOnPlane(rb.velocity, groundNormal);
            Vector3 groundForce = (groundVelocity - rb.velocity) * rb.mass * 0.5f;
            rb.AddForce(groundForce);
        }

        // 如果锁定X轴旋转，确保X旋转保持为0
        if (lockXRotation)
        {
            Vector3 currentEuler = transform.eulerAngles;
            if (Mathf.Abs(currentEuler.x) > 1f)
            {
                currentEuler.x = 0;
                rb.MoveRotation(Quaternion.Euler(currentEuler));
            }
        }

        // 如果锁定Z轴旋转，确保Z旋转保持为0
        if (lockZRotation)
        {
            Vector3 currentEuler = transform.eulerAngles;
            if (Mathf.Abs(currentEuler.z) > 1f)
            {
                currentEuler.z = 0;
                rb.MoveRotation(Quaternion.Euler(currentEuler));
            }
        }
    }

    void UpdateWheelRotation()
    {
        if (!isGrounded) return;

        // 更新轮子旋转角度
        float rotationSpeed = 360f / 60f; // 每RPM转动的角度

        leftWheelRotation += leftWheelRPM * rotationSpeed * Time.deltaTime;
        rightWheelRotation += rightWheelRPM * rotationSpeed * Time.deltaTime;

        // 应用旋转到轮子视觉对象
        if (leftWheel != null)
        {
            leftWheel.localRotation = Quaternion.Euler(leftWheelRotation, 0, 0);
        }

        if (rightWheel != null)
        {
            rightWheel.localRotation = Quaternion.Euler(rightWheelRotation, 0, 0);
        }
    }

    // 用于调试的Gizmos
    void OnDrawGizmosSelected()
    {
        // 绘制地面检测射线
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.DrawRay(rayOrigin, Vector3.down * groundCheckDistance);

        // 绘制地面法线
        if (isGrounded)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, groundNormal);

            // 绘制斜坡角度指示
            Gizmos.color = Color.yellow;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
                $"Slope: {currentSlopeAngle:F1}°\nSpeed: {forwardSpeed:F2}");
#endif
        }
    }
}