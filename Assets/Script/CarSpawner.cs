using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public Transform spawnPoint;
    public Transform endPoint;

    public float spawnInterval = 3f;
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
            mover.InitializePath(spawnPoint.position, endPoint.position);
    }
}
