using UnityEngine;

public class TriggerAreaMove : MonoBehaviour
{
    private GroundMoveRight groundMove;

    void Start()
    {
        groundMove = GetComponentInParent<GroundMoveRight>();
    }

    // Starts the parent platform movement when the player enters this trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            groundMove.StartMove();
        }
    }
}
