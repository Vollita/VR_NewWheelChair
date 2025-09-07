using UnityEngine;
using System.Collections;

public class TemporaryUI : MonoBehaviour
{
    [Header("显示设置")]
    [SerializeField] private float displayTime = 3f;    // UI显示的时间（秒）
    [SerializeField] private float fadeTime = 0.5f;     // 淡入淡出时间（秒）

    [Header("组件引用")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool disableOnHide = true; // 是否完全禁用对象

    private void OnValidate()
    {
        // 在编辑器中自动获取CanvasGroup组件
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // 确保有CanvasGroup组件
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                Debug.LogWarning("自动添加了CanvasGroup组件到 " + gameObject.name);
            }
        }

        // 启动显示流程
        StartCoroutine(ShowAndHideRoutine());
    }

    IEnumerator ShowAndHideRoutine()
    {
        // 可选：淡入效果（如果需要）
        yield return StartCoroutine(FadeIn());

        // 等待显示时间
        yield return new WaitForSeconds(displayTime);

        // 淡出效果
        yield return StartCoroutine(FadeOut());

        // 淡出完成后禁用对象
        if (disableOnHide)
            gameObject.SetActive(false);
    }

    IEnumerator FadeIn()
    {
        if (fadeTime <= 0)
        {
            canvasGroup.alpha = 1f;
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        if (fadeTime <= 0)
        {
            canvasGroup.alpha = 0f;
            yield break;
        }

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    // 公共方法：可以从其他脚本调用此方法来手动触发显示
    public void ShowTemporarily()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ShowAndHideRoutine());
    }

    // 公共方法：立即隐藏UI
    public void HideImmediately()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0f;
        if (disableOnHide)
            gameObject.SetActive(false);
    }
}