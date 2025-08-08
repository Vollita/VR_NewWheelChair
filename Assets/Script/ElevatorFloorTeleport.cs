using UnityEngine;
using UnityEngine.UI;
public class ElevatorTrigger : MonoBehaviour
{
    public float detectRadius = 1f;             // 探测半径
    public Transform player;                    // 手动指定玩家物体
    public Transform teleportTarget;            // 传送目的地
    public Transform lookTarget;                // 传送后面朝的目标
    public float teleportDelay = 2f;            // 传送延时（秒）
    public float blackScreenTime = 1f;          // 黑屏持续时间
    private bool hasTeleported = false;
    private bool isCountingDown = false;
    private Image blackScreenImage;
    private Canvas canvas;

    void Start()
    {
        CreateBlackScreen();

        // 如果没有手动指定玩家，尝试自动查找
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (hasTeleported || player == null || teleportTarget == null)
            return;

        // 检测玩家是否在电梯地板正上方 1 米以内的范围
        Vector3 center = transform.position + Vector3.up * 1f;
        Collider[] hits = Physics.OverlapSphere(center, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.transform == player && !isCountingDown)
            {
                isCountingDown = true;
                ShowBlackScreen();
                Invoke(nameof(TeleportPlayer), teleportDelay); // 延时传送
                break;
            }
        }
    }

    void TeleportPlayer()
    {
        if (hasTeleported) return;

        // 传送玩家 - 确保传送到正确的位置
        if (player != null && teleportTarget != null)
        {
            // 如果玩家有CharacterController，需要特殊处理
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.position = teleportTarget.position;
                cc.enabled = true;
            }
            else
            {
                player.position = teleportTarget.position;
            }

            // 设置面朝方向
            if (lookTarget != null)
            {
                Vector3 direction = (lookTarget.position - player.position);
                direction.y = 0; // 保持水平方向
                if (direction.sqrMagnitude > 0.001f) // 使用sqrMagnitude避免开方运算，并确保方向不为零
                {
                    player.rotation = Quaternion.LookRotation(direction.normalized);
                }
            }

            hasTeleported = true;
            Debug.Log("玩家被传送并调整朝向");

            // 打开所有带有 ElevatorArrivalDoor 脚本且Tag为 "arriveDoor" 的门
            GameObject[] doors = GameObject.FindGameObjectsWithTag("arriveDoor");
            foreach (var doorObj in doors)
            {
                ElevatorArrivalDoor doorScript = doorObj.GetComponent<ElevatorArrivalDoor>();
                if (doorScript != null)
                {
                    doorScript.OpenDoor();
                }
            }

            Invoke(nameof(HideBlackScreen), blackScreenTime); // 黑屏一段时间后恢复
        }
        else
        {
            Debug.LogError("传送失败：player 或 teleportTarget 为空");
            HideBlackScreen(); // 即使失败也要隐藏黑屏
        }
    }

    void CreateBlackScreen()
    {
        GameObject canvasObj = new GameObject("AutoCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        // 添加CanvasScaler确保在不同分辨率下正常工作
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject imgObj = new GameObject("BlackScreen");
        imgObj.transform.SetParent(canvasObj.transform, false);
        blackScreenImage = imgObj.AddComponent<Image>();
        blackScreenImage.color = new Color(0, 0, 0, 0); // 初始透明

        RectTransform rect = blackScreenImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 确保Canvas不会被意外销毁
        DontDestroyOnLoad(canvasObj);
    }

    void ShowBlackScreen()
    {
        if (blackScreenImage != null)
            blackScreenImage.color = Color.black;
    }

    void HideBlackScreen()
    {
        if (blackScreenImage != null)
            blackScreenImage.color = new Color(0, 0, 0, 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, detectRadius);
    }

    void OnDestroy()
    {
        // 清理资源
        if (canvas != null)
            DestroyImmediate(canvas.gameObject);
    }
}