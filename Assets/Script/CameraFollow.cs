using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(1.2f, 0.6f);
    public float smoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;
    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    // Follows the target smoothly and applies any active screen shake offset.
    void LateUpdate()
    {
        if (target != null)
        {
            float facing = target.localScale.x >= 0f ? 1f : -1f;
            Vector3 desired = new Vector3(
                target.position.x + offset.x * facing,
                target.position.y + offset.y,
                transform.position.z
            );
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                shakeOffset = (Vector3)Random.insideUnitCircle * shakeMagnitude;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
            transform.position = Vector3.SmoothDamp(transform.position, desired + shakeOffset, ref velocity, smoothTime);
        }
    }

    // Starts or extends a camera shake effect.
    public void Shake(float duration, float magnitude)
    {
        shakeTimer = Mathf.Max(shakeTimer, duration);
        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
    }
}
