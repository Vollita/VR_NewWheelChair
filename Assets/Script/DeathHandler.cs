using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    private bool isDead = false;

    public GameObject deathUI;       // Inspector中挂死亡UI
    public string targetTag = "Car"; // Inspector中可配置撞到什么Tag就判定死亡

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDead && collision.gameObject.CompareTag(targetTag))
        {
            Die();
        }
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
