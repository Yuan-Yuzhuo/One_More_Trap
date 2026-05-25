using UnityEngine;

public class GroundMoveRight : MonoBehaviour
{
    public float moveSpeed = 2f;

    // 向右移动多少距离
    public float moveDistance = 5f;

    private bool isMoving = false;

    private float startX;
    private float targetX;

    void Start()
    {
        startX = transform.position.x;
        targetX = startX + moveDistance;
    }

    void Update()
    {
        if (!isMoving) return;

        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        // 到达目标位置停止
        if (transform.position.x >= targetX)
        {
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;

            isMoving = false;
        }
    }

    public void StartMove()
    {
        isMoving = true;
    }
}