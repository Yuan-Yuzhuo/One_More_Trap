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

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        originalWidth = sr.bounds.size.x;

        // 记录地板左边的位置
        leftX = originalPosition.x - originalWidth / 2f;

        // 一开始保持完整显示
        SetGroundRatio(1f);
    }

    public void TriggerGround()
    {
        if (!isRunning)
        {
            StartCoroutine(ShrinkAndExtend());
        }
    }

    IEnumerator ShrinkAndExtend()
    {
        isRunning = true;

        // 缩回到 1/5
        yield return ChangeRatio(1f, 0.9f, shrinkTime);

        yield return new WaitForSeconds(stayTime);

        // 再伸回完整
        yield return ChangeRatio(0.9f, 1f, extendTime);

        isRunning = false;
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
        // 只改变 X 方向，Y 保持原来的大小
        transform.localScale = new Vector3(
            originalScale.x * ratio,
            originalScale.y,
            originalScale.z
        );

        // 左边固定，右边缩回/伸出
        float currentWidth = originalWidth * ratio;

        transform.position = new Vector3(
            leftX + currentWidth / 2f,
            originalPosition.y,
            originalPosition.z
        );
    }
}