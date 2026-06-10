using UnityEngine;
using Mirror;

public class PlayerUpgrades : NetworkBehaviour
{
    // Client çağırır → sunucu işler → sadece bu oyuncuya sonuç gönderilir
    [Command]
    public void CmdPurchaseUpgrade(int cost, UpgradeType upgradeType, float upgradeAmount)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (!gm.TryPurchase(cost))
        {
            TargetPurchaseFailed(connectionToClient);
            return;
        }

        TargetApplyUpgrade(connectionToClient, upgradeType, upgradeAmount);
    }

    // Sadece satın alan oyuncuda çalışır — hız/zıplama artışı burada uygulanır
    [TargetRpc]
    void TargetApplyUpgrade(NetworkConnection target, UpgradeType upgradeType, float upgradeAmount)
    {
        FPSController fps = GetComponent<FPSController>();

        switch (upgradeType)
        {
            case UpgradeType.Speed:
                fps?.ApplySpeedUpgrade(upgradeAmount);
                break;
            case UpgradeType.JumpForce:
                fps?.ApplyJumpUpgrade(upgradeAmount);
                break;
        }

        MarketUI.Instance?.OnPurchaseSuccess();
    }

    [TargetRpc]
    void TargetPurchaseFailed(NetworkConnection target)
    {
        MarketUI.Instance?.OnPurchaseFailed();
    }
}
