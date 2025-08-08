using UnityEngine;
using System.Collections;

public class ElevatorArrivalDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform otherDoor;          // ����������
    public float openDuration = 1f;      // ���Ŷ���ʱ��
    public float targetThickness = 0.2f; // ���ź��ȣ���״��
    public Vector3 shrinkAxis = Vector3.right; // ���ŷ���ͨ���� Vector3.right��

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Vector3 originalPos;
    private Vector3 targetPos;

    private bool hasOpened = false;
    private Coroutine currentRoutine;

    void Start()
    {
        originalScale = transform.localScale;
        originalPos = transform.position;

        // �������ź�ĳߴ�
        targetScale = originalScale;
        if (shrinkAxis == Vector3.right)
            targetScale.x = targetThickness;
        else if (shrinkAxis == Vector3.up)
            targetScale.y = targetThickness;
        else if (shrinkAxis == Vector3.forward)
            targetScale.z = targetThickness;

        // ���������������ﷴ��ƫ�ƣ���֮ǰ�����෴
        Vector3 dir = -shrinkAxis.normalized;

        float originalSize = 0f;
        float targetSize = 0f;

        if (shrinkAxis == Vector3.right)
        {
            originalSize = originalScale.x;
            targetSize = targetScale.x;
        }
        else if (shrinkAxis == Vector3.up)
        {
            originalSize = originalScale.y;
            targetSize = targetScale.y;
        }
        else if (shrinkAxis == Vector3.forward)
        {
            originalSize = originalScale.z;
            targetSize = targetScale.z;
        }

        float moveOffset = (originalSize - targetSize) * 0.5f;

        targetPos = originalPos + dir * moveOffset;
    }

    /// <summary>
    /// �ⲿ���ã��������ţ�ִֻ��һ�Σ�
    /// </summary>
    public void OpenDoor()
    {
        if (hasOpened) return;
        hasOpened = true;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(OpenDoorCoroutine());

        // ����һ����Ҳ����������
        if (otherDoor != null)
        {
            ElevatorArrivalDoor otherScript = otherDoor.GetComponent<ElevatorArrivalDoor>();
            if (otherScript != null)
                otherScript.OpenDoorMirror();
        }
    }

    /// <summary>
    /// ���ŵ��õĿ��ţ�����������
    /// </summary>
    private void OpenDoorMirror()
    {
        if (hasOpened) return;
        hasOpened = true;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // ���������������ﷴ��ƫ�ƣ���֮ǰ�����෴
        Vector3 dir = shrinkAxis.normalized;

        float originalSize = 0f;
        float targetSize = 0f;

        if (shrinkAxis == Vector3.right)
        {
            originalSize = originalScale.x;
            targetSize = targetScale.x;
        }
        else if (shrinkAxis == Vector3.up)
        {
            originalSize = originalScale.y;
            targetSize = targetScale.y;
        }
        else if (shrinkAxis == Vector3.forward)
        {
            originalSize = originalScale.z;
            targetSize = targetScale.z;
        }

        float moveOffset = (originalSize - targetSize) * 0.5f;

        Vector3 mirrorTargetPos = originalPos + dir * moveOffset;

        currentRoutine = StartCoroutine(OpenDoorCoroutine(mirrorTargetPos));
    }

    IEnumerator OpenDoorCoroutine()
    {
        yield return StartCoroutine(OpenDoorCoroutine(targetPos));
    }

    IEnumerator OpenDoorCoroutine(Vector3 customTargetPos)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.position = Vector3.Lerp(startPos, customTargetPos, t);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = customTargetPos;
        currentRoutine = null;
    }
}
