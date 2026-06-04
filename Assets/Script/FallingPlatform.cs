using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    public float delay = 0.05f;
    public float destroyTime = 0.5f;

    public float fallSpeed = 12f;
    public float gravityScale = 85f;

    private bool isTriggered = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Starts the fall sequence when the player enters the platform trigger.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(Fall());
        }
    }

    // Gives the player a short warning shake, then turns the platform into a falling body.
    IEnumerator Fall()
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < 3; i++)
        {
            transform.position += Vector3.right * 0.03f;
            yield return new WaitForSeconds(0.01f);
            transform.position -= Vector3.right * 0.03f;
            yield return new WaitForSeconds(0.01f);
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(0f, -fallSpeed);

        Destroy(gameObject, destroyTime);
    }
}
