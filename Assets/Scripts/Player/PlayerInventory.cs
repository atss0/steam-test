using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnItemCountChanged))]
    public int toplananEsya = 0;

    private FPSController fpsController;

    public override void OnStartClient()
    {
        base.OnStartClient();
        fpsController = GetComponent<FPSController>();

        if (isLocalPlayer)
        {
            UpdateInventoryUI(toplananEsya);
            fpsController?.ApplyWeightPenalty(toplananEsya);
        }
    }

    // InteractionSystem'dan çağrılır (sunucu tarafında)
    public void AddItem()
    {
        toplananEsya++;
    }

    // MonsterAI tarafından sunucu üzerinde çağrılır — SyncVar hook UI+ağırlığı günceller
    [Server]
    public void ResetInventory()
    {
        toplananEsya = 0;
    }

    // DeliveryZone'dan tetiklenir — client çağırır, sunucu işler
    [Command]
    public void CmdDeliverLoot()
    {
        if (toplananEsya <= 0) return;

        // Kasaya ekle
        if (GameManager.Instance != null)
            GameManager.Instance.AddLootToStash(toplananEsya);

        // Envanteri sıfırla — SyncVar hook hızı ve UI'ı otomatik günceller
        toplananEsya = 0;
    }

    void OnItemCountChanged(int oldValue, int newValue)
    {
        if (!isLocalPlayer) return;

        UpdateInventoryUI(newValue);

        if (fpsController == null)
            fpsController = GetComponent<FPSController>();

        fpsController?.ApplyWeightPenalty(newValue);
    }

    void UpdateInventoryUI(int count)
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.inventoryText.text = $"Toplanan: {count}";
    }
}
