using UnityEngine;
using UnityEngine.XR;

public class WheelchairMovement : MonoBehaviour
{
    public float moveSpeed = 3f;          // ǰ��/�����ٶ�
    public float turnSpeed = 60f;         // ת���ٶ�(��/��)
    public float headBobAmount = 0.05f;   // ͷ�����¶����ķ���
    public float headBobSpeed = 2f;       // ͷ���������ٶ�

    private CharacterController characterController;
    private Transform cameraTransform;
    private Vector3 originalCameraPosition;
    private float headBobTimer = 0f;
    private float currentRotation = 0f;   // ��ǰ��ת�Ƕ�

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
        // ��ȡ����ҡ������
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
        // ����ʽ�ƶ���Y�����ǰ�����ˣ�X�����ת��
        float moveInput = input.y;
        float turnInput = input.x;

        // ǰ��/�����ƶ�
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * moveInput;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // ����ת��
        if (Mathf.Abs(turnInput) > 0.1f)
        {
            // ������ת�Ƕ�(����������ٶ�)
            float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
            currentRotation += rotationAmount;

            // Ӧ����ת
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