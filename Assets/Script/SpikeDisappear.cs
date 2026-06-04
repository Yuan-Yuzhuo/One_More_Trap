using System.Collections;
using UnityEngine;

public class SpikeDisappear : MonoBehaviour
{
    public GameObject spikeGroup;
    public float delayTime = 6.5f;
    public float hideTime = 0.5f;

    void Start()
    {
        StartCoroutine(DisappearRoutine());
    }

    // Temporarily hides the spike group after a delay, then restores it.
    IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(delayTime);
        spikeGroup.SetActive(false);

        yield return new WaitForSeconds(hideTime);
        spikeGroup.SetActive(true);
    }
}
