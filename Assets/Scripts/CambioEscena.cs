using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena : MonoBehaviour
{
    public void CambiarEscena()
    {
        Debug.Log("Botón presionado, intentando cargar escena");
        SceneManager.LoadScene("Game");
    }
}
