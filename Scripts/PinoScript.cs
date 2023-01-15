/* Classe para implementacao do comportamento dos pinos para o jogo de boliche.
   
   Joao Marcello, 2021.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinoScript : MonoBehaviour
{
    private bool hit = false;
    public controle bola;
    private Vector3 initialPosition;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!hit) {  // se ainda nao foi marcado como "atingido"...
            // se o pino foi derrubado...
            if ( (Mathf.Abs(transform.eulerAngles.z) < 360-20 && Mathf.Abs(transform.eulerAngles.z) > 20) //-25  30
            || (transform.eulerAngles.x < 360-20 && transform.eulerAngles.x > 20) ){
                bola.addScore();     //...incrementa a pontuacao da rodada
                hit = true;         // marca este pino como "atingido"
            }
        }   

        //print(transform.eulerAngles);    
    }

    // retorna true se o pino foi derrubado ou false caso contrario
    public bool isDown(){ return hit; }

    // coloca o pino na posicao inicial
    public void Reset(){
        transform.position = initialPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        hit = false;
    }

}
