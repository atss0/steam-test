using UnityEngine;

public enum UpgradeType { Speed, JumpForce }

// Yeni upgrade eklemek için: Assets → sağ tık → Create → Game → Upgrade Data
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName = "Yeni Upgrade";
    [TextArea] public string description = "Açıklama";
    public int cost = 300;
    public UpgradeType upgradeType = UpgradeType.Speed;
    public float upgradeAmount = 1f;
}
