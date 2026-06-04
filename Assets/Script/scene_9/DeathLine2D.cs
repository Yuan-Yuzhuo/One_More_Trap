using UnityEngine;

public class DeathLine2D : MonoBehaviour
{
    // Kills the player when they enter the death trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}
