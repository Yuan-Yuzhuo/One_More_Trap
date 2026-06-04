using UnityEngine;
using System.Collections;

public class DropTrap : MonoBehaviour
{
    public Rigidbody2D spikeRb;
    public float delay = 0.2f;
    public float fallSpeed = 15f;
    public float gravityScale = 8f;

    private bool isTriggered = false;

    // Starts the trap drop when the player enters the trigger.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(Drop());
        }
    }

    // Delays briefly, then turns the spike into a falling rigidbody.
    IEnumerator Drop()
    {
        yield return new WaitForSeconds(delay);

        spikeRb.bodyType = RigidbodyType2D.Dynamic;
        spikeRb.gravityScale = gravityScale;
        spikeRb.velocity = new Vector2(0f, -fallSpeed);
    }
}
