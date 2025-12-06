using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRCapsuleWheelchairController : MonoBehaviour
{
    [Header("Wheel Settings")]
    public Transform leftWheel;
    public Transform rightWheel;
    public float wheelRadius = 0.3f;
    public float wheelFriction = 0.8f;

    [Header("Movement Settings")]
    public float maxMotorForce = 2000f;
    public float turnSpeed = 1.5f;
    public float maxSpeed = 2f;
    public float brakeForce = 1500f;
    public float slopeClimbMultiplier = 2.5f;

    [Header("Ground Detection")]
    public LayerMask groundLayer = -1;
    public float groundCheckDistance = 0.2f;
    public float groundCheckOffset = 0.1f;

    [Header("VR Controller Settings")]
    public XRNode leftHandNode = XRNode.LeftHand;    // 左手手柄
    public XRNode rightHandNode = XRNode.RightHand;  // 右手手柄
    public bool useVRControllers = true;             // 是否使用VR控制器
    public float inputDeadzone = 0.1f;               // 输入死区

    [Header("Capsule Collider")]
    public CapsuleCollider capsuleCollider;

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

    // VR控制器相关
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    private bool vrControllersInitialized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 配置刚体
        rb.mass = 150f;
        rb.drag = 0.1f;
        rb.angularDrag = 5f;
        rb.useGravity = true;

        // 锁定X和Z轴旋转，只允许Y轴旋转
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 初始化VR控制器
        if (useVRControllers)
        {
            InitializeVRControllers();
        }

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
    }

    void Update()
    {
        // 获取输入
        if (useVRControllers)
        {
            GetVRInput();
        }
        else
        {
            // 备用键盘输入（用于测试）
            GetKeyboardInput();
        }

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

        // 应用转向
        ApplyTurn();

        // 应用刹车
        ApplyBrake();

        // 限制速度
        LimitSpeed();

        // 在斜坡上施加额外稳定性
        ApplySlopeStability();
    }

    void InitializeVRControllers()
    {
        // 获取左右手柄设备
        leftHandDevice = InputDevices.GetDeviceAtXRNode(leftHandNode);
        rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandNode);

        vrControllersInitialized = leftHandDevice.isValid && rightHandDevice.isValid;

        if (!vrControllersInitialized)
        {
            Debug.LogWarning("VR controllers not fully initialized. Will retry in Update.");
        }
        else
        {
            Debug.Log("VR controllers initialized successfully.");
        }
    }

    void GetVRInput()
    {
        // 检查并重新获取设备（如果必要）
        if (!leftHandDevice.isValid || !rightHandDevice.isValid)
        {
            leftHandDevice = InputDevices.GetDeviceAtXRNode(leftHandNode);
            rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandNode);
            vrControllersInitialized = leftHandDevice.isValid && rightHandDevice.isValid;
        }

        if (!vrControllersInitialized) return;

        // 方法1: 使用摇杆控制 - 尝试多种输入方式
        Vector2 leftAxis = Vector2.zero;
        Vector2 rightAxis = Vector2.zero;

        bool leftInputFound = false;
        bool rightInputFound = false;

        // 尝试多种输入方式获取左手前进输入
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftAxis))
        {
            leftInputFound = true;
        }
        else if (leftHandDevice.TryGetFeatureValue(CommonUsages.secondary2DAxis, out leftAxis))
        {
            leftInputFound = true;
        }

        // 尝试多种输入方式获取右手转向输入
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightAxis))
        {
            rightInputFound = true;
        }
        else if (rightHandDevice.TryGetFeatureValue(CommonUsages.secondary2DAxis, out rightAxis))
        {
            rightInputFound = true;
        }

        // 如果找不到摇杆输入，尝试使用扳机键
        if (!leftInputFound)
        {
            float leftTrigger;
            if (leftHandDevice.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger))
            {
                leftAxis.y = leftTrigger;
                leftInputFound = true;
            }
        }

        if (!rightInputFound)
        {
            float rightTrigger;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger))
            {
                // 使用扳机键控制转向：右扳机=右转，左扳机=左转
                rightAxis.x = rightTrigger;
                rightInputFound = true;
            }
        }

        if (leftInputFound)
        {
            // 应用死区处理
            if (Mathf.Abs(leftAxis.y) > inputDeadzone)
            {
                moveInput = leftAxis.y;
            }
            else
            {
                moveInput = 0f;
            }
        }
        else
        {
            moveInput = 0f;
        }

        if (rightInputFound)
        {
            // 应用死区处理
            if (Mathf.Abs(rightAxis.x) > inputDeadzone)
            {
                turnInput = rightAxis.x;
            }
            else
            {
                turnInput = 0f;
            }
        }
        else
        {
            turnInput = 0f;
        }

        // 调试输出当前输入值
        Debug.Log($"Move Input (Left): {moveInput:F2}, Turn Input (Right): {turnInput:F2}");
    }

    void GetKeyboardInput()
    {
        // 备用键盘输入（用于测试或调试）
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        Debug.Log($"Keyboard - Move: {moveInput:F2}, Turn: {turnInput:F2}");
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

        // 计算轮子RPM（用于视觉旋转）- 修复版本
        // 速度在前进方向上的分量
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
        float forwardSpeed = forwardVelocity.magnitude;

        // 确定速度方向
        float speedDirection = Mathf.Sign(Vector3.Dot(rb.velocity, transform.forward));

        float wheelCircumference = 2 * Mathf.PI * wheelRadius;

        if (Mathf.Abs(turnInput) < 0.1f)
        {
            // 直行时两个轮子转速相同
            leftWheelRPM = (forwardSpeed / wheelCircumference) * 60f * speedDirection;
            rightWheelRPM = leftWheelRPM;
        }
        else
        {
            // 转向时内外轮转速不同 - 修复差速计算
            float baseSpeed = forwardSpeed * speedDirection;
            float turnRadius = 1.0f; // 转向半径
            float angularSpeed = turnInput * turnSpeed * 2f;

            // 差速计算：内轮减速，外轮加速
            // 当turnInput > 0时，右转，左轮是外轮，右轮是内轮
            // 当turnInput < 0时，左转，右轮是外轮，左轮是内轮

            if (Mathf.Abs(turnInput) > 0.1f)
            {
                // 计算差速因子
                float differentialFactor = 0.5f; // 差速程度

                if (turnInput > 0) // 右转
                {
                    // 左轮（外轮）加速，右轮（内轮）减速
                    leftWheelRPM = ((baseSpeed * (1 + differentialFactor)) / wheelCircumference) * 60f;
                    rightWheelRPM = ((baseSpeed * (1 - differentialFactor)) / wheelCircumference) * 60f;
                }
                else // 左转
                {
                    // 右轮（外轮）加速，左轮（内轮）减速
                    leftWheelRPM = ((baseSpeed * (1 - differentialFactor)) / wheelCircumference) * 60f;
                    rightWheelRPM = ((baseSpeed * (1 + differentialFactor)) / wheelCircumference) * 60f;
                }
            }
        }
    }

    void ApplyTurn()
    {
        if (!isGrounded) return;

        // 使用直接旋转而不是扭矩
        float rotationAmount = turnInput * turnSpeed * 50f * Time.fixedDeltaTime;

        // 调整斜坡上的转向速度
        if (currentSlopeAngle > 10f)
        {
            rotationAmount *= Mathf.Lerp(1f, 0.7f, currentSlopeAngle / 45f);
        }

        // 计算新的旋转角度
        Vector3 currentEulerAngles = transform.eulerAngles;
        currentEulerAngles.y += rotationAmount;

        // 应用旋转
        rb.MoveRotation(Quaternion.Euler(currentEulerAngles));
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
    }

    void UpdateWheelRotation()
    {
        if (!isGrounded) return;

        // 更新轮子旋转角度
        float rotationSpeed = 360f / 60f;

        // 确保轮子旋转方向正确
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
                $"Slope: {currentSlopeAngle:F1}°\nSpeed: {forwardSpeed:F2}\nMove: {moveInput:F2}\nTurn: {turnInput:F2}\n" +
                $"Left RPM: {leftWheelRPM:F1}\nRight RPM: {rightWheelRPM:F1}");
#endif
        }
    }
}
