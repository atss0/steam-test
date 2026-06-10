using UnityEngine;
using Mirror;

public class LootItem : NetworkBehaviour
{
    [SyncVar]
    public string itemName = "Eşya";

    [Header("Interaction Prompt")]
    public GameObject promptObject;

    public void ShowPrompt() => promptObject?.SetActive(true);
    public void HidePrompt() => promptObject?.SetActive(false);
}
