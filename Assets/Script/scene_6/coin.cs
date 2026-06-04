using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;

    // Collects the coin when the player enters its trigger.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Get Coins: " + value);
            Destroy(gameObject);
        }
    }
}
