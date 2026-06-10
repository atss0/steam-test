using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHasSuitChanged))]
    public bool hasSuit = false;

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.K))
            CmdRequestDie();
    }

    [Server]
    public void Die()
    {
        RpcOnDie();
    }

    [Command]
    private void CmdRequestDie()
    {
        Die();
    }

    [ClientRpc]
    private void RpcOnDie()
    {
        Debug.Log($"[PlayerStats] {gameObject.name} died.");
        // TODO: death screen / respawn
    }

    [Command]
    public void CmdEquipSuit()
    {
        if (hasSuit) return;
        hasSuit = true;
        RpcOnEquipSuit();
    }

    [ClientRpc]
    private void RpcOnEquipSuit()
    {
        Debug.Log($"[PlayerStats] {gameObject.name} suit equipped.");
        // TODO: equip animation / sound
    }

    private void OnHasSuitChanged(bool oldVal, bool newVal)
    {
        // TODO: suit mesh görünürlüğünü aç/kapat
        Debug.Log($"[PlayerStats] hasSuit: {oldVal} -> {newVal}");
    }
}
