using UnityEngine;

public class GroundMoveRight : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 5f;

    private bool isMoving = false;

    private float startX;
    private float targetX;

    void Start()
    {
        startX = transform.position.x;
        targetX = startX + moveDistance;
    }

    // Moves the platform right until it reaches its configured target.
    void Update()
    {
        if (!isMoving) return;

        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        if (transform.position.x >= targetX)
        {
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;

            isMoving = false;
        }
    }

    // Starts the platform movement.
    public void StartMove()
    {
        isMoving = true;
    }
}
