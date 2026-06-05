using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelDoor : MonoBehaviour
{
    public int nextSceneIndex;

    // Sends the player to the configured scene, or to the next build scene by default.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            int targetSceneIndex = nextSceneIndex;

            if (targetSceneIndex <= 0)
            {
                targetSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            }

            if (targetSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                GameStatsTracker.CompleteChallenge();
                return;
            }

            SceneTransitionController.LoadSceneFrozen(targetSceneIndex);
        }
    }
}
