using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    private bool isDead = false;

    public GameObject deathUI;       // Inspector�й�����UI
    public string targetTag = "Car"; // Inspector�п�����ײ��ʲôTag���ж�����

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

        // �ɼӣ�������ƣ������ƶ����߼�

        Invoke(nameof(RestartScene), 3f); // 3�������
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
