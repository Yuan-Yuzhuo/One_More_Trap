using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathLine2D : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 重开当前场景
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
