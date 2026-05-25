using UnityEngine;

public class TriggerMoveSpike : MonoBehaviour
{
    public Transform spike;
    public float moveDistance = 2f;
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered && collision.CompareTag("Player"))
        {
            triggered = true;

            // 👉 固定向前移动一段距离（X轴）
            targetPosition = spike.position + new Vector3(moveDistance, 0, 0);
        }
    }

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
