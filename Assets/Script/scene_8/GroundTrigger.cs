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