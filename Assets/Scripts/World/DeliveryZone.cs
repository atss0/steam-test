using UnityEngine;
using Mirror;

// Modüler tasarım: ileride bu script bir kamyonet objesine de eklenebilir.
// Trigger tespiti client-side çalışır, gerçek işlem Command üzerinden sunucuda yapılır.
public class DeliveryZone : NetworkBehaviour
{
    private PlayerInventory localPlayerInZone;

    void Update()
    {
        if (localPlayerInZone == null) return;

        bool hasItems = localPlayerInZone.toplananEsya > 0;

        // Envanter durumuna göre prompt'u dinamik göster/gizle
        if (HUDManager.Instance != null)
        {
            bool isShown = HUDManager.Instance.deliveryZoneText.gameObject.activeSelf;
            if (hasItems && !isShown) ShowPrompt();
            else if (!hasItems && isShown) HidePrompt();
        }

        if (Input.GetKeyDown(KeyCode.G) && hasItems)
            localPlayerInZone.CmdDeliverLoot();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null || !inv.isLocalPlayer) return;

        localPlayerInZone = inv;

        if (inv.toplananEsya > 0)
            ShowPrompt();
    }

    void OnTriggerExit(Collider other)
    {
        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null || !inv.isLocalPlayer) return;

        localPlayerInZone = null;
        HidePrompt();
    }

    void ShowPrompt()
    {
        if (HUDManager.Instance == null) return;
        HUDManager.Instance.deliveryZoneText.text = "G - Kasaya Bırak";
        HUDManager.Instance.deliveryZoneText.gameObject.SetActive(true);
    }

    void HidePrompt()
    {
        if (HUDManager.Instance == null) return;
        HUDManager.Instance.deliveryZoneText.gameObject.SetActive(false);
    }
}
