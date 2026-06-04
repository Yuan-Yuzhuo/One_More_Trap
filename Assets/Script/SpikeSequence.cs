using System.Collections;
using UnityEngine;

public class SpikeSequence2D : MonoBehaviour
{
    public GameObject[] spikeRows;
    public float delay = 0.3f;

    private bool triggered = false;

    // Starts showing spike rows one by one.
    public void StartSequence()
    {
        if (triggered) return;

        triggered = true;

        StartCoroutine(ShowSpikes());
    }

    // Enables each configured spike row with a delay between rows.
    private IEnumerator ShowSpikes()
    {
        for (int i = 0; i < spikeRows.Length; i++)
        {
            spikeRows[i].SetActive(true);

            yield return new WaitForSeconds(delay);
        }
    }
}
