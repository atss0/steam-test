using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SubmarineDive : NetworkBehaviour
{
    [Header("Dive Settings")]
    public Transform diveTarget;

    [Header("Passenger Detection")]
    public float detectionRadius = 8f;
    public LayerMask playerMask = ~0;

    [Header("Effects")]
    public ParticleSystem bubbleEffect;

    private void Update()
    {
        // Sadece sunucu (host/kaptan) G'ye basarak dalışı başlatır.
        // F yerine G kullanıyoruz — F zaten SalesTerminal satışı için ayrılmış.
        if (!isServer) return;
        if (Input.GetKeyDown(KeyCode.G))
            ServerDive();
    }

    [Server]
    private void ServerDive()
    {
        if (diveTarget == null)
        {
            Debug.LogWarning("[SubmarineDive] diveTarget atanmamış!");
            return;
        }

        Vector3 delta = diveTarget.position - transform.position;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerMask);
        var processed = new HashSet<NetworkIdentity>();

        foreach (var hit in hits)
        {
            var player = hit.GetComponentInParent<PlayerStats>();
            if (player == null) continue;
            if (!processed.Add(player.netIdentity)) continue;

            TargetTeleportPlayer(player.connectionToClient, delta);
        }

        transform.position = diveTarget.position;
        RpcPlayBubbles();
    }

    [TargetRpc]
    private void TargetTeleportPlayer(NetworkConnectionToClient target, Vector3 delta)
    {
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;

        var cc = localPlayer.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        localPlayer.transform.position += delta;

        if (cc != null) cc.enabled = true;
    }

    [ClientRpc]
    private void RpcPlayBubbles()
    {
        if (bubbleEffect != null)
            bubbleEffect.Play();
    }
}
