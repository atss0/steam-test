using Mirror;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToxicWater : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkServer.active) return;

        if (other.TryGetComponent<PlayerStats>(out var player) && !player.hasSuit)
            player.Die();
    }
}
