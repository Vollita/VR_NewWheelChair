using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorFloorTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportTarget;          // 目标位置
    public float blackScreenDuration = 2f;    // 黑屏持续时间
    public string playerTag = "Player";       // 玩家标签

    [Header("UI Settings")]
    public Canvas blackScreenCanvas;          // 黑屏Canvas（可选）
    public Image blackScreenImage;            // 黑屏Image（可选）

    [Header("Debug")]
    public bool showDebugInfo = true;

    private GameObject player;
    private bool isTeleporting = false;
    private Canvas createdCanvas;
    private Image createdImage;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning($"Player with tag '{playerTag}' not found!");
        }

        if (blackScreenCanvas == null || blackScreenImage == null)
        {
            CreateBlackScreenUI();
        }

        if (blackScreenCanvas != null)
        {
            blackScreenCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;

        if (other.CompareTag(playerTag))
        {
            if (showDebugInfo)
                Debug.Log("Player stepped on elevator floor. Starting teleport...");
            StartCoroutine(TeleportSequence());
        }
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        ShowBlackScreen();
        yield return new WaitForSeconds(blackScreenDuration);

        if (teleportTarget != null && player != null)
        {
            Transform playerTransform = player.transform;
            Transform parent = playerTransform.parent;

            if (parent != null && (parent.name.Contains("XR") || parent.name.Contains("Rig")))
            {
                parent.position = teleportTarget.position;
                parent.rotation = teleportTarget.rotation;
            }
            else
            {
                playerTransform.position = teleportTarget.position;
                playerTransform.rotation = teleportTarget.rotation;
            }

            if (showDebugInfo)
                Debug.Log("Teleport completed.");
        }
        else
        {
            Debug.LogWarning("Teleport target or player is missing!");
        }

        HideBlackScreen();
        isTeleporting = false;
    }

    void ShowBlackScreen()
    {
        if (blackScreenCanvas != null)
            blackScreenCanvas.gameObject.SetActive(true);
        else if (createdCanvas != null)
            createdCanvas.gameObject.SetActive(true);
    }

    void HideBlackScreen()
    {
        if (blackScreenCanvas != null)
            blackScreenCanvas.gameObject.SetActive(false);
        else if (createdCanvas != null)
            createdCanvas.gameObject.SetActive(false);
    }

    void CreateBlackScreenUI()
    {
        GameObject canvasObj = new GameObject("AutoBlackScreenCanvas");
        createdCanvas = canvasObj.AddComponent<Canvas>();
        createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        createdCanvas.sortingOrder = 1000;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("AutoBlackScreenImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        createdImage = imageObj.AddComponent<Image>();
        createdImage.color = Color.black;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        canvasObj.SetActive(false);
    }
}
