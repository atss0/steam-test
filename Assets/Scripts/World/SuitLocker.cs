using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SuitLocker : MonoBehaviour
{
    private readonly List<PlayerStats> playersInRange = new List<PlayerStats>();

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        foreach (var player in playersInRange)
        {
            if (player != null && player.isLocalPlayer)
            {
                player.CmdEquipSuit();
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerStats>(out var ps))
            playersInRange.Add(ps);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerStats>(out var ps))
            playersInRange.Remove(ps);
    }
}
