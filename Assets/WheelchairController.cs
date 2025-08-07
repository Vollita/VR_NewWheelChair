using UnityEngine;
using UnityEngine.InputSystem;

public class WheelchairController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float turnSpeed = 50f;
    public bool isUsingVR = false;

    private Rigidbody rb;
    private Vector2 lastInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 input = Vector2.zero;

        if (!isUsingVR)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        lastInput = input;

        Vector3 move = transform.forward * input.y * moveSpeed * Time.fixedDeltaTime;
        Quaternion turn = Quaternion.Euler(0, input.x * turnSpeed * Time.fixedDeltaTime, 0);

        rb.MovePosition(rb.position + move);
        rb.MoveRotation(rb.rotation * turn);
    }

    // 新增：提供抖动判断条件
    public bool IsManuallyMoving()
    {
        return lastInput != Vector2.zero;
    }
}
