using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class VRUIButtonLoader : MonoBehaviour
{
    public string targetSceneName = "Scene2";

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        // ¼àÌý°´Å¥µã»÷ÊÂ¼þ
        interactable.selectEntered.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked(SelectEnterEventArgs args)
    {
        Debug.Log("UI Button Clicked! Loading scene: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    private void OnDestroy()
    {
        // ÒÆ³ý¼àÌý£¬·ÀÖ¹ÄÚ´æÐ¹Â©
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnButtonClicked);
        }
    }
}