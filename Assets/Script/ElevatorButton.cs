using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElevatorButton : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportTarget;      // ����Ŀ��λ��
    public float blackScreenDuration = 2f; // ��������ʱ��

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E; // ���̽��������������ã�
    public float interactionRange = 2f;     // ��������
    public string playerTag = "Player";     // ��ұ�ǩ

    [Header("VR Settings")]
    public bool enableVRInteraction = true; // ����VR����
    public string vrControllerTag = "Controller"; // VR��������ǩ
    public string triggerButton = "Trigger"; // VR��������ť����
    public float vrActivationTime = 0.5f;   // VR��������ʱ��

    [Header("UI Settings")]
    public Canvas blackScreenCanvas;        // ����Canvas����ѡ�����Ϊ�ջ��Զ�������
    public Image blackScreenImage;          // ����Image����ѡ�����Ϊ�ջ��Զ�������

    [Header("Visual Feedback")]
    public GameObject buttonPressEffect;    // ��ť����ʱ���Ӿ�Ч��
    public Material buttonNormalMaterial;   // ��ť��������
    public Material buttonHighlightMaterial; // ��ť��������
    public AudioSource buttonSound;         // ��ť��Ч����ѡ��

    [Header("Debug")]
    public bool showDebugInfo = true;       // ��ʾ������Ϣ

    private GameObject player;
    private bool isPlayerInRange = false;
    private bool isTeleporting = false;
    private Canvas createdCanvas;
    private Image createdImage;

    // VR���
    private bool isVRControllerInRange = false;
    private GameObject currentVRController;
    private float vrButtonHoldTime = 0f;
    private bool vrButtonPressed = false;
    private Renderer buttonRenderer;

    void Start()
    {
        // Ѱ�����
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning($"Player with tag '{playerTag}' not found!");
        }

        // ��ȡ��ť��Ⱦ�����ڲ����л�
        buttonRenderer = GetComponent<Renderer>();

        // ���û��ָ������UI���Զ�����
        if (blackScreenCanvas == null || blackScreenImage == null)
        {
            CreateBlackScreenUI();
        }

        // ȷ������UI��ʼʱ�����ص�
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

        // �������Ƿ��ڽ�����Χ�ڣ����̽�����
        if (player != null)
        {
            CheckPlayerInRange();

            // ���̽������
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                ActivateButton();
            }
        }

        // VR�������
        if (enableVRInteraction)
        {
            HandleVRInteraction();
        }

        // ���°�ť�Ӿ�״̬
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
        // ���VR����������
        if (isVRControllerInRange && currentVRController != null)
        {
            // ���Զ���VR���뷽ʽ
            bool triggerPressed = false;

            // ��ʽ1: Unity Input Manager
            try
            {
                triggerPressed = Input.GetButton(triggerButton) ||
                               Input.GetAxis(triggerButton) > 0.5f ||
                               Input.GetButton("Fire1"); // ���ð�ť
            }
            catch
            {
                // ���Input Manager��û�����ö�Ӧ��ť�����Դ���
            }

            // ��ʽ2: XR Input (�������)
#if UNITY_XR_HANDS || UNITY_XR_INTERACTION_TOOLKIT
            try
            {
                // ����������XR�ض���������
                // ����: triggerPressed |= XRController.leftHand.trigger.isPressed;
            }
            catch
            {
                // XRϵͳ������ʱ����
            }
#endif

            // ��ʽ3: ��������ΪVR�������ı��ã������ã�
            if (!triggerPressed)
            {
                triggerPressed = Input.GetMouseButton(0);
            }

            // �������߼�
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

                // �ﵽ����ʱ��ʱ������ť
                if (vrButtonHoldTime >= vrActivationTime)
                {
                    ActivateButton();
                    vrButtonPressed = false; // ��ֹ�ظ�����
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
        // ���°�ť����
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

        // ��ʾ��ѹЧ��
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

        // ���Ű�ť��Ч
        if (buttonSound != null)
        {
            buttonSound.Play();
        }

        // ���������������VR�����������ģ�
        if (currentVRController != null)
        {
            TriggerVRHapticFeedback();
        }

        // ��ʼ��������
        StartCoroutine(TeleportSequence());
    }

    void TriggerVRHapticFeedback()
    {
        // ���Դ���VR�������Ĵ�������
        try
        {
            // �����������ض�VRƽ̨�Ĵ�����������
            // SteamVR: SteamVR_Actions.default_Haptic.Execute(0, 0.3f, 75, 1.0f, inputSource);
            // Oculus: OVRInput.SetControllerVibration(frequency, amplitude, controller);

            if (showDebugInfo)
            {
                Debug.Log("VR haptic feedback triggered");
            }
        }
        catch
        {
            // ���VRϵͳ�����ã����Դ���
        }
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        // ��ʾ����
        ShowBlackScreen();

        if (showDebugInfo)
        {
            Debug.Log("Black screen activated, waiting...");
        }

        // �ȴ�����ʱ��
        yield return new WaitForSeconds(blackScreenDuration);

        // ִ�д���
        if (teleportTarget != null && player != null)
        {
            // VR���ͨ����Ҫ��������XR Rig
            Transform playerTransform = player.transform;

            // ����Ƿ���VR��ң�Ѱ��XR Rig�����Ƶĸ�����
            Transform xrRig = playerTransform.parent;
            if (xrRig != null && (xrRig.name.Contains("XR") || xrRig.name.Contains("VR") || xrRig.name.Contains("Rig")))
            {
                // ����XR Rig
                xrRig.position = teleportTarget.position;
                xrRig.rotation = teleportTarget.rotation;

                if (showDebugInfo)
                {
                    Debug.Log($"VR Rig teleported to {teleportTarget.position}");
                }
            }
            else
            {
                // ������ͨ���
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

        // ���غ���
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
        // ����Canvas
        GameObject canvasObj = new GameObject("BlackScreenCanvas");
        createdCanvas = canvasObj.AddComponent<Canvas>();
        createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        createdCanvas.sortingOrder = 1000; // ȷ�������ϲ�

        // ���Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // ���Graphic Raycaster
        canvasObj.AddComponent<GraphicRaycaster>();

        // ������ɫImage
        GameObject imageObj = new GameObject("BlackScreenImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        createdImage = imageObj.AddComponent<Image>();
        createdImage.color = Color.black;

        // ����ImageΪȫ��
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // ��ʼʱ����
        canvasObj.SetActive(false);

        if (showDebugInfo)
        {
            Debug.Log("Black screen UI created automatically");
        }
    }

    // ����ͨ�������ű��������������������ť
    public void TriggerButton()
    {
        if (!isTeleporting)
        {
            ActivateButton();
        }
    }

    // ��Scene��ͼ����ʾ������Χ
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

    // ��������� - ͬʱ֧����Һ�VR������
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