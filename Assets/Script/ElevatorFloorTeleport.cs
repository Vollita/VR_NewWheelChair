using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public float detectRadius = 1f;             // 探测半径
    public Transform player;                    // 手动指定玩家物体
    public Transform teleportTarget;            // 传送目的地

    private bool hasTeleported = false;

    void Update()
    {
        if (hasTeleported || player == null || teleportTarget == null)
            return;

        // 检测玩家是否在电梯地板正上方 1 米以内的范围
        Vector3 center = transform.position + Vector3.up * 1f;
        Collider[] hits = Physics.OverlapSphere(center, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.transform == player)
            {
                TeleportPlayer();
                break;
            }
        }
    }

    void TeleportPlayer()
    {
        player.position = teleportTarget.position;
        hasTeleported = true;
        Debug.Log("玩家被传送");
    }

    void OnDrawGizmosSelected()
    {
        // 仅在Scene视图中显示检测范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, detectRadius);
    }
}
