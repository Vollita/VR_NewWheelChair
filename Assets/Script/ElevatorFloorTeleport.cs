using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public float detectRadius = 1f;             // ̽��뾶
    public Transform player;                    // �ֶ�ָ���������
    public Transform teleportTarget;            // ����Ŀ�ĵ�

    private bool hasTeleported = false;

    void Update()
    {
        if (hasTeleported || player == null || teleportTarget == null)
            return;

        // �������Ƿ��ڵ��ݵذ����Ϸ� 1 �����ڵķ�Χ
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
        Debug.Log("��ұ�����");
    }

    void OnDrawGizmosSelected()
    {
        // ����Scene��ͼ����ʾ��ⷶΧ
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, detectRadius);
    }
}
