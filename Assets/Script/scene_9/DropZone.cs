using UnityEngine;

public class DropZone2D : MonoBehaviour
{
    // Detaches the player from a moving platform when entering the drop zone.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        other.transform.SetParent(null);
    }
}
