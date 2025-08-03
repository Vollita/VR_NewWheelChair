using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElevatorButton : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportTarget;      // 传送目标位置
    public float blackScreenDuration = 2f; // 黑屏持续时间

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E; // 键盘交互按键（测试用）
    public float interactionRange = 2f;     // 交互距离
    public string playerTag = "Player";     // 玩家标签

    [Header("VR Settings")]
    public bool enableVRInteraction = true; // 启用VR交互
    public string vrControllerTag = "Controller"; // VR控制器标签
    public string triggerButton = "Trigger"; // VR触发器按钮名称
    public float vrActivationTime = 0.5f;   // VR长按激活时间

    [Header("UI Settings")]
    public Canvas blackScreenCanvas;        // 黑屏Canvas（可选，如果为空会自动创建）
    public Image blackScreenImage;          // 黑屏Image（可选，如果为空会自动创建）

    [Header("Visual Feedback")]
    public GameObject buttonPressEffect;    // 按钮按下时的视觉效果
    public Material buttonNormalMaterial;   // 按钮正常材质
    public Material buttonHighlightMaterial; // 按钮高亮材质
    public AudioSource buttonSound;         // 按钮音效（可选）

    [Header("Debug")]
    public bool showDebugInfo = true;       // 显示调试信息

    private GameObject player;
    private bool isPlayerInRange = false;
    private bool isTeleporting = false;
    private Canvas createdCanvas;
    private Image createdImage;

    // VR相关
    private bool isVRControllerInRange = false;
    private GameObject currentVRController;
    private float vrButtonHoldTime = 0f;
    private bool vrButtonPressed = false;
    private Renderer buttonRenderer;

    void Start()
    {
        // 寻找玩家
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning($"Player with tag '{playerTag}' not found!");
        }

        // 获取按钮渲染器用于材质切换
        buttonRenderer = GetComponent<Renderer>();

        // 如果没有指定黑屏UI，自动创建
        if (blackScreenCanvas == null || blackScreenImage == null)
        {
            CreateBlackScreenUI();
        }

        // 确保黑屏UI开始时是隐藏的
        if (blackScreenCanvas != null)
        {
            blackScreenCanvas.gameObject.SetActive(false);
        }

        if (showDebugInfo)
        {
            Debug.Log($"ElevatorButton initialized on {gameObject.name}");
        }
    }

    void Update()
    {
        if (isTeleporting) return;

        // 检查玩家是否在交互范围内（键盘交互）
        if (player != null)
        {
            CheckPlayerInRange();

            // 键盘交互检测
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                ActivateButton();
            }
        }

        // VR交互检测
        if (enableVRInteraction)
        {
            HandleVRInteraction();
        }

        // 更新按钮视觉状态
        UpdateButtonVisuals();
    }

    void CheckPlayerInRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;

        if (isPlayerInRange && !wasInRange && showDebugInfo)
        {
            Debug.Log($"Player entered interaction range of {gameObject.name}");
        }
        else if (!isPlayerInRange && wasInRange && showDebugInfo)
        {
            Debug.Log($"Player left interaction range of {gameObject.name}");
        }
    }

    void HandleVRInteraction()
    {
        // 检查VR控制器输入
        if (isVRControllerInRange && currentVRController != null)
        {
            // 尝试多种VR输入方式
            bool triggerPressed = false;

            // 方式1: Unity Input Manager
            try
            {
                triggerPressed = Input.GetButton(triggerButton) ||
                               Input.GetAxis(triggerButton) > 0.5f ||
                               Input.GetButton("Fire1"); // 备用按钮
            }
            catch
            {
                // 如果Input Manager中没有设置对应按钮，忽略错误
            }

            // 方式2: XR Input (如果可用)
#if UNITY_XR_HANDS || UNITY_XR_INTERACTION_TOOLKIT
            try
            {
                // 这里可以添加XR特定的输入检测
                // 例如: triggerPressed |= XRController.leftHand.trigger.isPressed;
            }
            catch
            {
                // XR系统不可用时忽略
            }
#endif

            // 方式3: 检查鼠标作为VR控制器的备用（测试用）
            if (!triggerPressed)
            {
                triggerPressed = Input.GetMouseButton(0);
            }

            // 处理长按逻辑
            if (triggerPressed)
            {
                if (!vrButtonPressed)
                {
                    vrButtonPressed = true;
                    vrButtonHoldTime = 0f;
                    if (showDebugInfo)
                    {
                        Debug.Log("VR trigger pressed, starting hold timer");
                    }
                }

                vrButtonHoldTime += Time.deltaTime;

                // 达到激活时间时触发按钮
                if (vrButtonHoldTime >= vrActivationTime)
                {
                    ActivateButton();
                    vrButtonPressed = false; // 防止重复触发
                    vrButtonHoldTime = 0f;
                }
            }
            else
            {
                if (vrButtonPressed && showDebugInfo)
                {
                    Debug.Log("VR trigger released before activation");
                }
                vrButtonPressed = false;
                vrButtonHoldTime = 0f;
            }
        }
    }

    void UpdateButtonVisuals()
    {
        // 更新按钮材质
        if (buttonRenderer != null)
        {
            bool shouldHighlight = isPlayerInRange || isVRControllerInRange || vrButtonPressed;

            if (shouldHighlight && buttonHighlightMaterial != null)
            {
                buttonRenderer.material = buttonHighlightMaterial;
            }
            else if (buttonNormalMaterial != null)
            {
                buttonRenderer.material = buttonNormalMaterial;
            }
        }

        // 显示按压效果
        if (buttonPressEffect != null)
        {
            buttonPressEffect.SetActive(vrButtonPressed && vrButtonHoldTime > 0.1f);
        }
    }

    public void ActivateButton()
    {
        if (isTeleporting) return;

        if (showDebugInfo)
        {
            Debug.Log($"Button activated on {gameObject.name}");
        }

        // 播放按钮音效
        if (buttonSound != null)
        {
            buttonSound.Play();
        }

        // 触觉反馈（如果是VR控制器触发的）
        if (currentVRController != null)
        {
            TriggerVRHapticFeedback();
        }

        // 开始传送流程
        StartCoroutine(TeleportSequence());
    }

    void TriggerVRHapticFeedback()
    {
        // 尝试触发VR控制器的触觉反馈
        try
        {
            // 这里可以添加特定VR平台的触觉反馈代码
            // SteamVR: SteamVR_Actions.default_Haptic.Execute(0, 0.3f, 75, 1.0f, inputSource);
            // Oculus: OVRInput.SetControllerVibration(frequency, amplitude, controller);

            if (showDebugInfo)
            {
                Debug.Log("VR haptic feedback triggered");
            }
        }
        catch
        {
            // 如果VR系统不可用，忽略错误
        }
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        // 显示黑屏
        ShowBlackScreen();

        if (showDebugInfo)
        {
            Debug.Log("Black screen activated, waiting...");
        }

        // 等待黑屏时间
        yield return new WaitForSeconds(blackScreenDuration);

        // 执行传送
        if (teleportTarget != null && player != null)
        {
            // VR玩家通常需要传送整个XR Rig
            Transform playerTransform = player.transform;

            // 检查是否是VR玩家（寻找XR Rig或类似的父对象）
            Transform xrRig = playerTransform.parent;
            if (xrRig != null && (xrRig.name.Contains("XR") || xrRig.name.Contains("VR") || xrRig.name.Contains("Rig")))
            {
                // 传送XR Rig
                xrRig.position = teleportTarget.position;
                xrRig.rotation = teleportTarget.rotation;

                if (showDebugInfo)
                {
                    Debug.Log($"VR Rig teleported to {teleportTarget.position}");
                }
            }
            else
            {
                // 传送普通玩家
                playerTransform.position = teleportTarget.position;
                playerTransform.rotation = teleportTarget.rotation;

                if (showDebugInfo)
                {
                    Debug.Log($"Player teleported to {teleportTarget.position}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Teleport target or player is null!");
        }

        // 隐藏黑屏
        HideBlackScreen();

        if (showDebugInfo)
        {
            Debug.Log("Teleport sequence completed");
        }

        isTeleporting = false;
    }

    void ShowBlackScreen()
    {
        if (blackScreenCanvas != null)
        {
            blackScreenCanvas.gameObject.SetActive(true);
        }
        else if (createdCanvas != null)
        {
            createdCanvas.gameObject.SetActive(true);
        }
    }

    void HideBlackScreen()
    {
        if (blackScreenCanvas != null)
        {
            blackScreenCanvas.gameObject.SetActive(false);
        }
        else if (createdCanvas != null)
        {
            createdCanvas.gameObject.SetActive(false);
        }
    }

    void CreateBlackScreenUI()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("BlackScreenCanvas");
        createdCanvas = canvasObj.AddComponent<Canvas>();
        createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        createdCanvas.sortingOrder = 1000; // 确保在最上层

        // 添加Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 添加Graphic Raycaster
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建黑色Image
        GameObject imageObj = new GameObject("BlackScreenImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        createdImage = imageObj.AddComponent<Image>();
        createdImage.color = Color.black;

        // 设置Image为全屏
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // 初始时隐藏
        canvasObj.SetActive(false);

        if (showDebugInfo)
        {
            Debug.Log("Black screen UI created automatically");
        }
    }

    // 可以通过其他脚本调用这个方法来触发按钮
    public void TriggerButton()
    {
        if (!isTeleporting)
        {
            ActivateButton();
        }
    }

    // 在Scene视图中显示交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (teleportTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, teleportTarget.position);
            Gizmos.DrawWireCube(teleportTarget.position, Vector3.one);
        }
    }

    // 触发器检测 - 同时支持玩家和VR控制器
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            if (showDebugInfo)
            {
                Debug.Log($"Player entered trigger of {gameObject.name}");
            }
        }
        else if (other.CompareTag(vrControllerTag) || other.name.Contains("Controller"))
        {
            isVRControllerInRange = true;
            currentVRController = other.gameObject;
            if (showDebugInfo)
            {
                Debug.Log($"VR Controller entered trigger of {gameObject.name}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (showDebugInfo)
            {
                Debug.Log($"Player left trigger of {gameObject.name}");
            }
        }
        else if (other.CompareTag(vrControllerTag) || other.name.Contains("Controller"))
        {
            isVRControllerInRange = false;
            currentVRController = null;
            vrButtonPressed = false;
            vrButtonHoldTime = 0f;
            if (showDebugInfo)
            {
                Debug.Log($"VR Controller left trigger of {gameObject.name}");
            }
        }
    }
}