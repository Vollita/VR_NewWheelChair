using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("���������")]
    public float mouseSensitivity = 100f;

    [Header("�ӽ�����")]
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    [Header("�Ƿ��������")]
    public bool lockCursor = true;

    private Transform playerBody;  // ������壨ˮƽ��ת��
    private float xRotation = 0f;  // ��ֱ��ת�Ƕ�

    void Start()
    {
        // ��ȡ������壨�����壩
        playerBody = transform.parent;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // ��ȡ�������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ��ֱ�ӽǿ���
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ˮƽ�ӽǿ���
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void OnDestroy()
    {
        // �ָ����״̬
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}