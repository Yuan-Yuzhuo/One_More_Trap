using UnityEngine;
using System.Collections;

public class DropTrap : MonoBehaviour
{
    public Rigidbody2D spikeRb;   // 掉落物
    public float delay = 0.2f;    // 触发延迟
    public float fallSpeed = 15f; // 初始下落速度
    public float gravityScale = 8f;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(Drop());
        }
    }

    IEnumerator Drop()
    {
        // 预警（可以加抖动）
        yield return new WaitForSeconds(delay);

        // 开始掉落
        spikeRb.bodyType = RigidbodyType2D.Dynamic;
        spikeRb.gravityScale = gravityScale;

        // 给一个向下初速度（关键）
        spikeRb.velocity = new Vector2(0f, -fallSpeed);
    }
}