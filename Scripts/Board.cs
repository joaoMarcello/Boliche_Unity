/* Classe para implementacao do comportamento do quadro que mostra a pontuacao dos players para o jogo de boliche.
   
   Joao Marcello, 2021.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private List<GameObject> lista;   // lista para armazenar os componentes do quadro
    public Placar placar;       // referencia para o componente Placar da bola (responsavel por armazenar a pontuacao dos players)

    // Start is called before the first frame update
    void Start()
    {
        lista = new List<GameObject>();
        int c = 1;   // variavel para determinar o numero que deve ser mostrado acima do sub-quadro, que representara o numero da rodada

        // percorrendo cada sub-quadro existente nos componentes filhos, esvaziando os seus conteudos de texto
        // e adicionado o objeto na lista
        foreach(Transform child in transform){
            // se o componente filho for "Quadro" (um prefab que foi definido no editor do Unity)
            if(child.tag == "Quadro"){       
                lista.Add(child.gameObject);     // adicionando o objeto na lista
                Transform t = child.transform.Find("NumRodada");    // procurando no objeto o componente Text chamado "numRodada"
                (t.GetComponent<Text>()).text = c.ToString();      // setando o valor do atributo text do componente com o valor atual da variavel c
                
                // procurando no objeto o componente Text chamado "Lance1" (componente q mostra a quant. de pinos derrubados na primeiro lance)
                t = child.transform.Find("Lance1");   
                (t.GetComponent<Text>()).text = "";

                // procurando no objeto o componente Text chamado "Lance2" (componente q mostra a quant. de pinos derrubados na segundo lance)
                t = child.transform.Find("Lance2");  
                (t.GetComponent<Text>()).text = "";

                // procurando no objeto o componente Text chamado "Score" (compoentne q mostra a pontuacao total da rodada)
                t = child.transform.Find("Score");     
                (t.GetComponent<Text>()).text = "";

                c++;   // atualizando a variavel c
            }
        }

    }

    /* atualiza os valores mostrados pelo mostrador do placar
       Recebe: 
            - rodada: a rodada a ser atualizada
            - score1, score2: quant. de pinos derrubados no primeiro e segundo lance da rodada, respectivamente
            - totalScore: pontuacao total do player na rodada
       */
    public void setScore(int rodada, int score1, int score2, int totalScore){
        GameObject quadro = lista[rodada-1];

        Transform s1 = quadro.transform.Find("Lance1");   // gameObject que mostra o lance1
        Transform s2 = quadro.transform.Find("Lance2");   // gameObject que mostra o lance2
        Transform s3 = quadro.transform.Find("Score");    // gameObject que mostra a pontuacao da rodada

        // atualizando o mostrador do placar para o primeiro lance. Se o valor for -1, o quadro nao mostrara nada
        // se o valor for 10 (strike), eh sinalizado com um "X". Se o valor foi zero, mostrara "-"
        Text t = s1.GetComponent<Text>();
        t.text = score1 != -1 ? (score1 != 10 ? score1.ToString() : "X") : "";
        t.text = score1 == 0 ? "-" : t.text;

        // atualizando o mostrador do placar para o segundo lance. Se o valor for -1, o quadro nao mostrara nada
        // se a quant. de pinos derrubados nos dois lances totaliza 10 (spare), eh sinalizado com um "/". Se o valor foi zero, mostrara "-"
        t = s2.GetComponent<Text>();
        t.text = score2 != -1 ? (score1 + score2 == 10 ? "/" : score2.ToString()) : "";
        t.text = score2 == 0 ? "-" : t.text;

        // atualizando o mostrador da pontuacao total da rodada (nao mostra os pontos das rodadas realizadas no final do jogo caso o player
        // faca strike ou spare na ultima rodada)
        if(rodada <= placar.getLenGame()){
            t = s3.GetComponent<Text>();
            t.text = (totalScore != -1) ? totalScore.ToString() : "" ;
        }
    }

    // reseta o placar do jogo
    public void clear(){
        for(int i=1; i <= 12; i++){
            setScore(i, -1, -1, -1);
        }
    }

}
