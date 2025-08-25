using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Escena a cargar cuando una necesidad llegue a 0")]
    public string escenaGameOver;

    [Header("Escena de Juego (para reiniciar)")]
    public string escenaJuego;

    [Header("Escena de Menú Principal (opcional)")]
    public string escenaMenu;

    [Tooltip("Segundos de espera antes de cambiar de escena (por si quieres audio/FX).")]
    public float delaySegundos = 0f;

    private static GameOverUI instancia;

    void Awake()
    {
        // >>> FALTABAAAA <<< 
        instancia = this;

        // Asegurar que el tiempo corre normal
        Time.timeScale = 1f;

        // Mostrar cursor y desbloquearlo
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Garantizar que exista un EventSystem (para clicks de UI)
        if (FindObjectOfType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        
    }

    void Start()
    {
        if (NeedsSystem.Instancia != null)
            NeedsSystem.Instancia.OnAnyNeedZero += HandleGameOver;
        else
            Debug.LogWarning("GameOverUI: No encontró NeedsSystem.Instancia.");
    }

    void OnDestroy()
    {
        if (NeedsSystem.Instancia != null)
            NeedsSystem.Instancia.OnAnyNeedZero -= HandleGameOver;

        if (instancia == this) instancia = null;
    }

    void HandleGameOver()
    {
        Time.timeScale = 1f;
        if (delaySegundos > 0f) Invoke(nameof(CargarEscena), delaySegundos);
        else CargarEscena();
    }

    void CargarEscena()
    {
        if (string.IsNullOrEmpty(escenaGameOver))
        {
            Debug.LogError("GameOverUI: escenaGameOver no asignada.");
            return;
        }
        SceneManager.LoadScene(escenaGameOver, LoadSceneMode.Single);
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(escenaJuego))
        {
            Debug.LogError("GameOverUI: escenaJuego no asignada.");
            return;
        }
        SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }

    // === API estática para llamar desde JugadorFinanzas ===
    public static void TriggerGameOver()
    {
        if (instancia != null) instancia.HandleGameOver();
        else Debug.LogWarning("GameOverUI.TriggerGameOver fue llamado pero no hay instancia en escena.");
    }
}
