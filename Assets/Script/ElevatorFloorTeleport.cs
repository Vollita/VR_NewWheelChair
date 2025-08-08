using UnityEngine;
using UnityEngine.UI;
public class ElevatorTrigger : MonoBehaviour
{
    public float detectRadius = 1f;             // ̽��뾶
    public Transform player;                    // �ֶ�ָ���������
    public Transform teleportTarget;            // ����Ŀ�ĵ�
    public Transform lookTarget;                // ���ͺ��泯��Ŀ��
    public float teleportDelay = 2f;            // ������ʱ���룩
    public float blackScreenTime = 1f;          // ��������ʱ��
    private bool hasTeleported = false;
    private bool isCountingDown = false;
    private Image blackScreenImage;
    private Canvas canvas;

    void Start()
    {
        CreateBlackScreen();

        // ���û���ֶ�ָ����ң������Զ�����
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

        // �������Ƿ��ڵ��ݵذ����Ϸ� 1 �����ڵķ�Χ
        Vector3 center = transform.position + Vector3.up * 1f;
        Collider[] hits = Physics.OverlapSphere(center, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.transform == player && !isCountingDown)
            {
                isCountingDown = true;
                ShowBlackScreen();
                Invoke(nameof(TeleportPlayer), teleportDelay); // ��ʱ����
                break;
            }
        }
    }

    void TeleportPlayer()
    {
        if (hasTeleported) return;

        // ������� - ȷ�����͵���ȷ��λ��
        if (player != null && teleportTarget != null)
        {
            // ��������CharacterController����Ҫ���⴦��
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

            // �����泯����
            if (lookTarget != null)
            {
                Vector3 direction = (lookTarget.position - player.position);
                direction.y = 0; // ����ˮƽ����
                if (direction.sqrMagnitude > 0.001f) // ʹ��sqrMagnitude���⿪�����㣬��ȷ������Ϊ��
                {
                    player.rotation = Quaternion.LookRotation(direction.normalized);
                }
            }

            hasTeleported = true;
            Debug.Log("��ұ����Ͳ���������");

            // �����д��� ElevatorArrivalDoor �ű���TagΪ "arriveDoor" ����
            GameObject[] doors = GameObject.FindGameObjectsWithTag("arriveDoor");
            foreach (var doorObj in doors)
            {
                ElevatorArrivalDoor doorScript = doorObj.GetComponent<ElevatorArrivalDoor>();
                if (doorScript != null)
                {
                    doorScript.OpenDoor();
                }
            }

            Invoke(nameof(HideBlackScreen), blackScreenTime); // ����һ��ʱ���ָ�
        }
        else
        {
            Debug.LogError("����ʧ�ܣ�player �� teleportTarget Ϊ��");
            HideBlackScreen(); // ��ʹʧ��ҲҪ���غ���
        }
    }

    void CreateBlackScreen()
    {
        GameObject canvasObj = new GameObject("AutoCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        // ���CanvasScalerȷ���ڲ�ͬ�ֱ�������������
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject imgObj = new GameObject("BlackScreen");
        imgObj.transform.SetParent(canvasObj.transform, false);
        blackScreenImage = imgObj.AddComponent<Image>();
        blackScreenImage.color = new Color(0, 0, 0, 0); // ��ʼ͸��

        RectTransform rect = blackScreenImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // ȷ��Canvas���ᱻ��������
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
        // ������Դ
        if (canvas != null)
            DestroyImmediate(canvas.gameObject);
    }
}