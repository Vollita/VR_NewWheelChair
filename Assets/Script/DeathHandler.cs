using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    private bool isDead = false;

    public GameObject deathUI;               // Inspector�й�����UI
    public string[] deathTags = { "Car", "Door" }; // �����ö����������Tag

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

        // �ɼӣ�������ƣ������ƶ����߼�

        Invoke(nameof(RestartScene), 3f); // 3�������
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
