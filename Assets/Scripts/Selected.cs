using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Selected : MonoBehaviour
{
    LayerMask mask;
    public float distancia = 3.0f;

    public Texture2D puntero;
    public GameObject TextDetect;
    GameObject ultimoReconocido = null;
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        mask = LayerMask.GetMask("RayCast Detect");
        TextDetect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, distancia, mask))
        {
            Debug.Log("Hit: " + hit.collider.name);
            Deselect();
            SelectedObject(hit.transform);

            if (hit.collider.CompareTag("Objeto Interactivo") && Input.GetKeyDown(KeyCode.E))
            {
                hit.collider.GetComponent<ObjetoInteractivo>().ActivarObjeto();
            }

            Debug.DrawRay(ray.origin, ray.direction * distancia, Color.magenta);
        }
        else
        {
            Deselect();
        }
    }

    void SelectedObject(Transform transform)
    {
        transform.GetComponent<MeshRenderer>().material.color = Color.cyan;
        ultimoReconocido = transform.gameObject;
    }

    void Deselect()
    {
        if (ultimoReconocido)
        {
            ultimoReconocido.GetComponent < Renderer>().material.color = Color.white;
            ultimoReconocido = null;
        }
    }

    void OnUGUI()
    {
        Rect rect = new Rect(Screen.width / 2, Screen.height / 2, puntero.width, puntero.height);
        GUI.DrawTexture(rect, puntero);

        if(ultimoReconocido)
        {
            TextDetect.SetActive(true);
        }

        else
        {
            TextDetect.SetActive(false);
        }
    }


    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * distancia);
        Gizmos.DrawWireSphere(transform.position + transform.forward * distancia, 0.1f);
    }

}
