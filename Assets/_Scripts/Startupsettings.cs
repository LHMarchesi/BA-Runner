using UnityEngine;

/// <summary>
/// Fuerza el modo fullscreen apenas arranca el juego,
/// antes de que cargue la primera escena. Corre una sola
/// vez al inicio; después el jugador puede seguir
/// cambiando a ventana desde Settings con total libertad
/// durante esa sesión.
/// </summary>
public static class StartupSettings
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void ForceFullscreenOnLaunch()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }
}