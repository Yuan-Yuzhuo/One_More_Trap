using UnityEngine;

public class GroundShrinkTrigger : MonoBehaviour
{
    [Header("缩小速度")]
    public float shrinkSpeed = 2f;

    [Header("最小宽度")]
    public float minWidth = 0f;

    private bool isShrinking = false;

    [Header("下一个Ground")]
    public GroundShrinkTrigger nextGround;

    void Update()
    {
        if (isShrinking)
        {
            Vector3 scale = transform.localScale;

            // 缩小 X 宽度
            scale.x -= shrinkSpeed * Time.deltaTime;

            // 防止变成负数
            if (scale.x <= minWidth)
            {
                scale.x = minWidth;

                // 销毁自己
                Destroy(gameObject);

                return;
            }

            transform.localScale = scale;
        }
    }

    public void StartShrink()
    {
        isShrinking = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 让下一个平台开始缩小
            if (nextGround != null)
            {
                nextGround.StartShrink();
            }
        }
    }
}