using System.Collections;
using UnityEngine;

public class RetractableGround : MonoBehaviour
{
    public float shrinkTime = 0.5f;
    public float stayTime = 1f;
    public float extendTime = 0.5f;

    private bool isRunning = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float originalWidth;
    private float leftX;
    private Collider2D groundCollider;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        groundCollider = GetComponent<Collider2D>();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        originalWidth = sr.bounds.size.x;
        leftX = originalPosition.x - originalWidth / 2f;

        SetGroundRatio(1f);
    }

    // Starts the shrink-and-extend animation if it is not already running.
    public void TriggerGround()
    {
        if (!isRunning)
        {
            StartCoroutine(ShrinkAndExtend());
        }
    }

    // Shrinks the platform briefly, waits, then restores its full width.
    IEnumerator ShrinkAndExtend()
    {
        isRunning = true;

        yield return ChangeRatio(1f, 0.9f, shrinkTime);
        yield return new WaitForSeconds(stayTime);
        yield return ChangeRatio(0.9f, 1f, extendTime);

        isRunning = false;
    }

    // Interpolates the visible platform width between two ratios.
    IEnumerator ChangeRatio(float from, float to, float time)
    {
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            float t = timer / time;

            float ratio = Mathf.Lerp(from, to, t);
            SetGroundRatio(ratio);
            ReleaseTrappedPlayers();

            yield return null;
        }

        SetGroundRatio(to);
        ReleaseTrappedPlayers();
    }

    // Resizes the platform while keeping its left edge fixed.
    void SetGroundRatio(float ratio)
    {
        transform.localScale = new Vector3(
            originalScale.x * ratio,
            originalScale.y,
            originalScale.z
        );

        float currentWidth = originalWidth * ratio;

        transform.position = new Vector3(
            leftX + currentWidth / 2f,
            originalPosition.y,
            originalPosition.z
        );
    }

    // Lets wedged players fall through instead of being pinned between retractable ground pieces.
    void ReleaseTrappedPlayers()
    {
        if (groundCollider == null)
            return;

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        for (int i = 0; i < players.Length; i++)
        {
            players[i].ReleaseIfEmbeddedInGround(groundCollider);
        }
    }
}
