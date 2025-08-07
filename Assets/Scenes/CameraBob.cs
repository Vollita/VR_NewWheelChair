using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("λ���𶯲������ѷŴ�5����")]
    public float positionAmplitude = 5f;  // ԭ��0.1 �� 0.5
    public float positionFrequency = 10f;

    [Header("��ת�𶯲������ѷŴ�5����")]
    public float rotationAmplitude = 12.5f;  // ԭ��2.5 �� 12.5����λ���Ƕȣ�
    public float rotationFrequency = 12f;

    public Transform wheelchairTransform;
    public WheelchairController controller;

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    void Update()
    {
        if (wheelchairTransform == null || controller == null) return;

        if (controller.IsManuallyMoving())
        {
            float t = Time.time;

            // λ���𶯣�����+����
            float offsetY = (Mathf.PerlinNoise(0, t * positionFrequency) - 1f) * 2f;
            float offsetX = (Mathf.PerlinNoise(t * positionFrequency, 0) - 1f) * 2f;
            Vector3 positionOffset = new Vector3(offsetX, offsetY, 0f) * positionAmplitude;

            // ��ת�𶯣�����ƫͷ + ���µ�ͷ
            float angleX = (Mathf.PerlinNoise(t * rotationFrequency, 1.0f) - 1f) * 2f;
            float angleY = (Mathf.PerlinNoise(1.0f, t * rotationFrequency) - 1f) * 2f;
            Vector3 rotationOffset = new Vector3(angleX, angleY, 0f) * rotationAmplitude;

            // Ӧ����
            transform.localPosition = initialLocalPos + positionOffset;
            transform.localRotation = Quaternion.Euler(rotationOffset) * initialLocalRot;
        }
        else
        {
            // ֹͣʱƽ���ָ�
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPos, Time.deltaTime * 5f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, initialLocalRot, Time.deltaTime * 5f);
        }
    }
}
















