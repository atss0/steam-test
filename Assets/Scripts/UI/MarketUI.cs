using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketUI : MonoBehaviour
{
    public static MarketUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    public GameObject marketPanel;
    public TextMeshProUGUI creditsDisplay;
    public TextMeshProUGUI notificationText;
    public Transform buttonContainer;   // VerticalLayoutGroup eklenmiş boş obje
    public Button closeButton;

    [Header("Upgrade Button Prefab")]
    public GameObject upgradeButtonPrefab;

    [Header("Upgrades — buraya ScriptableObject'leri sürükle")]
    public List<UpgradeData> upgrades;

    private PlayerUpgrades playerUpgrades;
    private Coroutine notifCoroutine;

    void Awake()
    {
        Instance = this;
        marketPanel.SetActive(false);
    }

    void Start()
    {
        closeButton.onClick.AddListener(Close);
        BuildButtons();
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    // ── Buton Üretimi ────────────────────────────────────────────────

    void BuildButtons()
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (UpgradeData upgrade in upgrades)
        {
            GameObject btnObj = Instantiate(upgradeButtonPrefab, buttonContainer);

            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"{upgrade.upgradeName}\n{upgrade.cost} Kredi";

            // Closure — hangi upgrade olduğunu yakala
            UpgradeData captured = upgrade;
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                playerUpgrades?.CmdPurchaseUpgrade(
                    captured.cost,
                    captured.upgradeType,
                    captured.upgradeAmount
                );
            });
        }
    }

    // ── Aç / Kapat ───────────────────────────────────────────────────

    public void Open(PlayerUpgrades upgrades)
    {
        playerUpgrades = upgrades;
        RefreshCredits(GameManager.Instance != null ? GameManager.Instance.CompanyCredits : 0);
        marketPanel.SetActive(true);
        IsOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        marketPanel.SetActive(false);
        IsOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ── Geri Bildirim ────────────────────────────────────────────────

    public void RefreshCredits(int credits)
    {
        if (creditsDisplay != null)
            creditsDisplay.text = $"Şirket Kasası: {credits} Kredi";
    }

    public void OnPurchaseSuccess()
    {
        RefreshCredits(GameManager.Instance != null ? GameManager.Instance.CompanyCredits : 0);
        ShowNotification("Satın Alma Başarılı!", Color.green);
    }

    public void OnPurchaseFailed()
    {
        ShowNotification("Yetersiz Kredi!", Color.red);
    }

    void ShowNotification(string message, Color color)
    {
        if (notificationText == null) return;
        if (notifCoroutine != null) StopCoroutine(notifCoroutine);
        notificationText.text = message;
        notificationText.color = color;
        notificationText.gameObject.SetActive(true);
        notifCoroutine = StartCoroutine(HideNotification());
    }

    IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(2.5f);
        notificationText.gameObject.SetActive(false);
        notifCoroutine = null;
    }
}
