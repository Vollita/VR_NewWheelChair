using UnityEngine;

public class StepResistance : MonoBehaviour
{
    [Header("��������")]
    public float climbSpeed = 3.0f;               // Y�����ٶ�
    public float movementDrag = 0.4f;             // �ƶ�������0.4 = ������40%��

    [Header("����")]
    public bool showDebugInfo = true;

    private Transform wheelchairPlayer;
    private Rigidbody wheelchairRb;
    private float stepTopY;                       // ̨�׶���Y����

    void Start()
    {
        // ����̨�׶���Y����
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            stepTopY = transform.position.y + col.size.y / 2;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.name == "WheelChair_Player")
        {
            wheelchairPlayer = collision.transform;
            wheelchairRb = collision.rigidbody;

            // ����Ƿ�����ǰ�ƶ�
            if (wheelchairRb.velocity.z > 0.1f)
            {
                // ������λ�û��̨�׶�������̧��Y
                if (wheelchairPlayer.position.y < stepTopY)
                {
                    Vector3 newPos = wheelchairPlayer.position;
                    newPos.y += climbSpeed * Time.deltaTime;
                    wheelchairPlayer.position = newPos;

                    // ����ƶ�����
                    Vector3 velocity = wheelchairRb.velocity;
                    velocity.x *= movementDrag;
                    velocity.z *= movementDrag;
                    wheelchairRb.velocity = velocity;

                    if (showDebugInfo)
                    {
                        Debug.Log($"���������У���ǰY: {wheelchairPlayer.position.y:F2}, Ŀ��Y: {stepTopY:F2}");
                    }
                }
            }
        }
    }
}