using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CartaInversion : MonoBehaviour
{
    [Header("UI Componentes")]
    public TMP_Text textoNombre;
    public TMP_Text textoDescripcion;
    public TMP_Text textoGanancia;       // �Ganancia fija: +X� o �Ganancia variable��
    public Button botonInvertir;
    public TMP_InputField inputCantidad;
    public Button botonConfirmar;
    public TMP_Text textoResultado;      // OPCIONAL: feedback al jugador

    [Header("UI Progreso (opcional)")]
    
    public Slider barraProgreso;          // Usa un Image con Fill = Filled (Radial o Horizontal)
    

    [Header("Datos de Inversi�n")]
    public string nombreInversion;
    public string descripcion;
    public int gananciaBase;             // usada en inversi�n segura
    public bool esRiesgosa;

   
    
    

    [Header("Tiempo de inversi�n (editable por carta)")]
    public bool usarRango = false;                 // Si true, usa un rango aleatorio
    public float duracionSegundos = 3f;            // Duraci�n fija si usarRango = false
    public Vector2 rangoDuracionSeg = new Vector2(2f, 5f); // [min, max] si usarRango = true

    private bool inversionEnCurso = false;
    private int cantidadPendiente = 0;
    

    void OnEnable()
    {
        // Inicializa visibilidad de controles
        if (inputCantidad) { inputCantidad.text = ""; inputCantidad.gameObject.SetActive(false); }
        if (botonConfirmar) { botonConfirmar.gameObject.SetActive(false); botonConfirmar.interactable = true; }
        if (botonInvertir)
        {
            botonInvertir.onClick.RemoveAllListeners();
            botonInvertir.onClick.AddListener(ActivarInput);
            botonInvertir.gameObject.SetActive(true);
            botonInvertir.interactable = true;
        }

        // Progreso oculto al inicio
        if (barraProgreso) 
        { 
            barraProgreso.value = 0f;  barraProgreso.gameObject.SetActive(false); 
        }

        

        // Refresca textos
        if (textoNombre) textoNombre.text = nombreInversion;
        if (textoDescripcion) textoDescripcion.text = descripcion;
        if (textoGanancia)
            textoGanancia.text = esRiesgosa
                ? "Ganancia variable (riesgosa)"
                : $"Ganancia fija: +{gananciaBase}";
        if (textoResultado) textoResultado.text = "";

        // Habilitar/Deshabilitar bot�n seg�n desbloqueo
        ActualizarInteractividad();
    }

    // Si prefieres setear datos por c�digo:
    public void InicializarCarta(string nombre, string desc, int ganancia, bool riesgosa)
    {
        nombreInversion = nombre;
        descripcion = desc;
        gananciaBase = ganancia;
        esRiesgosa = riesgosa;

        if (textoNombre) textoNombre.text = nombreInversion;
        if (textoDescripcion) textoDescripcion.text = descripcion;
        if (textoGanancia)
            textoGanancia.text = esRiesgosa
                ? "Ganancia variable (riesgosa)"
                : $"Ganancia fija: +{gananciaBase}";
    }

    public void ActualizarInteractividad()
    {
        bool puedeInvertir = !esRiesgosa || UIManager.instancia.RiesgosDesbloqueados();
        if (botonInvertir) botonInvertir.interactable = puedeInvertir && !inversionEnCurso;
    }

    void ActivarInput()
    {
        // Doble check por seguridad
        if (esRiesgosa && !UIManager.instancia.RiesgosDesbloqueados())
        {
            MostrarResultado("A�n no desbloqueada. Haz primero inversiones seguras exitosas.");
            return;
        }
        if (inversionEnCurso) return;

        if (botonInvertir) botonInvertir.gameObject.SetActive(false);
        if (inputCantidad) { inputCantidad.text = ""; inputCantidad.gameObject.SetActive(true); inputCantidad.ActivateInputField(); }

        if (botonConfirmar)
        {
            botonConfirmar.onClick.RemoveAllListeners();
            botonConfirmar.onClick.AddListener(ConfirmarInversion);
            botonConfirmar.gameObject.SetActive(true);
            botonConfirmar.interactable = true;
        }
    }

    void ConfirmarInversion()
    {
        if (!int.TryParse(inputCantidad.text, out int cantidad) || cantidad <= 0)
        {
            MostrarResultado("Ingresa una cantidad v�lida.");
            return;
        }

        // Descontar primero (con UIManager para refrescar UI)
        if (!UIManager.instancia.TryGastarCreditos(cantidad))
        {
            MostrarResultado("No tienes suficientes cr�ditos.");
            return;
        }

        // Duraci�n por carta (lo que ya ten�as configurable en el Inspector)
        float duracion = usarRango
            ? Random.Range(rangoDuracionSeg.x, rangoDuracionSeg.y)
            : duracionSegundos;

        // Lanzar procesamiento en un objeto que NO se desactiva
        InvestmentManager.Instancia.IniciarInversion(
            nombreInversion,
            esRiesgosa,
            cantidad,
            gananciaBase,
            duracion,
            onResolvedUI: () =>
            {
                // Este callback se llama al terminar, SI la carta sigue activa.
                // Aqu� puedes resetear UI local si quieres.
                if (this && gameObject.activeInHierarchy)
                {
                    if (inputCantidad) inputCantidad.gameObject.SetActive(false);
                    if (botonConfirmar) botonConfirmar.gameObject.SetActive(false);
                    if (botonInvertir) botonInvertir.gameObject.SetActive(true);
                }
            }
        );

        // (Opcional) mostrar barra local mientras el panel est� abierto
        if (barraProgreso)
        {
            barraProgreso.value = 0f;
            barraProgreso.gameObject.SetActive(true);
            StartCoroutine(BarraLocal(duracion));
        }
        if (botonInvertir) botonInvertir.gameObject.SetActive(false);
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(false);
        if (inputCantidad) inputCantidad.gameObject.SetActive(false);
    }

    // Solo visual; si cierras el panel este UI se desactiva y no pasa nada.
    IEnumerator BarraLocal(float dur)
    {
        float t = 0f;
        while (t < dur && gameObject.activeInHierarchy)
        {
            t += Time.unscaledDeltaTime;
            if (barraProgreso) barraProgreso.value = Mathf.Clamp01(t / dur);
            yield return null;
        }
        if (barraProgreso) barraProgreso.gameObject.SetActive(false);
    }

    IEnumerator RutinaInversion(float duracion)
    {
        inversionEnCurso = true;

        // Bloquear UI local durante el proceso
        if (inputCantidad) inputCantidad.gameObject.SetActive(false);
        if (botonConfirmar) botonConfirmar.gameObject.SetActive(false);
        if (botonInvertir) botonInvertir.interactable = false;

        // Mostrar progreso
        if (barraProgreso) 
        {
            barraProgreso.gameObject.SetActive(true); barraProgreso.value = 0f;

        }
        

        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime; // usa unscaled por si pausas el juego con timeScale
            float p = Mathf.Clamp01(t / duracion);
            if (barraProgreso) barraProgreso.value = p;
            
            yield return null;
        }

        // Ocultar progreso
        if (barraProgreso) barraProgreso.gameObject.SetActive(false);
        

        // Calcular resultado al final de la espera
        ResolverInversion(cantidadPendiente);

        // Reset UI local de la carta
        if (botonInvertir)
        {
            botonInvertir.gameObject.SetActive(true);
            botonInvertir.interactable = true;
        }
        inversionEnCurso = false;
        ActualizarInteractividad(); // revalida si la riesgosa debe habilitarse
    }

    void ResolverInversion(int cantidad)
    {
        if (esRiesgosa)
        {
            // 50% pierde todo, 50% gana (2x a 4x)
            bool gano = Random.value < 0.5f;
            if (!gano)
            {
                MostrarResultado($"Invertiste {cantidad} y perdiste todo. ");
            }
            else
            {
                int multiplicador = Random.Range(2, 5); // 2,3,4 (max exclusivo en enteros)
                int retorno = cantidad * multiplicador;
                UIManager.instancia.AgregarCreditos(retorno);
                MostrarResultado($"�Ganaste! Retorno: {retorno} (x{multiplicador}). ");
            }
        }
        else
        {
            int retorno = cantidad + Mathf.Max(0, gananciaBase);
            UIManager.instancia.AgregarCreditos(retorno);
            UIManager.instancia.RegistrarInversionSeguraExitosa();
            MostrarResultado($"Inversi�n segura exitosa. Retorno: {retorno} (+{gananciaBase}). ");

            // Por si se desbloquean las riesgosas justo ahora:
            UIManager.instancia.RefrescarInteractividadCartas();
        }
    }

    void MostrarResultado(string msg)
    {
        if (textoResultado) textoResultado.text = msg;
        else Debug.Log(msg);
    }
}
