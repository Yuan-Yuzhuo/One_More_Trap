using UnityEngine;

public class TriggerAreaMove : MonoBehaviour
{
    private GroundMoveRight groundMove;

    void Start()
    {
        groundMove = GetComponentInParent<GroundMoveRight>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            groundMove.StartMove();
        }
    }
}