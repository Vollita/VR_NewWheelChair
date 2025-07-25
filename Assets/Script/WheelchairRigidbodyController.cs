using UnityEngine;
using UnityEngine.XR;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class WheelchairRigidbodyController : MonoBehaviour
{
    [Header("Movement")]
    public float maxMoveSpeed = 3f;
    public float moveForce = 100f;
    public float turnTorque = 30f;
    public float lowSpeedTurnAssist = 0.5f;

    [Header("Turning Settings")]
    public float turnDeadZone = 0.2f;
    public float turnExponent = 1.5f;
    public float maxAngularSpeed = 1.5f;

    [Header("Rolling / Braking")]
    public float rollingResistance = 3f;
    public float brakeMultiplier = 2f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = ~0;
    public float groundStickForce = 10f;
    public float normalSmoothTime = 0.2f;

    [Header("Seat (Optional)")]
    public WheelchairSeat seat;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;
    private Vector3 smoothedNormal = Vector3.up;
    private InputDevice leftHand;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // 🚩 更稳定的物理属性
        rb.mass = 20f;
        rb.drag = 1f;
        rb.angularDrag = 5f;

        if (!seat)
            seat = GetComponentInChildren<WheelchairSeat>();
    }

    IEnumerator Start()
    {
        rb.useGravity = false;
        yield return null;

        // 吸附到地面（避免初始悬空）
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 5f, groundLayer))
        {
            transform.position = hit.point;
        }

        rb.useGravity = true;
    }

    void OnEnable()
    {
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        if (!leftHand.isValid)
            leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        Vector2 axis;
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis))
        {
            float deadZoneY = 0.1f;
            moveInput = Mathf.Abs(axis.y) > deadZoneY ? axis.y : 0f;

            if (Mathf.Abs(axis.x) > turnDeadZone)
            {
                float normalized = (Mathf.Abs(axis.x) - turnDeadZone) / (1f - turnDeadZone);
                turnInput = Mathf.Sign(axis.x) * Mathf.Pow(normalized, turnExponent);
            }
            else
            {
                turnInput = 0f;
            }

            seat?.ApplyHeadBob(axis.magnitude);
        }
        else
        {
            moveInput = 0f;
            turnInput = 0f;
            seat?.ResetSeatPosition();
        }
    }

    void FixedUpdate()
    {
        UpdateGroundInfo();
        ApplyMovementForces();
        ApplyRollingResistance();
        StickToGround();
        ClampVelocity(); // 🚩 限制水平和垂直速度
    }

    private void ApplyMovementForces()
    {
        if (Mathf.Approximately(moveInput, 0f) && Mathf.Approximately(turnInput, 0f))
        {
            // 松手立即停止旋转
            Vector3 angular = rb.angularVelocity;
            Vector3 projected = Vector3.Project(angular, smoothedNormal);
            rb.angularVelocity -= projected;
            return;
        }

        Vector3 forwardOnSlope = Vector3.ProjectOnPlane(transform.forward, smoothedNormal).normalized;

        if (!Mathf.Approximately(moveInput, 0f))
        {
            float targetSpeed = moveInput * maxMoveSpeed;
            Vector3 planarVel = GetPlanarVelocityOnGround();
            float currentSpeed = Vector3.Dot(planarVel, forwardOnSlope);

            float desiredAccel = (targetSpeed - currentSpeed) * moveForce;
            desiredAccel += Vector3.Dot(-Physics.gravity, forwardOnSlope);

            rb.AddForce(forwardOnSlope * desiredAccel, ForceMode.Acceleration);
        }

        if (!Mathf.Approximately(turnInput, 0f))
        {
            Vector3 torqueAxis = smoothedNormal;
            float torque = turnInput * turnTorque;

            float currentAngularSpeed = Vector3.Project(rb.angularVelocity, torqueAxis).magnitude;
            if (currentAngularSpeed < maxAngularSpeed)
                rb.AddTorque(torqueAxis * torque, ForceMode.Acceleration);

            if (rb.velocity.magnitude < 0.2f && Mathf.Abs(moveInput) < 0.1f)
            {
                rb.angularVelocity = torqueAxis * (turnInput * lowSpeedTurnAssist);
            }
        }
    }

    private void ApplyRollingResistance()
    {
        Vector3 planarVel = GetPlanarVelocityOnGround();
        if (planarVel.sqrMagnitude < 0.0001f)
            return;

        Vector3 slopeForce = Vector3.ProjectOnPlane(Physics.gravity, smoothedNormal);
        rb.AddForce(-slopeForce * rb.mass, ForceMode.Force);

        float resistance = rollingResistance;
        if (!Mathf.Approximately(moveInput, 0f))
        {
            float dot = Vector3.Dot(planarVel.normalized, transform.forward * Mathf.Sign(moveInput));
            if (dot < -0.2f)
                resistance *= brakeMultiplier;
        }

        rb.AddForce(-planarVel * resistance * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    private void StickToGround()
    {
        if (isGrounded)
            rb.AddForce(-smoothedNormal * groundStickForce, ForceMode.Acceleration);
    }

    private void ClampVelocity()
    {
        Vector3 planarVel = GetPlanarVelocityOnGround();
        if (planarVel.magnitude > maxMoveSpeed)
        {
            Vector3 clampedVel = planarVel.normalized * maxMoveSpeed;
            Vector3 verticalVel = Vector3.Project(rb.velocity, smoothedNormal);
            rb.velocity = clampedVel + verticalVel;
        }

        // 🚩 限制垂直上升速度（避免“飞起”）
        if (rb.velocity.y > 0.5f && isGrounded)
        {
            Vector3 v = rb.velocity;
            v.y = 0.5f;
            rb.velocity = v;
        }
    }

    private void UpdateGroundInfo()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float sphereRadius = 0.3f;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        smoothedNormal = Vector3.Lerp(smoothedNormal, groundNormal, Time.fixedDeltaTime / normalSmoothTime);
    }

    private Vector3 GetPlanarVelocityOnGround()
    {
        return Vector3.ProjectOnPlane(rb.velocity, smoothedNormal);
    }
}
