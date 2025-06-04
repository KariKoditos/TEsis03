using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Selected : MonoBehaviour
{
    LayerMask mask;
    public float distancia = 2.5f;

    public Texture2D puntero;
    public GameObject TextDetect;
    GameObject ultimoReconocido = null;

    // Start is called before the first frame update
    void Start()
    {
        mask = LayerMask.GetMask("RayCast Detect");
        TextDetect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit,distancia,mask))
        {
            Deselect();
            SelectedObject(hit.transform);
            Debug.Log("Siento cosas de chava");


            if( hit.collider.tag == "Objeto Interactivo")
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.transform.GetComponent<ObjetoInteractivo>().ActivarObjeto();
                }
            }

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) *  distancia, Color.magenta);
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
   }
