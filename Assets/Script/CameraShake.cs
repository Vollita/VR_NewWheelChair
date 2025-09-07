using UnityEngine;

public class CameraShakeInputBased : MonoBehaviour
{
    [Header("抖动设置")]
    public float shakeIntensity = 0.1f;
    public float shakeFrequency = 10f;
    public float minSpeedForShake = 0.1f; // 触发抖动的最小速度

    [Header("输入检测")]
    public bool useWASDInput = true;
    public bool useVRInput = true;

    private Vector3 baseLocalPosition;
    private float noiseSeed;
    private WASDWheel wasdController;
    private WheelchairDualWheelController vrController;
    private Rigidbody targetRigidbody;

    void Start()
    {
        baseLocalPosition = transform.localPosition;
        noiseSeed = Random.Range(0f, 100f);

        // 获取控制器组件
        wasdController = FindObjectOfType<WASDWheel>();
        vrController = FindObjectOfType<WheelchairDualWheelController>();

        // 获取刚体用于速度检测
        if (wasdController != null)
            targetRigidbody = wasdController.GetComponent<Rigidbody>();
        else if (vrController != null)
            targetRigidbody = vrController.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        bool hasInput = CheckForInput();
        bool isMoving = CheckIfMoving();

        if (hasInput && isMoving)
        {
            ApplyCameraShake();
        }
        else
        {
            ResetCameraPosition();
        }
    }

    private bool CheckForInput()
    {
        // 检查WASD输入
        if (useWASDInput)
        {
            float verticalInput = Input.GetAxis("Vertical");
            float horizontalInput = Input.GetAxis("Horizontal");

            if (Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(horizontalInput) > 0.1f)
            {
                return true;
            }
        }

        // 检查VR输入
        if (useVRInput && vrController != null)
        {
            // 通过反射获取私有字段（或者你可以修改WheelchairDualWheelController为public字段）
            System.Reflection.FieldInfo moveInputField = typeof(WheelchairDualWheelController).GetField("moveInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo turnInputField = typeof(WheelchairDualWheelController).GetField("turnInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (moveInputField != null && turnInputField != null)
            {
                float vrMoveInput = (float)moveInputField.GetValue(vrController);
                float vrTurnInput = (float)turnInputField.GetValue(vrController);

                if (Mathf.Abs(vrMoveInput) > 0.1f || Mathf.Abs(vrTurnInput) > 0.1f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckIfMoving()
    {
        if (targetRigidbody != null)
        {
            // 只考虑水平速度
            Vector3 horizontalVelocity = new Vector3(
                targetRigidbody.velocity.x,
                0f,
                targetRigidbody.velocity.z
            );

            return horizontalVelocity.magnitude > minSpeedForShake;
        }

        return false;
    }

    private void ApplyCameraShake()
    {
        // 计算抖动强度（基于速度）
        float speedFactor = targetRigidbody != null ?
            Mathf.Clamp01(targetRigidbody.velocity.magnitude / 5f) : 1f;

        float actualIntensity = shakeIntensity * speedFactor;

        // 使用Perlin噪声创建平滑抖动
        float shake = Mathf.PerlinNoise(noiseSeed, Time.time * shakeFrequency) * 2f - 1f;
        Vector3 shakeOffset = Vector3.up * shake * actualIntensity;

        // 应用抖动
        transform.localPosition = baseLocalPosition + shakeOffset;
    }

    private void ResetCameraPosition()
    {
        transform.localPosition = baseLocalPosition;
    }

    // 配置方法
    public void SetShakeParameters(float intensity, float frequency)
    {
        shakeIntensity = intensity;
        shakeFrequency = frequency;
    }

    public void SetInputSources(bool useWASD, bool useVR)
    {
        useWASDInput = useWASD;
        useVRInput = useVR;
    }
}