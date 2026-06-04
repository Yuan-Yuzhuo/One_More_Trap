using UnityEngine;

public class RotateLine : MonoBehaviour
{
    public float rotateSpeed = 90f;

    // Rotates this object continuously around the Z axis.
    void Update()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
