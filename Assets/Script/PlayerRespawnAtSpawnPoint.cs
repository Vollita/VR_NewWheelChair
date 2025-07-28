using System.Collections;
using UnityEngine;

public class PlayerRespawnAtSpawnPoint : MonoBehaviour
{
    public float delayFrames = 3f;  // 延迟几帧再移动，默认3帧

    void Start()
    {
        StartCoroutine(MoveToSpawnPointAfterDelay());
    }

    private IEnumerator MoveToSpawnPointAfterDelay()
    {
        // 等 delayFrames 帧，确保场景和对象都加载完成
        for (int i = 0; i < delayFrames; i++)
        {
            yield return null;
        }

        string spawnName = PlayerPrefs.GetString("SpawnPoint", "");
        if (string.IsNullOrEmpty(spawnName))
        {
            Debug.LogWarning("PlayerRespawnAtSpawnPoint: SpawnPoint name is empty.");
            yield break;
        }

        GameObject spawnPoint = GameObject.Find(spawnName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"PlayerRespawnAtSpawnPoint: Spawn point '{spawnName}' not found.");
            yield break;
        }

        // 设置玩家位置和旋转
        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;

        Debug.Log($"PlayerRespawnAtSpawnPoint: Moved player to spawn point '{spawnName}'.");
    }
}
