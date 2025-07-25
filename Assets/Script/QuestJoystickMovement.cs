using UnityEngine;
using UnityEngine.XR;

public class WheelchairMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 60f;

    private CharacterController characterController;
    private WheelchairSeat seat;  // ÐéÄâ×øµæÒýÓÃ

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        seat = GetComponentInChildren<WheelchairSeat>();

        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    void Update()
    {
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input))
        {
            MovePlayer(input);
            seat?.ApplyHeadBob(input.magnitude);
        }
        else
        {
            seat?.ResetSeatPosition();
        }
    }

    void MovePlayer(Vector2 input)
    {
        float moveInput = input.y;
        float turnInput = input.x;

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * moveInput;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        if (Mathf.Abs(turnInput) > 0.1f)
        {
            float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);
        }
    }
}
