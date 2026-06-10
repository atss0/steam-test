using UnityEngine;
using Mirror;

// Etkileşim mantığı InteractionSystem'dadır (oyuncu tarafında).
// İleride terminalID, stok bilgisi gibi veriler buraya eklenir.
public class MarketTerminal : NetworkBehaviour
{
    [SyncVar]
    public string terminalName = "Market Terminali";
}
