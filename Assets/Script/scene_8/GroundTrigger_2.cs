using UnityEngine;

public class GroundTrigger_2 : MonoBehaviour
{
    private Retractable_2 ground;

    void Start()
    {
        ground = GetComponentInParent<Retractable_2>();

        if (ground == null)
        {
            Debug.LogError("No Retractable_2 script found on parent object!");
        }
    }

    // Triggers the parent one-way retractable ground when the player enters this trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ground != null)
            {
                ground.TriggerGround();
            }
        }
    }
}
