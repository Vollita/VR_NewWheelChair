using System.Collections;
using UnityEngine;

public class PlayerRespawnAtSpawnPoint : MonoBehaviour
{
    public float delayFrames = 3f;  // �ӳټ�֡���ƶ���Ĭ��3֡

    void Start()
    {
        StartCoroutine(MoveToSpawnPointAfterDelay());
    }

    private IEnumerator MoveToSpawnPointAfterDelay()
    {
        // �� delayFrames ֡��ȷ�������Ͷ��󶼼������
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

        // �������λ�ú���ת
        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;

        Debug.Log($"PlayerRespawnAtSpawnPoint: Moved player to spawn point '{spawnName}'.");
    }
}
