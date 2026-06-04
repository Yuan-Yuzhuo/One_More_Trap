using UnityEngine;

public class CoinRemoveGround : MonoBehaviour
{
    public GameObject groundToRemove;

    // Removes the linked ground and consumes this coin when collected.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (groundToRemove != null)
            {
                groundToRemove.SetActive(false);
            }

            Destroy(gameObject);
        }
    }
}
