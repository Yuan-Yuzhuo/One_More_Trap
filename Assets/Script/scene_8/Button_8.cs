using UnityEngine;

public class FloorButton2D : MonoBehaviour
{
    public Transform pressPart;

    public float pressDepth = 0.1f;

    public float speed = 8f;

    public SpikeSequence2D spikeSequence;

    public GameObject door;

    private Vector3 startPos;
    private Vector3 targetPos;

    private bool pressed = false;

    private void Start()
    {
        startPos = pressPart.localPosition;
        targetPos = startPos;

        // 一开始门隐藏
        if (door != null)
        {
            door.SetActive(false);
        }
    }

    private void Update()
    {
        pressPart.localPosition = Vector3.Lerp(
            pressPart.localPosition,
            targetPos,
            Time.deltaTime * speed
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (pressed) return;

        pressed = true;

        targetPos = startPos + Vector3.down * pressDepth;

        if (spikeSequence != null)
        {
            spikeSequence.StartSequence();
        }

        // 门出现
        if (door != null)
        {
            door.SetActive(true);
        }
    }
}