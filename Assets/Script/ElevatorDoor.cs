using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElevatorDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform otherDoor;       // 另一扇门
    public float openDuration = 1f;   // 门打开需要的时间
    public float closeDuration = 1f;  // 门关闭需要的时间

    [Header("Auto Timer Settings")]
    public float openTime = 3f;       // 门保持打开的时间
    public float closeTime = 2f;      // 门保持关闭的时间
    public bool isMainDoor = true;    // 是否为主控门（只有一个门设为true）

    private bool isOpen = false;
    private Coroutine currentRoutine;
    private Coroutine timerRoutine;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    // 用于检测玩家碰撞
    private List<GameObject> playersInContact = new List<GameObject>();

    void Start()
    {
        // 保存原始状态（关闭状态）
        originalScale = transform.localScale;
        originalPosition = transform.position;

        Debug.Log($"ElevatorDoor initialized on {gameObject.name}");

        // 只有主控门启动定时器，避免重复
        if (isMainDoor)
        {
            timerRoutine = StartCoroutine(AutoDoorTimer());
        }
    }

    void OnDestroy()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
        }
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
    }

    IEnumerator AutoDoorTimer()
    {
        while (true)
        {
            // 等待关闭时间
            yield return new WaitForSeconds(closeTime);

            // 同步开门
            OpenDoorSync();

            // 等待打开时间
            yield return new WaitForSeconds(openTime);

            // 同步关门
            CloseDoorSync();
        }
    }

    void OpenDoorSync()
    {
        Debug.Log("Opening doors synchronously");

        // 开启自己
        OpenDoor();

        // 开启另一个门
        if (otherDoor != null)
        {
            ElevatorDoor otherDoorScript = otherDoor.GetComponent<ElevatorDoor>();
            if (otherDoorScript != null)
            {
                otherDoorScript.OpenDoor();
            }
        }
    }

    void CloseDoorSync()
    {
        Debug.Log("Closing doors synchronously");

        // 关闭自己
        CloseDoor();

        // 关闭另一个门
        if (otherDoor != null)
        {
            ElevatorDoor otherDoorScript = otherDoor.GetComponent<ElevatorDoor>();
            if (otherDoorScript != null)
            {
                otherDoorScript.CloseDoor();
            }
        }
    }

    public void OpenDoor()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(OpenDoorCoroutine());
    }

    public void CloseDoor()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(CloseDoorCoroutine());
    }

    IEnumerator OpenDoorCoroutine()
    {
        isOpen = true;

        Vector3 targetScale = originalScale;
        Vector3 targetPosition = originalPosition;

        // 计算延伸目标
        if (otherDoor != null)
        {
            Vector3 direction = otherDoor.position - originalPosition;
            float distance = direction.magnitude;

            // 根据方向确定延伸轴
            Vector3 absDir = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));

            if (absDir.x >= absDir.y && absDir.x >= absDir.z)
            {
                // X轴延伸
                targetScale = new Vector3(distance, originalScale.y, originalScale.z);
                targetPosition = originalPosition + new Vector3(direction.x * 0.5f, 0, 0);
            }
            else if (absDir.y >= absDir.z)
            {
                // Y轴延伸
                targetScale = new Vector3(originalScale.x, distance, originalScale.z);
                targetPosition = originalPosition + new Vector3(0, direction.y * 0.5f, 0);
            }
            else
            {
                // Z轴延伸
                targetScale = new Vector3(originalScale.x, originalScale.y, distance);
                targetPosition = originalPosition + new Vector3(0, 0, direction.z * 0.5f);
            }
        }

        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / openDuration;

            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = targetPosition;
        currentRoutine = null;
    }

    IEnumerator CloseDoorCoroutine()
    {
        isOpen = false;

        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / closeDuration;

            transform.localScale = Vector3.Lerp(startScale, originalScale, progress);
            transform.position = Vector3.Lerp(startPosition, originalPosition, progress);

            yield return null;
        }

        transform.localScale = originalScale;
        transform.position = originalPosition;
        currentRoutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playersInContact.Contains(other.gameObject))
            {
                playersInContact.Add(other.gameObject);
                CheckForDeath(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInContact.Remove(other.gameObject);
        }
    }

    void CheckForDeath(GameObject player)
    {
        // 检查玩家是否同时接触到两个电梯门
        ElevatorDoor[] allDoors = FindObjectsOfType<ElevatorDoor>();
        int contactCount = 0;

        foreach (ElevatorDoor door in allDoors)
        {
            if (door.playersInContact.Contains(player))
            {
                contactCount++;
            }
        }

        // 如果玩家同时接触到两个或更多电梯门，触发死亡
        if (contactCount >= 2)
        {
            DeathHandler deathHandler = player.GetComponent<DeathHandler>();
            if (deathHandler != null)
            {
                deathHandler.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public bool IsMoving()
    {
        return currentRoutine != null;
    }
}