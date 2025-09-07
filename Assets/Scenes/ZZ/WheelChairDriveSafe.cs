using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // Keyboard.current
#endif

[RequireComponent(typeof(Rigidbody))]
public class WheelchairDriveSafe : MonoBehaviour
{
    public Rigidbody rb;

    [Header("动力/转向")]
    public float forwardForce = 200f;   // 调小推力，防止过猛导致翻车
    public float turnTorque = 50f;   // 调整转向力度，避免过度旋转导致翻车
    public float maxSpeed = 8f;    // 水平限速

    void Reset() { rb = GetComponent<Rigidbody>(); }
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, 0.5f, 0); // 调整重心，避免翻车
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (!rb) return;

        // 直接读取键盘输入（不依赖新输入系统）
        float h = 0f;
        if (Input.GetKey(KeyCode.A)) h = -1f;  // 左转
        if (Input.GetKey(KeyCode.D)) h = 1f;   // 右转

        Debug.Log($"Horizontal Input (h): {h}");  // 输出 h 值，看看是否正确

        // 前进推动力
        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);
        float v = (w ? 1f : 0f) + (s ? -1f : 0f);

        // 水平限速
        Vector3 hv = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (hv.magnitude < maxSpeed)
            rb.AddForce(transform.forward * (v * forwardForce), ForceMode.Force);

        // 转向
        if (Mathf.Abs(h) > 0.01f)
            rb.AddTorque(Vector3.up * (h * turnTorque), ForceMode.Force);
    }

}
