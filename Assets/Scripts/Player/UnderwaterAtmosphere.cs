using Mirror;
using UnityEngine;

public class UnderwaterAtmosphere : NetworkBehaviour
{
    [Header("Threshold")]
    public float underwaterY = -1000f;

    [Header("Underwater Fog")]
    public Color underwaterFogColor = new Color(0.02f, 0.08f, 0.10f);
    public float underwaterFogDensity = 0.08f;
    public FogMode underwaterFogMode = FogMode.ExponentialSquared;

    [Header("Transition")]
    public float transitionSpeed = 2f;

    // Yüzey sis değerleri — Start'ta kaydedilir
    private bool   surfaceFogEnabled;
    private Color  surfaceFogColor;
    private float  surfaceFogDensity;
    private FogMode surfaceFogMode;

    private CameraClearFlags surfaceClearFlags;
    private Color            surfaceBgColor;
    private Camera           cam;

    private bool  isUnderwater;
    private Color targetColor;
    private float targetDensity;

    private void Start()
    {
        if (!isLocalPlayer) return;

        surfaceFogEnabled = RenderSettings.fog;
        surfaceFogColor   = RenderSettings.fogColor;
        surfaceFogDensity = RenderSettings.fogDensity;
        surfaceFogMode    = RenderSettings.fogMode;

        cam = GetComponent<FPSController>()?.playerCamera ?? Camera.main;
        if (cam != null)
        {
            surfaceClearFlags = cam.clearFlags;
            surfaceBgColor    = cam.backgroundColor;
        }

        targetColor   = surfaceFogColor;
        targetDensity = surfaceFogDensity;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        bool shouldBeUnderwater = transform.position.y < underwaterY;

        if (shouldBeUnderwater != isUnderwater)
        {
            isUnderwater = shouldBeUnderwater;

            if (isUnderwater)
            {
                RenderSettings.fog     = true;
                RenderSettings.fogMode = underwaterFogMode;
                targetColor            = underwaterFogColor;
                targetDensity          = underwaterFogDensity;

                if (cam != null)
                {
                    cam.clearFlags     = CameraClearFlags.SolidColor;
                    cam.backgroundColor = underwaterFogColor;
                }
            }
            else
            {
                RenderSettings.fog     = surfaceFogEnabled;
                RenderSettings.fogMode = surfaceFogMode;
                targetColor            = surfaceFogColor;
                targetDensity          = surfaceFogDensity;

                if (cam != null)
                {
                    cam.clearFlags      = surfaceClearFlags;
                    cam.backgroundColor = surfaceBgColor;
                }
            }
        }

        // Smooth renk ve yoğunluk geçişi
        RenderSettings.fogColor   = Color.Lerp(RenderSettings.fogColor,   targetColor,   Time.deltaTime * transitionSpeed);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetDensity, Time.deltaTime * transitionSpeed);
    }

    private void OnDestroy()
    {
        if (!isLocalPlayer) return;
        RenderSettings.fog        = surfaceFogEnabled;
        RenderSettings.fogColor   = surfaceFogColor;
        RenderSettings.fogDensity = surfaceFogDensity;
        RenderSettings.fogMode    = surfaceFogMode;

        if (cam != null)
        {
            cam.clearFlags      = surfaceClearFlags;
            cam.backgroundColor = surfaceBgColor;
        }
    }
}
