using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public Transform spawnPoint;
    public Transform endPoint;

    // 五个拐点 - 可以在Inspector中拖拽设置，不需要的留空
    public Transform waypoint1;
    public Transform waypoint2;
    public Transform waypoint3;
    public Transform waypoint4;
    public Transform waypoint5;

    public float spawnInterval = 3f;
    public float carSpeed = 1f; // 车辆速度：每秒钟经过的格子数（Unity单位）

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCar();
            timer = 0f;
        }
    }

    void SpawnCar()
    {
        if (carPrefab == null || spawnPoint == null || endPoint == null)
        {
            Debug.LogWarning("CarSpawner：预制体或起点终点未设置！");
            return;
        }

        GameObject newCar = Instantiate(carPrefab, spawnPoint.position, Quaternion.identity);
        CarMover mover = newCar.GetComponent<CarMover>();
        if (mover != null)
        {
            // 检查是否有拐点，如果有则使用拐点路径
            Vector3[] pathPoints = BuildPathPoints();
            if (pathPoints.Length > 2)
            {
                // 有拐点的情况
                float totalDistance = CalculatePathDistance(pathPoints);
                float duration = totalDistance / carSpeed;
                mover.InitializePathWithWaypoints(pathPoints, duration);
            }
            else
            {
                // 没有拐点，直线移动
                float distance = Vector3.Distance(spawnPoint.position, endPoint.position);
                float duration = distance / carSpeed;
                mover.InitializePath(spawnPoint.position, endPoint.position, duration);
            }
        }
    }

    private Vector3[] BuildPathPoints()
    {
        System.Collections.Generic.List<Vector3> points = new System.Collections.Generic.List<Vector3>();

        // 添加起点
        points.Add(spawnPoint.position);

        // 添加非空的拐点
        if (waypoint1 != null) points.Add(waypoint1.position);
        if (waypoint2 != null) points.Add(waypoint2.position);
        if (waypoint3 != null) points.Add(waypoint3.position);
        if (waypoint4 != null) points.Add(waypoint4.position);
        if (waypoint5 != null) points.Add(waypoint5.position);

        // 添加终点
        points.Add(endPoint.position);

        return points.ToArray();
    }

    private float CalculatePathDistance(Vector3[] pathPoints)
    {
        float totalDistance = 0f;
        for (int i = 1; i < pathPoints.Length; i++)
        {
            totalDistance += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
        }
        return totalDistance;
    }
}