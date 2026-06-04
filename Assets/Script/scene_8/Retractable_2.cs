using System.Collections;
using UnityEngine;

public class Retractable_2 : MonoBehaviour
{
    public float shrinkTime = 0.5f;
    public float targetRatio = 0.8f;

    private bool isRunning = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float originalWidth;
    private float leftX;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        originalWidth = sr.bounds.size.x;
        leftX = originalPosition.x - originalWidth / 2f;

        SetGroundRatio(1f);
    }

    // Starts the one-way shrink animation if it is not already running.
    public void TriggerGround()
    {
        if (!isRunning)
        {
            StartCoroutine(ShrinkOnly());
        }
    }

    // Shrinks the platform to the configured final width.
    IEnumerator ShrinkOnly()
    {
        isRunning = true;
        yield return ChangeRatio(1f, targetRatio, shrinkTime);
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

            yield return null;
        }

        SetGroundRatio(to);
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
}
