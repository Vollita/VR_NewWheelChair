using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceUICenter : MonoBehaviour
{
    [Tooltip("UI与摄像机之间的距离")]
    public float distanceFromCamera = 1.0f;

    [Tooltip("是否跟随摄像机旋转")]
    public bool followCameraRotation = true;

    [Tooltip("UI在X轴上的偏移量")]
    public float offsetX = 0;

    [Tooltip("UI在Y轴上的偏移量")]
    public float offsetY = 0;

    private Canvas uiCanvas;
    private Camera mainCamera;

    void Start()
    {
        // 获取Canvas组件并设置为World Space模式
        uiCanvas = GetComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.WorldSpace;

        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("场景中没有主摄像机，请添加一个摄像机并设置为MainCamera");
            enabled = false;
            return;
        }

        // 初始化UI位置到摄像机正前方
        UpdateUIPosition();
    }

    void LateUpdate()
    {
        // 每帧更新UI位置，确保始终在摄像机正中间
        UpdateUIPosition();
    }

    void UpdateUIPosition()
    {
        if (mainCamera == null) return;

        // 计算UI应该在的位置：摄像机前方指定距离处
        Vector3 targetPosition = mainCamera.transform.position +
                                mainCamera.transform.forward * distanceFromCamera +
                                mainCamera.transform.right * offsetX +
                                mainCamera.transform.up * offsetY;

        // 设置UI位置
        transform.position = targetPosition;

        // 如果需要，让UI始终面向摄像机
        if (followCameraRotation)
        {
            transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);
        }
    }
}
