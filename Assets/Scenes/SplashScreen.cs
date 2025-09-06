using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [Header("画面设置")]
    public float displayTime = 2.0f;      // 图片显示时间
    public float fadeOutTime = 1.0f;      // 淡出时间

    [Header("组件引用")]
    public Image splashImage;             // 启动图片组件
    public CanvasGroup canvasGroup;       // 画布组组件

    [Header("后续场景")]
    public string nextSceneName = "MainMenu"; // 淡出后加载的场景名

    void Start()
    {
        // 确保有必要的组件引用
        if (splashImage == null)
            splashImage = GetComponent<Image>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // 设置初始透明度
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // 开始显示流程
        StartCoroutine(ShowSplash());
    }

    IEnumerator ShowSplash()
    {
        // 等待指定的显示时间
        yield return new WaitForSeconds(displayTime);

        // 淡出效果
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeOutTime));
            yield return null;
        }

        // 完全淡出后加载下一个场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // 如果没有指定下一个场景，只是禁用当前对象
            gameObject.SetActive(false);
        }
    }

    // 重新开始演示（用于编辑器测试）
    public void RestartSplash()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 1f;
        StartCoroutine(ShowSplash());
    }
}