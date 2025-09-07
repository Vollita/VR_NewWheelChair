using UnityEngine;

public class CarMover : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;
    private float moveDuration = 5f;
    private float timer = 0f;
    private bool isMoving = false;
    private Renderer carRenderer;
    private float showAngleThreshold = 5f;

    // 拐点路径相关变量
    private Vector3[] pathPoints;
    private int currentTargetIndex = 0;
    private Vector3 currentStart;
    private Vector3 currentTarget;
    private float segmentDuration;
    private float segmentTimer = 0f;
    private bool isRotating = false;
    private float rotationSpeed = 360f; // 度/秒
    private Quaternion targetRotation;

    public void InitializePathWithWaypoints(Vector3[] waypoints, float duration)
    {
        pathPoints = waypoints;
        moveDuration = duration;

        // 计算每段的持续时间
        segmentDuration = duration / (pathPoints.Length - 1);

        timer = 0f;
        segmentTimer = 0f;
        currentTargetIndex = 1;
        currentStart = pathPoints[0];
        currentTarget = pathPoints[1];
        startPos = waypoints[0];
        endPos = waypoints[waypoints.Length - 1];
        isMoving = true;
        isRotating = false;

        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation((currentTarget - currentStart).normalized);

        carRenderer = GetComponentInChildren<Renderer>();
        if (carRenderer != null)
            carRenderer.enabled = true; // 总是显示车辆
    }

    public void InitializePath(Vector3 start, Vector3 end, float duration)
    {
        startPos = start;
        endPos = end;
        moveDuration = duration;
        timer = 0f;
        isMoving = true;
        pathPoints = null; // 清空拐点数组，表示直线移动

        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation((endPos - startPos).normalized);
        carRenderer = GetComponentInChildren<Renderer>();
        if (carRenderer != null)
            carRenderer.enabled = true; // 总是显示车辆
    }

    void Update()
    {
        if (!isMoving) return;
        timer += Time.deltaTime;

        // 如果使用拐点路径
        if (pathPoints != null && pathPoints.Length > 2)
        {
            UpdateWithWaypoints();
        }
        else
        {
            // 直线移动
            float progress = Mathf.Clamp01(timer / moveDuration);
            Vector3 pos = Vector3.Lerp(startPos, endPos, progress);
            pos.y = startPos.y;
            transform.position = pos;

            Vector3 moveDir = (endPos - startPos).normalized;
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void UpdateWithWaypoints()
    {
        if (isRotating)
        {
            // 旋转阶段
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 检查旋转是否完成
            if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
                segmentTimer = 0f; // 重置段计时器，开始移动
            }
        }
        else
        {
            // 移动阶段
            segmentTimer += Time.deltaTime;
            float segmentProgress = Mathf.Clamp01(segmentTimer / segmentDuration);

            // 在当前段内移动
            Vector3 pos = Vector3.Lerp(currentStart, currentTarget, segmentProgress);
            pos.y = startPos.y;
            transform.position = pos;

            // 检查是否到达当前目标点
            if (segmentProgress >= 1f)
            {
                // 到达当前目标点
                transform.position = currentTarget;

                // 检查是否还有下一个目标点
                if (currentTargetIndex < pathPoints.Length - 1)
                {
                    // 还有下一个点，开始旋转
                    currentTargetIndex++;
                    currentStart = currentTarget;
                    currentTarget = pathPoints[currentTargetIndex];

                    // 计算下一段的旋转目标
                    Vector3 nextDirection = (currentTarget - currentStart).normalized;
                    targetRotation = Quaternion.LookRotation(nextDirection);

                    // 开始旋转
                    isRotating = true;
                }
                else
                {
                    // 到达终点
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DeathHandler death = other.GetComponent<DeathHandler>();
            if (death != null)
                death.Die();
        }
    }
}