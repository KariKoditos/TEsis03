using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [Header("Escena de Juego ")]
    public string escenaJuego;

    [Header("Escena de Menú Principal")]
    public string escenaMenu;

    void Awake()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Reintentar()
    {
        SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }

    public void IrAlMenu()
    {
        if (!string.IsNullOrEmpty(escenaMenu))
            SceneManager.LoadScene(escenaMenu, LoadSceneMode.Single);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
