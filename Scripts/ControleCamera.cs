/* Classe para controle da camera do jogo de Boliche.

   Joao Marcello, 2021.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleCamera : MonoBehaviour
{
    public GameObject bola;    // referencia para a bola
    private Vector3 offset;
    private Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - bola.transform.position;   // distancia da bola pra camera
        initialPosition = transform.position;  // salvando a posicao inicial da camera (para a funcao reset)
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (transform.position.z > -4.0f) {    // nao muda a posicao da camera se a posicao.z for maior que -4
            transform.position = bola.transform.position + offset;
        }

    }

    // coloca a camera na posicao inicial
    public void Reset(){
        transform.position = initialPosition;
    }
}
