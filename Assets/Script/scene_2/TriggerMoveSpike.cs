using UnityEngine;

public class TriggerMoveSpike : MonoBehaviour
{
    public Transform spike;
    public float moveDistance = 2f;
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool triggered = false;

    // Sets the spike target position when the player enters the trigger.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered && collision.CompareTag("Player"))
        {
            triggered = true;
            targetPosition = spike.position + new Vector3(moveDistance, 0, 0);
        }
    }

    // Moves the spike toward its triggered target position.
    void Update()
    {
        if (triggered)
        {
            spike.position = Vector3.MoveTowards(
                spike.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }
}
