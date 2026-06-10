using UnityEngine;
using Mirror;

public class CameraSwitcher : NetworkBehaviour
{
    private FPSController fpsController;
    private PlayerAnimator playerAnimator;
    private Camera fpsCamera;
    private Camera overviewCamera;
    private bool isOverview = false;

    void Start()
    {
        if (!isLocalPlayer)
        {
            enabled = false;
            return;
        }

        fpsController = GetComponent<FPSController>();
        playerAnimator = GetComponent<PlayerAnimator>();
        fpsCamera = fpsController.playerCamera;

        GameObject camObj = GameObject.Find("OverviewCamera");
        if (camObj != null)
            overviewCamera = camObj.GetComponent<Camera>();
        else
            Debug.LogWarning("CameraSwitcher: 'OverviewCamera' sahnede bulunamadı!");

        if (overviewCamera != null)
            overviewCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (MarketUI.IsOpen) return;
        if (overviewCamera == null) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            isOverview = !isOverview;
            fpsCamera.gameObject.SetActive(!isOverview);
            overviewCamera.gameObject.SetActive(isOverview);

            // Dış kameraya geçince kendi modelini göster, FPS'e dönünce gizle
            if (playerAnimator != null)
                playerAnimator.SetModelVisible(isOverview);
        }
    }
}
