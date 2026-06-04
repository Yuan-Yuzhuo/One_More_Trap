using System.Collections;
using UnityEngine;

public class SpacecraftMove2D : MonoBehaviour
{
    public float moveDistance = 8f;

    public float moveSpeed = 3f;

    private bool moving = false;

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.right * moveDistance;
    }

    // Starts spacecraft movement and parents the player to the platform.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        if (!moving)
        {
            StartCoroutine(MoveRight());
        }

        collision.collider.transform.SetParent(transform);
    }

    // Moves the spacecraft to its final horizontal position.
    IEnumerator MoveRight()
    {
        moving = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;
    }
}
