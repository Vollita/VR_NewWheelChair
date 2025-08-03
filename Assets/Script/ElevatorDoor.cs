using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElevatorDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform otherDoor;       // ��һ����
    public float openDuration = 1f;   // �Ŵ���Ҫ��ʱ��
    public float closeDuration = 1f;  // �Źر���Ҫ��ʱ��

    [Header("Auto Timer Settings")]
    public float openTime = 3f;       // �ű��ִ򿪵�ʱ��
    public float closeTime = 2f;      // �ű��ֹرյ�ʱ��
    public bool isMainDoor = true;    // �Ƿ�Ϊ�����ţ�ֻ��һ������Ϊtrue��

    private bool isOpen = false;
    private Coroutine currentRoutine;
    private Coroutine timerRoutine;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    // ���ڼ�������ײ
    private List<GameObject> playersInContact = new List<GameObject>();

    void Start()
    {
        // ����ԭʼ״̬���ر�״̬��
        originalScale = transform.localScale;
        originalPosition = transform.position;

        Debug.Log($"ElevatorDoor initialized on {gameObject.name}");

        // ֻ��������������ʱ���������ظ�
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
            // �ȴ��ر�ʱ��
            yield return new WaitForSeconds(closeTime);

            // ͬ������
            OpenDoorSync();

            // �ȴ���ʱ��
            yield return new WaitForSeconds(openTime);

            // ͬ������
            CloseDoorSync();
        }
    }

    void OpenDoorSync()
    {
        Debug.Log("Opening doors synchronously");

        // �����Լ�
        OpenDoor();

        // ������һ����
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

        // �ر��Լ�
        CloseDoor();

        // �ر���һ����
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

        // ��������Ŀ��
        if (otherDoor != null)
        {
            Vector3 direction = otherDoor.position - originalPosition;
            float distance = direction.magnitude;

            // ���ݷ���ȷ��������
            Vector3 absDir = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));

            if (absDir.x >= absDir.y && absDir.x >= absDir.z)
            {
                // X������
                targetScale = new Vector3(distance, originalScale.y, originalScale.z);
                targetPosition = originalPosition + new Vector3(direction.x * 0.5f, 0, 0);
            }
            else if (absDir.y >= absDir.z)
            {
                // Y������
                targetScale = new Vector3(originalScale.x, distance, originalScale.z);
                targetPosition = originalPosition + new Vector3(0, direction.y * 0.5f, 0);
            }
            else
            {
                // Z������
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
        // �������Ƿ�ͬʱ�Ӵ�������������
        ElevatorDoor[] allDoors = FindObjectsOfType<ElevatorDoor>();
        int contactCount = 0;

        foreach (ElevatorDoor door in allDoors)
        {
            if (door.playersInContact.Contains(player))
            {
                contactCount++;
            }
        }

        // ������ͬʱ�Ӵ����������������ţ���������
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