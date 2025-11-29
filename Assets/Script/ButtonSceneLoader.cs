using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    // 在按钮的 OnClick 中传入场景名即可
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
