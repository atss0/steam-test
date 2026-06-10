using UnityEngine;
using Mirror;

public class InteractionSystem : NetworkBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;

    private Camera playerCam;

    // Loot state
    private NetworkIdentity targetLoot;
    private LootItem currentLootItem;

    // Sales Terminal state
    private bool isLookingAtTerminal;

    // Market Terminal state
    private bool isLookingAtMarket;

    void Start()
    {
        if (!isLocalPlayer) return;

        FPSController fps = GetComponent<FPSController>();
        if (fps != null)
            playerCam = fps.playerCamera;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (MarketUI.IsOpen) return; // Market açıkken raycast ve input durdurulur

        CheckForInteractable();
        HandleInput();
    }

    // ── Raycast ─────────────────────────────────────────────────────

    void CheckForInteractable()
    {
        if (playerCam == null) return;

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            if (hit.collider.CompareTag("Loot"))
            {
                HandleLootTarget(hit.collider);
                ClearTerminal();
                ClearMarket();
                return;
            }

            if (hit.collider.CompareTag("SalesTerminal"))
            {
                HandleTerminalTarget();
                ClearLoot();
                ClearMarket();
                return;
            }

            if (hit.collider.CompareTag("MarketTerminal"))
            {
                HandleMarketTarget();
                ClearLoot();
                ClearTerminal();
                return;
            }
        }

        ClearLoot();
        ClearTerminal();
        ClearMarket();
    }

    // ── Loot ────────────────────────────────────────────────────────

    void HandleLootTarget(Collider col)
    {
        LootItem lootItem = col.GetComponent<LootItem>();
        if (currentLootItem != null && currentLootItem != lootItem)
            currentLootItem.HidePrompt();

        currentLootItem = lootItem;
        targetLoot = col.GetComponent<NetworkIdentity>();
        currentLootItem?.ShowPrompt();
    }

    void ClearLoot()
    {
        currentLootItem?.HidePrompt();
        currentLootItem = null;
        targetLoot = null;
    }

    // ── Sales Terminal ───────────────────────────────────────────────

    void HandleTerminalTarget()
    {
        isLookingAtTerminal = true;

        GameManager gm = GameManager.Instance;
        int stashCount = gm != null ? gm.TotalLootInStash : 0;

        string prompt = stashCount > 0
            ? $"F - Sat  ({stashCount} Eşya / {stashCount * (gm?.GetCurrentScrapPrice() ?? 0)} Kredi)"
            : "Kasada Satılacak Eşya Yok";

        HUDManager.Instance?.SetTerminalPrompt(prompt, true);
    }

    void ClearTerminal()
    {
        if (!isLookingAtTerminal) return;
        isLookingAtTerminal = false;
        HUDManager.Instance?.SetTerminalPrompt("", false);
    }

    // ── Market Terminal ──────────────────────────────────────────────

    void HandleMarketTarget()
    {
        isLookingAtMarket = true;
        HUDManager.Instance?.SetTerminalPrompt("E - Market Aç", true);
    }

    void ClearMarket()
    {
        if (!isLookingAtMarket) return;
        isLookingAtMarket = false;
        HUDManager.Instance?.SetTerminalPrompt("", false);
    }

    // ── Input ────────────────────────────────────────────────────────

    void HandleInput()
    {
        // E — Eşya topla VEYA Market aç
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (targetLoot != null)
            {
                currentLootItem?.HidePrompt();
                currentLootItem = null;
                CmdPickupLoot(targetLoot);
            }
            else if (isLookingAtMarket)
            {
                GetComponent<PlayerAnimator>()?.TriggerButtonPush();
                ClearMarket();
                MarketUI.Instance?.Open(GetComponent<PlayerUpgrades>());
            }
        }

        // F — Satış terminali
        if (Input.GetKeyDown(KeyCode.F) && isLookingAtTerminal)
        {
            GetComponent<PlayerAnimator>()?.TriggerButtonPush();
            CmdSellLoot();
        }
    }

    // ── Commands (Server Authority) ──────────────────────────────────

    [Command]
    void CmdPickupLoot(NetworkIdentity lootIdentity)
    {
        if (lootIdentity == null) return;
        GetComponent<PlayerInventory>()?.AddItem();
        NetworkServer.Destroy(lootIdentity.gameObject);
    }

    [Command]
    void CmdSellLoot()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.TotalLootInStash <= 0) return;

        gm.SellAllLoot(out int itemsSold, out int creditsEarned);
        TargetSaleNotification(connectionToClient, itemsSold, creditsEarned);
    }

    // ── Target RPC ───────────────────────────────────────────────────

    [TargetRpc]
    void TargetSaleNotification(NetworkConnection target, int itemsSold, int creditsEarned)
    {
        HUDManager.Instance?.ShowSaleNotification(itemsSold, creditsEarned);
    }
}
