using UnityEngine;
using UnityEngine.XR;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WheelchairDualWheelController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider wheelL;
    public WheelCollider wheelR;

    [Header("Wheel Meshes (Optional)")]
    public Transform meshL;
    public Transform meshR;

    [Header("Movement Settings")]
    public float maxMotorTorque;   // 最大马达扭矩
    public float brakeTorque;      // 制动力矩
    public float maxSpeed;           // 最大速度限制

    [Header("Slope Settings")]
    public float slopeSlideForce;    // 斜坡下滑力大小
    public LayerMask slopeLayer;         // 斜坡层

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundRayLength = 1.0f;

    [Header("Input Settings")]
    public XRNode inputNode = XRNode.LeftHand;

    private InputDevice inputDevice;
    private float moveInput;
    private float turnInput;

    private Rigidbody rb;

    // 斜坡检测
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
        inputDevice = InputDevices.GetDeviceAtXRNode(inputNode);
        StartCoroutine(AlignToGround());

        // 提高轮胎摩擦力
        //var sidewaysFriction = wheelL.sidewaysFriction;
        //sidewaysFriction.extremumValue = 1.5f;
        //sidewaysFriction.asymptoteValue = 1.2f;
        //sidewaysFriction.stiffness = 2.5f;

        //var forwardFriction = wheelL.forwardFriction;
        //forwardFriction.extremumValue = 2.0f;
        //forwardFriction.asymptoteValue = 1.5f;
        //forwardFriction.stiffness = 3.0f;

        //wheelL.sidewaysFriction = sidewaysFriction;
        //wheelL.forwardFriction = forwardFriction;
        //wheelR.sidewaysFriction = sidewaysFriction;
        //wheelR.forwardFriction = forwardFriction;
    }

    void Update()
    {
        if (!inputDevice.isValid)
            inputDevice = InputDevices.GetDeviceAtXRNode(inputNode);

        Vector2 axis;
        if (inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis))
        {
            moveInput = axis.y;
            turnInput = axis.x;
        }
        else
        {
            moveInput = 0f;
            turnInput = 0f;
        }

        // 检测斜坡
        //CheckSlope();
    }

    void FixedUpdate()
    {
        ApplyDifferentialDrive();
        UpdateWheelVisual(wheelL, meshL);
        UpdateWheelVisual(wheelR, meshR);
        ClampMaxSpeed();

        // 如果在斜坡上且没有输入，施加下滑力
        //if (isOnSlope && Mathf.Approximately(moveInput, 0f))
        //{
            //ApplySlopeSlideForce();
        //}
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
        // 计算斜坡方向（法线的垂直投影）
        //Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;

        // 施加下滑力
        //rb.AddForce(slopeDirection * slopeSlideForce, ForceMode.Force);
    }

    private void ApplyDifferentialDrive()
    {
        float leftTorque = (moveInput + turnInput) * maxMotorTorque;
        float rightTorque = (moveInput - turnInput) * maxMotorTorque;

        wheelL.motorTorque = leftTorque;
        wheelR.motorTorque = rightTorque;

        bool isIdle = Mathf.Approximately(moveInput, 0f) && Mathf.Approximately(turnInput, 0f);

        // 平地无输入 → 刹车
        // 斜坡无输入 → 不刹车
        float currentBrake = isIdle ? (isOnSlope ? 0f : brakeTorque) : 0f;

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
        yield return null;

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 5f, groundLayer))
        {
            transform.position = hit.point + Vector3.up * 0.1f;
        }

        yield return new WaitForSeconds(0.05f);
        rb.useGravity = true;
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundRayLength, groundLayer);
    }
}
