using UnityEngine;
using System.Collections;

public class DoorAppear : MonoBehaviour
{
    public GameObject door;
    public float delay = 1f;

    void Start()
    {
        StartCoroutine(Appear());
    }

    IEnumerator Appear()
    {
        yield return new WaitForSeconds(delay);

        door.SetActive(true);
    }
}