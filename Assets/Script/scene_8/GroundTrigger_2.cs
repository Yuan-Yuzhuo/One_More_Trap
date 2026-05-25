using UnityEngine;

public class GroundTrigger_2 : MonoBehaviour
{
    private Retractable_2 ground;

    void Start()
    {
        ground = GetComponentInParent<Retractable_2>();

        if (ground == null)
        {
            Debug.LogError("父物体没有 Retractable_2 脚本！");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ground != null)
            {
                ground.TriggerGround();
            }
        }
    }
}
