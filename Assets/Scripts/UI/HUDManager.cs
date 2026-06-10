using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Inventory")]
    public TextMeshProUGUI inventoryText;

    [Header("Delivery Zone")]
    public TextMeshProUGUI deliveryZoneText;

    [Header("Shared Stash")]
    public TextMeshProUGUI stashCountText;

    [Header("Economy")]
    public TextMeshProUGUI creditsText;
    public TextMeshProUGUI saleNotificationText;

    [Header("Terminal Prompt")]
    public TextMeshProUGUI terminalPromptText;

    [Header("Crosshair")]
    public Image crosshairDot;

    private Coroutine saleCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // InteractionSystem tarafından her frame çağrılır — terminal prompt
    public void SetTerminalPrompt(string text, bool visible)
    {
        if (terminalPromptText == null) return;
        terminalPromptText.text = text;
        terminalPromptText.gameObject.SetActive(visible);
    }

    // Satış sonrası geçici bildirim — 3 saniye sonra kaybolur
    public void ShowSaleNotification(int itemsSold, int creditsEarned)
    {
        if (saleNotificationText == null) return;
        if (saleCoroutine != null) StopCoroutine(saleCoroutine);

        saleNotificationText.text = $"{itemsSold} Eşya Satıldı!  +{creditsEarned} Kredi Kazanıldı!";
        saleNotificationText.gameObject.SetActive(true);
        saleCoroutine = StartCoroutine(HideSaleNotification());
    }

    IEnumerator HideSaleNotification()
    {
        yield return new WaitForSeconds(3f);
        saleNotificationText.gameObject.SetActive(false);
        saleCoroutine = null;
    }
}
