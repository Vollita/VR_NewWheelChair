using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    private bool isDead = false;

    public GameObject deathUI;               // Inspector中挂死亡UI
    public string[] deathTags = { "Car", "Door" }; // 可配置多个死亡触发Tag

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDead && IsDeadlyTag(collision.gameObject.tag))
        {
            Die();
        }
    }

    bool IsDeadlyTag(string tag)
    {
        foreach (string t in deathTags)
        {
            if (tag == t)
                return true;
        }
        return false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player died!");

        if (deathUI != null)
            deathUI.SetActive(true);

        // 可加：冻结控制，禁用移动等逻辑

        Invoke(nameof(RestartScene), 3f); // 3秒后重启
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
