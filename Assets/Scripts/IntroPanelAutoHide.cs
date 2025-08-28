using UnityEngine;
using System.Collections;

public class IntroPanelAutoHide : MonoBehaviour
{
    [Header("Panel a mostrar al iniciar")]
    public GameObject panel;                // Asigna tu panel de bienvenida
    public float duracionSegundos = 5f;     // 5 por defecto

    [Header("Comportamiento mientras está visible")]
    public bool pausarJuego = false;        // Si quieres que no corra el tiempo
    public bool bloquearMovimiento = true;  // Deshabilitar movimiento del jugador
    public bool mostrarCursor = true;       // Mostrar cursor mientras el panel está activo
    public KeyCode cerrarAnticipado = KeyCode.None; // p.ej. KeyCode.Space si quieres cerrar antes

    [Header("Opcional")]
    public FPSController controladorJugador;  // Arrastra tu FPSController si tienes uno
    

    float timeScalePrevio = 1f;
    bool activo = false;

    void Start()
    {
        if (!panel)
        {
            Debug.LogWarning("IntroPanelAutoHide: no hay panel asignado.");
            return;
        }

        Mostrar();
        // Usa tiempo REAL para el temporizador si pausas el juego, normal en caso contrario
        if (pausarJuego) StartCoroutine(AutoOcultarRealtime());
        else StartCoroutine(AutoOcultar());
    }

    void Update()
    {
        if (!activo) return;

        if (cerrarAnticipado != KeyCode.None && Input.GetKeyDown(cerrarAnticipado))
            Ocultar();
    }

    void Mostrar()
    {
        activo = true;
        panel.SetActive(true);

        

        if (pausarJuego)
        {
            timeScalePrevio = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (bloquearMovimiento && controladorJugador) controladorJugador.habilitarMovimiento = false;

        if (mostrarCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Ocultar()
    {
        if (!activo) return;
       

        
        else panel.SetActive(false);

        if (pausarJuego) Time.timeScale = timeScalePrevio;

        if (bloquearMovimiento && controladorJugador) controladorJugador.habilitarMovimiento = true;

        // Opcional: si quieres volver a bloquear el cursor:
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    IEnumerator AutoOcultar()
    {
        yield return new WaitForSeconds(duracionSegundos);
        Ocultar();
    }

    IEnumerator AutoOcultarRealtime()
    {
        yield return new WaitForSecondsRealtime(duracionSegundos);
        Ocultar();
    }

    IEnumerator Fade(CanvasGroup cg, float a, float b, float t)
    {
        float el = 0f;
        while (el < t)
        {
            el += (pausarJuego ? Time.unscaledDeltaTime : Time.deltaTime);
            cg.alpha = Mathf.Lerp(a, b, el / t);
            yield return null;
        }
        cg.alpha = b;
    }

    IEnumerator FadeYApagar(CanvasGroup cg, float a, float b, float t)
    {
        yield return Fade(cg, a, b, t);
        panel.SetActive(false);
    }
}
