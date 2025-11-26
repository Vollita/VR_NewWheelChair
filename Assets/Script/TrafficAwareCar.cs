using UnityEngine;

public class TrafficAwareCar : MonoBehaviour
{
    public static TrafficLightController globalTrafficLight; // 全局静态引用

    // 移动参数
    private Vector3 startPos;
    private Vector3 endPos;
    private float normalSpeed = 10f;
    private bool isMoving = false;
    private Renderer carRenderer;

    public void InitializePath(Vector3 start, Vector3 end, float duration = 5f)
    {
        startPos = start;
        endPos = end;
        isMoving = true;

        float distance = Vector3.Distance(start, end);
        normalSpeed = distance / duration;

        transform.position = startPos;
        Vector3 direction = (endPos - startPos).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        carRenderer = GetComponentInChildren<Renderer>();
        if (carRenderer != null)
        {
            carRenderer.enabled = true;
        }
    }

    void Update()
    {
        if (!isMoving) return;

        MoveCar();
        UpdateRenderer();
        CheckEnd();
    }

    void MoveCar()
    {
        Vector3 direction = (endPos - transform.position).normalized;
        if (direction == Vector3.zero) return;

        float currentSpeed = GetCurrentSpeed();

        // 移动
        if (currentSpeed > 0)
        {
            transform.position += direction * currentSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    float GetCurrentSpeed()
    {
        if (globalTrafficLight == null)
        {
            return normalSpeed; // 没有红绿灯就正常走
        }

        // 检查红灯
        if (IsRedLight())
        {
            return 0f; // 红灯完全停止
        }
        // 检查黄灯
        //else if (IsYellowLight())
        //{
        //    return normalSpeed * 0.5f; // 黄灯一半速度
        //}
        // 绿灯或其他情况
        else
        {
            return normalSpeed; // 正常速度
        }
    }

    bool IsRedLight()
    {
        return globalTrafficLight != null &&
               globalTrafficLight.redBox != null &&
               globalTrafficLight.matRed != null &&
               globalTrafficLight.redBox.material != null &&
               globalTrafficLight.redBox.material.name.Contains(globalTrafficLight.matRed.name);
    }

    //bool IsYellowLight()
    //{
    //    return globalTrafficLight != null &&
    //           globalTrafficLight.yellowBox != null &&
    //           globalTrafficLight.matYellow != null &&
    //           globalTrafficLight.yellowBox.material != null &&
    //           globalTrafficLight.yellowBox.material.name.Contains(globalTrafficLight.matYellow.name);
    //}

    void UpdateRenderer()
    {
        if (carRenderer != null)
        {
            Vector3 moveDir = (endPos - transform.position).normalized;
            if (moveDir != Vector3.zero)
            {
                float angle = Vector3.Angle(transform.forward, moveDir);
                carRenderer.enabled = angle < 45f;
            }
            else
            {
                carRenderer.enabled = true;
            }
        }
    }

    void CheckEnd()
    {
        float distanceToEnd = Vector3.Distance(transform.position, endPos);
        if (distanceToEnd < 0.5f)
        {
            Destroy(gameObject);
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