using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public string spawnPointName;

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered portal trigger.");

            isLoading = true;
            DontDestroyOnLoad(other.gameObject);
            PlayerPrefs.SetString("SpawnPoint", spawnPointName);
            // 使用静态事件代理包装，避免销毁异常
            SceneManager.sceneLoaded += OnSceneLoadedSafe;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // 包装的安全调用
    private void OnSceneLoadedSafe(Scene scene, LoadSceneMode mode)
    {
        try
        {
            OnSceneLoaded(scene, mode);
        }
        finally
        {
            // 安全取消订阅，避免空引用异常
            SceneManager.sceneLoaded -= OnSceneLoadedSafe;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded. Looking for spawn point {spawnPointName}");

        string spawnName = PlayerPrefs.GetString("SpawnPoint", "");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (!string.IsNullOrEmpty(spawnName) && player != null)
        {
            GameObject spawnPoint = null;
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (GameObject rootObj in scene.GetRootGameObjects())
                {
                    if (rootObj.name == spawnName)
                    {
                        spawnPoint = rootObj;
                        break;
                    }
                }
            }

            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;

                Transform cam = player.transform.Find("Main Camera");
                if (cam != null)
                    cam.localRotation = Quaternion.identity;

                Debug.Log("Player teleported to spawn point.");
            }
            else
            {
                Debug.LogWarning($"Spawn point '{spawnName}' not found in scene.");
            }
        }
        else
        {
            Debug.LogWarning("Player or spawn point name invalid.");
        }

        isLoading = false;
    }
}
