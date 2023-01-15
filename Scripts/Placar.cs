/* Classe para gerenciar a pontuacao dos players.

   Joao Marcello, 2021.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// possiveis players da partida
public enum Players{
    player1,
    player2
}

public class Placar : MonoBehaviour
{   
    /* Matrizes para armazenar a pontuacao dos player (14 linhas e 3 colunas). O armazenamento se dah da seguinte forma:
       O primeiro indice representa a rodada atual. O segundo e terceiro indices representam, respectivamente, a quant. de pinos
       derrubados no primeiro e segundo lance da rodada. O quarto indice armazena os pontos obtidos na rodada. O valor -1 indica
       "bola ainda nao lancada" ou "pontuacao ainda nao computada"
    */
    private int[,] p1 = new int[14,3];
    private int[,] p2 = new int[14,3];

    private int rodada = 1;     // numero da rodada inicial
    public int gameLenght;      // quantidade de rodadas que tera o jogo 

    private Players current;   // o jogador atual
    public bool twoPlayers=true;   // habilita o modo para dois players
    
    public Text currentPlayerMessage;   // messagem que indica qual o player atual

    //public Text placarP1;
    //public Text placarP2;


    private bool p1Ended;   // indica se o player 1 ja jogou todas as suas rodadas
    private bool p2Ended;   // indica se o player 2 ja jogou todas as suas rodadas

    public GameObject p1Board;    // o mostrador do placar do player 1
    public GameObject p2Board;     // o mostrador do placar do player 1
  
    public GameObject manager;    // gerencia as trocas de tela

    // Start is called before the first frame update
    void Start()
    {
        InitGame();
    }

    public void InitGame(){

        // inicializando as matrizes com a pontuacao dos player (colocando -1 em todos os campos)
        for(int i=0; i <= gameLenght + 2; i++)
            for(int j = 0; j < 3; j++){
                p1[i,j] = -1;
                p2[i,j] = -1;
            }
        
        p1[0,2] = 0;
        p2[0,2] = 0;

        rodada = 1;

        p1Ended = false;
        p2Ended = false;

        current = Players.player1;   // o primeiro player eh o Player 1

        if(!twoPlayers){
            p2Board.SetActive(false);
            currentPlayerMessage.text = "";
        }

        //currentPlayerMessage.text = "VEZ DO PLAYER1";
    }

    /* Retorna a quantidade de srikes consecultivos dado o numero da rodada. */
    public int strikeRow(int rodada){
        int n = 0;
        int[,] v = currentIsPlayer1() ? p1 : p2;
        for(int k=rodada; k >= 1; k--){
            if(v[k,0] == 10)
                n++;
            else
                break;
        }
        return n;
    }

    // ativa os quadros com o placar dos players 
    public void showPlacar(){
        p1Board.SetActive(true);
        if(twoPlayers)
            p2Board.SetActive(true);
    }

    // oculta os quadros com o pla ar dos players
    public void hidePlacar(){ p1Board.SetActive(false); p2Board.SetActive(false);}

    // retorna true se o player 1 ja jogou todas as bolas do jogo 
    public void player1Ended(){ p1Ended = true; }

    // retorna true se o player 2 ja jogou todas as bolas do jogo
    public void player2Ended(){ p2Ended = true; }

    // // retorna true se ambos os players ja jogaram todas as bolas (fim do jogo)
    public bool endGame(){
        if(twoPlayers)
            return p1Ended && p2Ended;
        else
            return p1Ended;
    }

    // reinicia o jogo
    public void restart(){
        InitGame();
    }

    // retorna a pontuacao final de ambos os players (os pontos da ultima rodada)
    public int[] getTotalScore(){
        int[] n = new int[2];
        n[0] = p1[gameLenght, 2];
        n[1] = p2[gameLenght, 2];
        return n;
    }

    // retorna o valor da rodada atual
    public int getRodada(){ return rodada; }

    // retorna a quant. de rodadas da partida
    public int getLenGame(){ return gameLenght; }

    // retorna true se o player atual eh o player 1 ou false caso contrario
    public bool currentIsPlayer1(){
        return current == Players.player1;
    }

    // executa a mudanca de player
    public void changePlayer(){
        if(endGame())
            return;
            
        manager.GetComponent<Manager>().setState(GameStates.ChangePlayerScreen);

        if(!twoPlayers)
            return;

        current = (current == Players.player1 ? Players.player2 : Players.player1);
        //currentPlayerMessage.text = (current == Players.player1 ? "VEZ DO PLAYER1" : "VEZ DO PLAYER2");
        
        manager.GetComponent<Manager>().setState(GameStates.ChangePlayerScreen);
    }

    // Muda o player atual para o player desejado, de acordo com o numero informado
    public void setPlayer(int i){
        current = i == 1 ? Players.player1 : Players.player2;
    }

    // armazena os pontos dosl lances da rodada atual na matriz com os pontos
    /* Recebe:
       - score: a quant. de pinos derrubados no lance (maximo eh 10)
       - lance: o numero do lance (1 ou 2)
    */
    public void guardarPontos(int score, int lance){
        int[,] v = (currentIsPlayer1() ? p1 : p2);    // a matriz de pontos do player atual
        
        v[rodada, lance-1] = score;

        updateScore();   // atualizando o valor dos pontos das rodadas
    }
    
    // Atualiza o valor dos pontos de todas as rodadas.
    public void updateScore(){
        int[,] v = currentIsPlayer1() ? p1 : p2;

        for(int i=1; i <= gameLenght+2; i++){
            if(v[i-1, 2] != -1){ // se rodada anterior ja tem os pontos
                if(i == gameLenght){  // checando os pontos da ultima rodada
                    if(v[i, 0] == 10){  // se strike na ultima rodada
                        bool r = (v[i+1,0] != -1 && v[i+1,1] + v[i+1,0] == 10) ;

                        if(v[i,0] != -1)
                            if(r)
                                v[i,2] = v[i-1, 2] + 10 + (v[i+1,0] != -1 ? v[i+1,0] : 0) + (v[i+2,0] != -1 ? v[i+2,0] : 0);
                    }
                    else{  // nao foi strike
                        if(v[i, 0] != -1 && v[i,1] != -1){ // se ja jogou os dois lances da rodada...
                            bool r = (v[i,0] + v[i,1] != 10) || (v[i+1,0] != -1);  // nao foi spare ou foi spare e ja jogou a proxima bola
                            if(r)
                                v[i, 2] = v[i-1, 2] + v[i, 0] + v[i,1];
                        }
                    }
                }
                else if(i > gameLenght){  // rodadas extras
                    if(v[i, 0] != -1)
                        v[i,2] = v[i-1,2] + v[i, 0] + (v[i,1] != -1 ? v[i,1] : 0);
                }
                if(v[i, 0] == 10){  // STRIKE
                    int soma = -1;
                    if(v[i+1, 0] != -1){ // se a primeira bola da proxima rodada ja foi jogada...
                        if(v[i+1, 0] == 10){  // a proxima jogada foi um strike
                            soma = 10;
                            if(v[i+2,0] != -1)  // se a primeira bola dois lances a frente ja foi jogada
                                soma += v[i+2, 0];
                            else
                                soma = -1;
                        }
                        else{  // a proxima jogada nao foi um strike, entao soma-se os dois lances
                            if(v[i+1, 1] != -1) // se ja fez o segundo lance da proxima rodada...
                                soma = v[i+1, 0] + v[i+1, 1];
                            else
                                soma = -1;
                        }
                    }

                    if(soma != -1)
                        v[i,2] = v[i-1, 2] + 10 + soma;//*/
                }
                else if(v[i, 0] + v[i, 1] == 10){ // SPARE
                    if(v[i+1,0] != -1)   // se a primeira bola da proxima rodada ja foi jogada...
                        v[i, 2] = v[i-1,2] + 10 + v[i+1,0];
                }
                else{  // NAO DERRUBOU TODOS OS PINOS EM DOIS LANCES...
                    if(v[i, 0] != -1 && v[i,1] != -1) // se ja jogou os dois lances da rodada...
                        v[i, 2] = v[i-1, 2] + v[i, 0] + v[i,1];
                }
            }
        }

        /*if(currentIsPlayer1())
            placarP1.text = getText();
        else
            placarP2.text = getText();//*/

        // atualiza os placares de ambos os player no quadro que mostra a pontuacao
        if(currentIsPlayer1())
            for(int k=1; k <= gameLenght+2; k++)
                p1Board.GetComponent<Board>().setScore(k, p1[k, 0], p1[k, 1], p1[k, 2]);
        else
            for(int k=1; k <= gameLenght+2; k++)
                p2Board.GetComponent<Board>().setScore(k, p2[k, 0], p2[k, 1], p2[k, 2]);//*/

    }

    // vai para a proxima rodada
    public bool nextRodada(){
        if(rodada < gameLenght){
            rodada++;
        }
        return false;
    }

    // coloca a rodada atual para um valor especificado
    public void setRodada(int i){ this.rodada = i; }

    // retorna true se o jogo ja esta na ultima rodada
    public bool ultimaRodada(){ return rodada >= gameLenght; }

    // retorna true se o player atual fez spare na ultima rodada
    public bool spareInLastTurn(){
        int[,] v = currentIsPlayer1() ? p1 : p2;
        return v[gameLenght,0] != 10 && v[gameLenght, 0] + v[gameLenght,1] == 10;
    }

    
    /*public void setPlacar(int lance, int pinosDerrubados){
        int[,] v = (currentIsPlayer1() ? p1 : p2);

        v[rodada, lance-1] = pinosDerrubados;
    }//*/


    /*private string representacao(int score1, int score2){
        if(score1 == -1)
            return "   ";

        if(score1==10)
            return "X- ";
        else
            if(score2 == -1)
                return score1.ToString() + "- " ;
            else
                return score1.ToString() + "-" + (score1+score2 == 10 ? "/" : score2.ToString()) ;
    }

    public string getText(){
        string s;
        string s2= "      ";
        int[,] v = (currentIsPlayer1() ? p1 : p2);

        s = (currentIsPlayer1() ? "P1:   " : "P2:   ");
        for(int i=1; i <= gameLenght+2; i++){
            s += representacao(v[i, 0], v[i,1]);
            s += "    ";
            
            s2 += (v[i,2] != -1 ? v[i,2].ToString() : " ") + "   ";
        }
        s = s + "\n" + s2;
        
        return s;
    }//*/
}
