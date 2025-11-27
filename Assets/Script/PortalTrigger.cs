using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public string spawnPointName;
    [SerializeField]
    private bool isLoading = false;
    [SerializeField]
    private static bool isPlayerTeleporting = false;

    void OnEnable()
    {
        // 确保传送点cube设置为触发器
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 防止重复触发和确保只有玩家能触发
        //if (isLoading || isPlayerTeleporting || !other.CompareTag("Player"))
        //    return;

        // 确保场景名称和生成点名称都已设置
        if (string.IsNullOrEmpty(sceneToLoad) || string.IsNullOrEmpty(spawnPointName))
        {
            Debug.LogError("场景名称或生成点名称未设置！");
            return;
        }

        Debug.Log($"玩家接触传送点，准备传送到场景: {sceneToLoad}，生成点: {spawnPointName}");
        StartCoroutine(TeleportPlayer());
    }

    IEnumerator TeleportPlayer()
    {
        isLoading = true;
        isPlayerTeleporting = true;

        // 获取玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("找不到玩家对象！");
            ResetTeleportState();
            yield break;
        }

        // 保存生成点信息
        PlayerPrefs.SetString("SpawnPoint", spawnPointName);
        PlayerPrefs.Save();

        // 注册场景加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 取消事件订阅
        SceneManager.sceneLoaded -= OnSceneLoaded;

        StartCoroutine(PositionPlayerAfterLoad());
    }

    IEnumerator PositionPlayerAfterLoad()
    {
        // 等待一帧，确保所有对象都已初始化
        yield return null;
        Debug.Log("Lightmaps = " + LightmapSettings.lightmaps.Length);
        string spawnName = PlayerPrefs.GetString("SpawnPoint", "");

        Debug.Log($"场景加载完成，寻找生成点: {spawnName}");

        // 查找玩家对象
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("在新场景中找不到玩家对象！");
            ResetTeleportState();
            yield break;
        }

        // 查找生成点
        GameObject spawnPoint = GameObject.Find(spawnName);
        if (spawnPoint == null)
        {
            Debug.LogError($"找不到生成点: {spawnName}");
            ResetTeleportState();
            yield break;
        }

        // 计算传送位置（cube上方一格）
        Vector3 targetPosition = CalculateSpawnPosition(spawnPoint);

        // 设置玩家位置和旋转
        player.transform.position = targetPosition;
        player.transform.rotation = spawnPoint.transform.rotation;

        // 🔥 VR修复：完全不操作相机，让VR系统自己处理
        // 注释掉原来的相机重置代码，这是导致VR渲染昏暗的原因：
        // Transform cam = player.transform.Find("Main Camera");
        // if (cam != null)
        //     cam.localRotation = Quaternion.identity;

        Debug.Log($"玩家成功传送到: {targetPosition}");

        // 清理PlayerPrefs
        PlayerPrefs.DeleteKey("SpawnPoint");

        ResetTeleportState();
    }

    Vector3 CalculateSpawnPosition(GameObject spawnPoint)
    {
        Vector3 spawnPosition = spawnPoint.transform.position;

        // 获取cube的边界
        Renderer cubeRenderer = spawnPoint.GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            // 将玩家放置在cube顶部上方一点点
            float cubeHeight = cubeRenderer.bounds.size.y;
            spawnPosition.y += (cubeHeight / 2) + 0.1f; // cube中心到顶部 + 一点缓冲
        }
        else
        {
            // 如果没有渲染器，默认上移1个单位
            spawnPosition.y += 1f;
        }

        return spawnPosition;
    }

    void ResetTeleportState()
    {
        isLoading = false;
        isPlayerTeleporting = false;
    }

    // 在对象销毁时清理事件订阅，防止内存泄漏
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}