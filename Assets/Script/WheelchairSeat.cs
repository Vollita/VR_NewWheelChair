using UnityEngine;

/// <summary>
/// �������棺�������˸߶ȡ�ͷ��������������бЧ����
/// �� XR ����㼶���ڴ˶����·���ʹ����Ч��Ӱ����������ӽǡ�
/// </summary>
public class WheelchairSeat : MonoBehaviour
{
    [Header("Seat Settings")]
    [Tooltip("��������߶ȣ���������θ�����ľֲ� Y ƫ�ƣ���")]
    public float seatHeight = 0.6f;

    [Header("Head Bob Settings")]
    public float headBobAmount = 0.05f;
    public float headBobSpeed = 2f;

    [Header("Slope Simulation")]
    [Tooltip("��ǰ�¶ȽǶȣ���λΪ�ȣ���ֵΪ���£������б����")]
    public float slopeAngle = 0f; // -10 ~ +10 degrees typical range
    public float maxTiltAngle = 10f; // �����б��
    public float tiltSmoothing = 2f;
    public float slopeOffsetZ = 0.1f; // ����ʱ������󻬵ľ���

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float headBobTimer = 0f;

    void Awake()
    {
        baseLocalPosition = transform.localPosition;
        baseLocalPosition.y = seatHeight;
        baseLocalRotation = transform.localRotation;

        transform.localPosition = baseLocalPosition;
    }

    void Update()
    {
        ApplySlopeTilt();
    }

    /// <summary>
    /// Ӧ��ͷ���������н�ʱ���ã�
    /// </summary>
    public void ApplyHeadBob(float inputMagnitude)
    {
        if (inputMagnitude > 0.1f)
        {
            headBobTimer += Time.deltaTime * headBobSpeed;
            float bobY = Mathf.Sin(headBobTimer) * headBobAmount * inputMagnitude;

            transform.localPosition = new Vector3(
                baseLocalPosition.x,
                seatHeight + bobY,
                baseLocalPosition.z
            );
        }
        else
        {
            ResetSeatPosition();
        }
    }

    /// <summary>
    /// ���ö���״̬
    /// </summary>
    public void ResetSeatPosition()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            new Vector3(baseLocalPosition.x, seatHeight, baseLocalPosition.z),
            Time.deltaTime * headBobSpeed
        );
        headBobTimer = 0f;
    }

    /// <summary>
    /// �������θ߶ȣ��ⲿ�ɵ��ã�
    /// </summary>
    public void SetSeatHeight(float newHeight)
    {
        seatHeight = newHeight;
        baseLocalPosition.y = seatHeight;
    }

    /// <summary>
    /// Ӧ��������бģ��
    /// </summary>
    private void ApplySlopeTilt()
    {
        // Clamp �¶�
        float clampedSlope = Mathf.Clamp(slopeAngle, -maxTiltAngle, maxTiltAngle);

        // ����Ŀ����ת���� X �����ǣ�
        Quaternion targetRotation = Quaternion.Euler(clampedSlope, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * tiltSmoothing);

        // ����ʱ��΢���ƫ�ƣ�ģ��������
        float zOffset = Mathf.Sin(clampedSlope * Mathf.Deg2Rad) * slopeOffsetZ;
        Vector3 offsetPosition = baseLocalPosition + new Vector3(0f, 0f, -zOffset);
        transform.localPosition = Vector3.Lerp(transform.localPosition, offsetPosition, Time.deltaTime * tiltSmoothing);
    }
}