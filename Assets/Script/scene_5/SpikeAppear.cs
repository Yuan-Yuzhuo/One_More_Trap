using UnityEngine;
using System.Collections;

public class SpikeAppear : MonoBehaviour
{
    public GameObject spikeGroup;
    public float delay = 1f;

    void Start()
    {
        StartCoroutine(Appear());
    }

    // Reveals the linked spike group after a delay.
    IEnumerator Appear()
    {
        yield return new WaitForSeconds(delay);

        spikeGroup.SetActive(true);
    }
}
