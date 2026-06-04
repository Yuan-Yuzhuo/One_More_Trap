using UnityEngine;

public class GroundShrinkTrigger : MonoBehaviour
{
    [Header("Shrink Speed")]
    public float shrinkSpeed = 2f;

    [Header("Minimum Width")]
    public float minWidth = 0f;

    private bool isShrinking = false;

    [Header("Next Ground")]
    public GroundShrinkTrigger nextGround;

    // Shrinks this platform over time once triggered.
    void Update()
    {
        if (isShrinking)
        {
            Vector3 scale = transform.localScale;
            scale.x -= shrinkSpeed * Time.deltaTime;

            if (scale.x <= minWidth)
            {
                scale.x = minWidth;
                Destroy(gameObject);
                return;
            }

            transform.localScale = scale;
        }
    }

    // Enables shrinking for this platform.
    public void StartShrink()
    {
        isShrinking = true;
    }

    // Starts the next platform shrinking when the player lands here.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (nextGround != null)
            {
                nextGround.StartShrink();
            }
        }
    }
}
