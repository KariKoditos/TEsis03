using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena : MonoBehaviour
{
    public void CambiarEscena()
    {
        Debug.Log("Bot�n presionado, intentando cargar escena");
        SceneManager.LoadScene("Game");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
