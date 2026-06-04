using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    private RetractableGround ground;

    void Start()
    {
        ground = GetComponentInParent<RetractableGround>();

        if (ground == null)
        {
            Debug.LogError("No RetractableGround script found on parent object!");
        }
    }

    // Triggers the parent retractable ground when the player enters this trigger.
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
