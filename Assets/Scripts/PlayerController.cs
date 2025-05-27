using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovimientoBasico : MonoBehaviour
{
    public float velocidad = 5f;
    public float gravedad = -9.81f;
    public float alturaSalto = 1.5f;

    private CharacterController controller;
    private Vector3 velocidadVertical;
    private bool estaEnSuelo;

    public Transform chequeoSuelo;
    public float distanciaSuelo = 0.4f;
    public LayerMask capaSuelo;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Comprobamos si está en el suelo
        estaEnSuelo = Physics.CheckSphere(chequeoSuelo.position, distanciaSuelo, capaSuelo);

        if (estaEnSuelo && velocidadVertical.y < 0)
            velocidadVertical.y = -2f; // mantener al personaje en el suelo

        // Movimiento horizontal
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 mover = transform.right * x + transform.forward * z;
        controller.Move(mover * velocidad * Time.deltaTime);

        // Saltar
        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            velocidadVertical.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);
        }

        // Aplicar gravedad
        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }
}

    

