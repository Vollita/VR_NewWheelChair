using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("鼠标灵敏度")]
    public float mouseSensitivity = 100f;

    [Header("视角限制")]
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    [Header("是否锁定鼠标")]
    public bool lockCursor = true;

    private Transform playerBody;  // 玩家身体（水平旋转）
    private float xRotation = 0f;  // 垂直旋转角度

    void Start()
    {
        // 获取玩家身体（父物体）
        playerBody = transform.parent;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 垂直视角控制
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 水平视角控制
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void OnDestroy()
    {
        // 恢复鼠标状态
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}