using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Enter the name of the target scene to load")]
    public string targetSceneName = "UI2";

    [Header("Debug Options")]
    public bool enableLogs = true;
    public bool enableMouseTesting = true;

    private void Start()
    {
        if (enableLogs)
            Debug.Log($"SceneLoader initialized on: {gameObject.name}");

        // 确保对象有碰撞体
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            if (enableLogs)
                Debug.Log("Added BoxCollider to object");
        }
    }

    // 鼠标点击检测方法
    private void OnMouseDown()
    {
        if (!enableMouseTesting) return;
        if (!Application.isEditor) return;

        if (enableLogs)
            Debug.Log("Mouse click detected on: " + gameObject.name);

        LoadTargetScene();
    }

    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set!");
            return;
        }

        if (!SceneExistsInBuildSettings(targetSceneName))
        {
            Debug.LogError($"Scene '{targetSceneName}' is not in build settings!");
            return;
        }

        if (enableLogs)
            Debug.Log($"Loading scene: {targetSceneName}");

        SceneManager.LoadScene(targetSceneName);
    }

    private bool SceneExistsInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string scene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (scene.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}