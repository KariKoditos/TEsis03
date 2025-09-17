using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CartaInversion : MonoBehaviour
{
    [Header("UI Componentes")]
    public TMP_Text textoNombre;
    public TMP_Text textoDescripcion;
    public TMP_Text textoGanancia;       
    public Button botonInvertir;
    public TMP_InputField inputCantidad;
    public Button botonConfirmar;
    public TMP_Text textoResultado;      

    [Header("UI Progreso (opcional)")]

    public Slider barraProgreso;          

    [Header("Datos de Inversión")]
    public string nombreInversion;
    public string descripcion;
    public int gananciaBase;             
    public bool esRiesgosa;

    private IInvestmentStrategy _estrategia;





    [Header("Tiempo de inversión (editable por carta)")]
    public bool usarRango = false;                 
    public float duracionSegundos = 3f;            
    public Vector2 rangoDuracionSeg = new Vector2(2f, 5f); 

    private bool inversionEnCurso = false;
    private int cantidadPendiente = 0;


    void OnEnable()
    {
        
        if (inputCantidad) { inputCantidad.text = ""; inputCantidad.gameObject.SetActive(false); }
        if (botonConfirmar) { botonConfirmar.gameObject.SetActive(false); botonConfirmar.interactable = true; }
        if (botonInvertir)
        {
            botonInvertir.onClick.RemoveAllListeners();
            botonInvertir.onClick.AddListener(ActivarInput);
            botonInvertir.gameObject.SetActive(true);
            botonInvertir.interactable = true;
        }

        
        if (barraProgreso)
        {
            barraProgreso.value = 0f; barraProgreso.gameObject.SetActive(false);
        }

        if (esRiesgosa)
        {
            
            _estrategia = new InversionRiesgosaStrategy(-0.10f, 0.20f); //strategy
        }
        else
        {
            
            _estrategia = new InversionSeguraStrategy(gananciaBase);
        }

        
        if (textoNombre) textoNombre.text = nombreInversion;
        if (textoDescripcion) textoDescripcion.text = descripcion;
        if (textoGanancia)
            textoGanancia.text = esRiesgosa
                ? "Ganancia variable (riesgosa)"
                : $"Ganancia fija: +{gananciaBase}";
        if (textoResultado) textoResultado.text = "";

        
        ActualizarInteractividad();
    }

    // Si prefieres setear datos por código:
    public void InicializarCarta(string nombre, string desc, int ganancia, bool riesgosa)
    {
        nombreInversion = nombre;
        descripcion = desc;
        gananciaBase = ganancia;
        esRiesgosa = riesgosa;

        if (textoNombre) textoNombre.text = nombreInversion;
        if (textoDescripcion) textoDescripcion.text = descripcion;
        if (esRiesgosa)
        {
            textoGanancia.text = "Ganancia variable (riesgosa)";
        }
        else
        {
            textoGanancia.text = "Ganancia fija: +" + gananciaBase;
        };
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
            MostrarResultado("Aún no desbloqueada. Haz primero inversiones seguras exitosas.");
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
            MostrarResultado("Ingresa una cantidad válida.");
            return;
        }

        // Descontar primero (con UIManager para refrescar UI)
        if (!UIManager.instancia.TryGastarCreditos(cantidad))
        {
            MostrarResultado("No tienes suficientes créditos.");
            return;
        }

        // Duración por carta (lo que ya tenías configurable en el Inspector)
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
                // Aquí puedes resetear UI local si quieres.
                if (this && gameObject.activeInHierarchy)
                {
                    if (inputCantidad) inputCantidad.gameObject.SetActive(false);
                    if (botonConfirmar) botonConfirmar.gameObject.SetActive(false);
                    if (botonInvertir) botonInvertir.gameObject.SetActive(true);
                }
            }
        );

        // (Opcional) mostrar barra local mientras el panel está abierto
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
        // Retorno variable según estrategia
        int delta = _estrategia.CalcularRetorno(cantidad);
        int retornoTotal = cantidad + delta;  

        if (retornoTotal <= 0)
        {
            //pérdida total 
            MostrarResultado($"Invertiste {cantidad} y no obtuviste retorno.");
        }
        else
        {
            UIManager.instancia.AgregarCreditos(retornoTotal);
            MostrarResultado($"Retorno: {retornoTotal} ({_estrategia.Nombre}).");
        }

        // Lógica de progresión 
        if (!esRiesgosa && delta > 0)
        {
            UIManager.instancia.RegistrarInversionSeguraExitosa();
            UIManager.instancia.RefrescarInteractividadCartas();
        }
    }

    void MostrarResultado(string msg)
    {
        if (textoResultado) textoResultado.text = msg;
        else Debug.Log(msg);
    }

 
}
