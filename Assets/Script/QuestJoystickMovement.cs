using UnityEngine;
using UnityEngine.XR;

public class WheelchairMovement : MonoBehaviour
{
    public float moveSpeed = 3f;          // 前进/后退速度
    public float turnSpeed = 60f;         // 转向速度(度/秒)
    public float headBobAmount = 0.05f;   // 头部上下抖动的幅度
    public float headBobSpeed = 2f;       // 头部抖动的速度

    private CharacterController characterController;
    private Transform cameraTransform;
    private Vector3 originalCameraPosition;
    private float headBobTimer = 0f;
    private float currentRotation = 0f;   // 当前旋转角度

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        originalCameraPosition = cameraTransform.localPosition;

        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    void Update()
    {
        // 获取左手摇杆输入
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input))
        {
            MovePlayer(input);
            HandleHeadBob(input.magnitude);
        }
        else
        {
            ResetHeadPosition();
        }
    }

    void MovePlayer(Vector2 input)
    {
        // 轮椅式移动：Y轴控制前进后退，X轴控制转向
        float moveInput = input.y;
        float turnInput = input.x;

        // 前进/后退移动
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * moveInput;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // 左右转向
        if (Mathf.Abs(turnInput) > 0.1f)
        {
            // 计算旋转角度(基于输入和速度)
            float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
            currentRotation += rotationAmount;

            // 应用旋转
            transform.Rotate(0, rotationAmount, 0);
        }
    }

    void HandleHeadBob(float inputMagnitude)
    {
        if (inputMagnitude > 0.1f)
        {
            headBobTimer += Time.deltaTime * headBobSpeed;
            float newY = originalCameraPosition.y + Mathf.Sin(headBobTimer) * headBobAmount * inputMagnitude;

            cameraTransform.localPosition = new Vector3(
                originalCameraPosition.x,
                newY,
                originalCameraPosition.z
            );
        }
        else
        {
            ResetHeadPosition();
        }
    }

    void ResetHeadPosition()
    {
        if (cameraTransform.localPosition != originalCameraPosition)
        {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                originalCameraPosition,
                Time.deltaTime * headBobSpeed
            );
        }
        headBobTimer = 0f;
    }
}