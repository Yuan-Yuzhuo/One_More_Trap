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

        if (door != null)
        {
            door.SetActive(false);
        }
    }

    // Smoothly animates the button press part toward its target position.
    private void Update()
    {
        pressPart.localPosition = Vector3.Lerp(
            pressPart.localPosition,
            targetPos,
            Time.deltaTime * speed
        );
    }

    // Presses the button once, starts linked spikes, and reveals the linked door.
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

        if (door != null)
        {
            door.SetActive(true);
        }
    }
}
