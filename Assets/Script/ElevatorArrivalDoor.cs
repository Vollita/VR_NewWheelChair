using UnityEngine;
using System.Collections;

public class ElevatorArrivalDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform otherDoor;          // 对面那扇门
    public float openDuration = 1f;      // 开门动画时间
    public float targetThickness = 0.2f; // 开门后厚度（棍状）
    public Vector3 shrinkAxis = Vector3.right; // 缩放方向（通常是 Vector3.right）

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

        // 计算缩放后的尺寸
        targetScale = originalScale;
        if (shrinkAxis == Vector3.right)
            targetScale.x = targetThickness;
        else if (shrinkAxis == Vector3.up)
            targetScale.y = targetThickness;
        else if (shrinkAxis == Vector3.forward)
            targetScale.z = targetThickness;

        // 修正收缩方向：这里反向偏移，和之前代码相反
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
    /// 外部调用：开两扇门（只执行一次）
    /// </summary>
    public void OpenDoor()
    {
        if (hasOpened) return;
        hasOpened = true;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(OpenDoorCoroutine());

        // 让另一扇门也开（反方向）
        if (otherDoor != null)
        {
            ElevatorArrivalDoor otherScript = otherDoor.GetComponent<ElevatorArrivalDoor>();
            if (otherScript != null)
                otherScript.OpenDoorMirror();
        }
    }

    /// <summary>
    /// 对门调用的开门（反向收缩）
    /// </summary>
    private void OpenDoorMirror()
    {
        if (hasOpened) return;
        hasOpened = true;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // 修正收缩方向：这里反向偏移，和之前代码相反
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
