using UnityEngine;
using Mirror;

// Satış terminali prefab'ının kimlik scripti.
// Etkileşim mantığı InteractionSystem'dadır (oyuncu tarafında).
// İleride terminalName, terminalID gibi özellikler buraya eklenir.
public class SalesTerminal : NetworkBehaviour
{
    [SyncVar]
    public string terminalName = "Satış Terminali";
}
