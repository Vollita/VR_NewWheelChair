using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene1Loader : MonoBehaviour
{
    // 通过场景名称跳转
    public void LoadSceneByName(string sceneName)
    {
        // 检查场景是否存在
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"场景 '{sceneName}' 不存在！请检查构建设置。");
        }
    }
}