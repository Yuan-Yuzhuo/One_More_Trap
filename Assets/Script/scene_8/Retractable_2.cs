using System.Collections;
using UnityEngine;

public class Retractable_2 : MonoBehaviour
{
    public float shrinkTime = 0.5f;

    // 缩到剩多少
    // 0.8 = 保留 80%
    // 0.2 = 保留 20%
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

        // 记录地板左边位置
        leftX = originalPosition.x - originalWidth / 2f;

        // 初始完整显示
        SetGroundRatio(1f);
    }

    public void TriggerGround()
    {
        if (!isRunning)
        {
            StartCoroutine(ShrinkOnly());
        }
    }

    IEnumerator ShrinkOnly()
    {
        isRunning = true;

        // 从右往左缩
        yield return ChangeRatio(1f, targetRatio, shrinkTime);
    }

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

    void SetGroundRatio(float ratio)
    {
        // 缩放 X
        transform.localScale = new Vector3(
            originalScale.x * ratio,
            originalScale.y,
            originalScale.z
        );

        // 保持左边固定
        float currentWidth = originalWidth * ratio;

        transform.position = new Vector3(
            leftX + currentWidth / 2f,
            originalPosition.y,
            originalPosition.z
        );
    }
}