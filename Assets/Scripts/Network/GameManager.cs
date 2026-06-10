using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SyncVar(hook = nameof(OnTotalLootChanged))]
    public int TotalLootInStash = 0;

    [SyncVar(hook = nameof(OnCreditsChanged))]
    public int CompanyCredits = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateStashUI(TotalLootInStash);
        UpdateCreditsUI(CompanyCredits);
    }

    // ── Kasa (Stash) ────────────────────────────────────────────────

    [Server]
    public void AddLootToStash(int amount)
    {
        TotalLootInStash += amount;
    }

    void OnTotalLootChanged(int oldValue, int newValue) => UpdateStashUI(newValue);

    void UpdateStashUI(int value)
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.stashCountText.text = $"Kasadaki Toplam Ganimet: {value}";
    }

    // ── Ekonomi ─────────────────────────────────────────────────────

    // Mock fiyat fonksiyonu — ileride Go API'sinden dinamik borsa fiyatı çekecek
    // TODO: İleride Go API'sine bağlanıp dinamik borsa fiyatı çekecek
    public int GetCurrentScrapPrice()
    {
        return 150;
    }

    // Tüm kasayı satar, sonuçları out parametreleriyle döner
    [Server]
    public void SellAllLoot(out int itemsSold, out int creditsEarned)
    {
        itemsSold = TotalLootInStash;
        creditsEarned = itemsSold * GetCurrentScrapPrice();

        CompanyCredits += creditsEarned;
        TotalLootInStash = 0;
    }

    // Market satın alımı — bakiye yeterliyse düşer ve true döner
    [Server]
    public bool TryPurchase(int cost)
    {
        if (CompanyCredits < cost) return false;
        CompanyCredits -= cost;
        return true;
    }

    void OnCreditsChanged(int oldValue, int newValue)
    {
        UpdateCreditsUI(newValue);
        // Market açıksa anında güncelle
        MarketUI.Instance?.RefreshCredits(newValue);
    }

    void UpdateCreditsUI(int value)
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.creditsText.text = $"Şirket Kasası: {value} Kredi";
    }
}
