using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        // Kameranın baktığı yönün tersine döndür — metin her zaman oyuncuya bakar
        transform.rotation = Quaternion.LookRotation(-cam.transform.forward, cam.transform.up) * Quaternion.Euler(0, 180, 0);
    }
}
