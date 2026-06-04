using UnityEngine;

public class SpikeKill : MonoBehaviour
{
    // Sends a death message to the player when they touch the spike trigger.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.SendMessage("Die");
        }
    }
}
