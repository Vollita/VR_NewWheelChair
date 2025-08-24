using UnityEngine;

public class CarSystem : MonoBehaviour
{
    [Header("=== 车辆生成设置 ===")]
    public GameObject carPrefab;
    public Transform spawnPoint;
    public Transform endPoint;
    public float spawnInterval = 3f;

    [Header("=== 红绿灯系统 ===")]
    public TrafficLightController trafficLight;

    [Header("=== 车辆参数 ===")]
    public float carMoveDuration = 5f;

    private float spawnTimer = 0f;

    void Start()
    {
        // 设置全局红绿灯引用
        if (trafficLight != null)
        {
            TrafficAwareCar.globalTrafficLight = trafficLight;
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            // 只有在不是红灯时才生成车辆
            if (!IsRedLight())
            {
                SpawnCar();
            }
            else
            {
                Debug.Log("红灯亮起 - 暂停生成车辆");
            }
            spawnTimer = 0f;
        }
    }

    bool IsRedLight()
    {
        if (trafficLight == null) return false;

        return trafficLight.redBox != null &&
               trafficLight.matRed != null &&
               trafficLight.redBox.material != null &&
               trafficLight.redBox.material.name.Contains(trafficLight.matRed.name);
    }

    void SpawnCar()
    {
        if (carPrefab == null || spawnPoint == null || endPoint == null)
        {
            Debug.LogError("CarSystem: 缺少必要组件！");
            return;
        }

        GameObject newCar = Instantiate(carPrefab, spawnPoint.position, Quaternion.identity);

        TrafficAwareCar carScript = newCar.GetComponent<TrafficAwareCar>();
        if (carScript == null)
        {
            carScript = newCar.AddComponent<TrafficAwareCar>();
        }

        carScript.InitializePath(spawnPoint.position, endPoint.position, carMoveDuration);

        Debug.Log("生成新车辆");
    }

    [ContextMenu("测试生成车辆")]
    void TestSpawnCar()
    {
        SpawnCar();
    }
}