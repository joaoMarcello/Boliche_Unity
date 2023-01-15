/* Aqui contem a classe responsavel por fazer a transicao entre os estados do game,
   desde a transicao entre player um e player dois, a tela inicial e final  do jogo, etc

   Joao Marcello, 2021.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Enum para identificar o estado em que o jogo se encontra atualmente  */
public enum GameStates{
    Logo,   // se esta mostrando o logo (tela inicial)
    ChangePlayerScreen,  // se esta mostrando a mensagem de troca de player
    PlayBall,   // no jogo propriamente dito (o jogador lancando a bola)
    EndGame   // se esta mostrando a tela final do jogo (com os pontos e o vencendor do jogo)
}

/* Classe responsavel pela transicao de telas do jogo, musicas de fundo e outras funcoes
*/
public class Manager : MonoBehaviour
{
    public GameObject texto;   // objeto Text que indicara qual o player q esta jogando 
    public GameObject rodadaTexto;   // objeto Text que indicara o numero da rodada
    public GameObject background;   // tela escura para mostrar o numero do player e a rodada atual (so questao de design)
    public GameObject bola;     // o gameObject da bola
    public GameObject logo;     // Imagem da tela inicial (o logo do game) 
    public GameObject endGameScreen;   // tela escura para a tela final
    public GameObject pressStart;    // objeto Text para mostrar a mensagem "pressione enter" na tela inicial

    // as musicas tocadas no background do jogo
    public AudioClip theme1;   
    public AudioClip theme2;
    public AudioClip theme3;

    private Placar placar;    // objeto do tipo placar
    private controle ballController;   // o script para o controle dos movimentos da bola
    private GameStates state;     // o estado atual em que o game se encontra

    private Color p1Color;   // variavel color para o player 1
    private Color p2Color;   // variavel color para o player 2

    // variavel de controle usada para impedir q uma tecla seja contada como pressionada mais de uma vez no update
    private bool pressed = true;  

    // lista para todos os AudioSource que serao tocados
    private AudioSource[] lista;

    // variaveis usadas para controlar o componente alfa da cor da mensagem "pressione enter" da tela inicial do jogo
    private float r=0;
    private float alfa = 1.0f;

    /*
        CONTEUDO DE CADA AUDIOSOURCE
    0 - crowd uau
    1 - slow clap
    2 - change player ding
    3 - final play
    4 - in game song  (AudioClip que pode variar)
    5 - logo/menu principal 
    6 - end game ding
    7 - empate ding
    */

    // Start is called before the first frame update
    void Start()
    {
        lista = GetComponents<AudioSource>();  // atribuido todos os AudioSource a uma lista 
        lista[4].loop = true;  // a musica do fundo tocara em loop
        lista[5].loop = true;  // a musica da tela inicial tocara em loop

        placar = bola.GetComponent<Placar>();   // a objeto da classe Placar associado a bola
        ballController = bola.GetComponent<controle>();  // o objeto da classe controle associado a bola
        
        p1Color = new Color(1,0,0,1);   // a cor do player 1 - vermelho
        p2Color = new Color(0,0,1,1);   // cor do player 2 - azul

        background.SetActive(true);  // destivando a tela preta
        setText("Player1");    
        texto.SetActive(true);

        setState(GameStates.Logo);  // o estado inicial mostra o logo do game
        endGameScreen.SetActive(false);
        pressStart.SetActive(true);

        Cursor.visible = false;  // desativando o cursor do mouse
    }

    // Update is called once per frame
    void Update()
    {   
        if(Input.GetKey("escape"))  // se pressionar a tecla 'esc' sai do jogo
            Application.Quit();
            
        if(pressed && !pressSomeKey())  // se as teclas usadas nao estao liberadas e 'presses' eh true
            pressed = false;   // libera as teclas

        if(state == GameStates.ChangePlayerScreen){  // ESTADO = MUDANCA DE PLAYER
            ballController.pause(true);   // pausa os movimentos da bola

            if(!pressed && pressSomeKey()){  // se apertou o botao para avancar
                pressed = true;
                setState(GameStates.PlayBall);  // muda o estado para PLAYBALL (o player jogando a bola)
            }
        }
        else if(state == GameStates.EndGame){  // ESTADO = FIM DE JOGO
            if(!pressed && pressSomeKey()){   // se apertou a tecla para avancar, o estado muda para a tela inicial
                pressed = true;
                setState(GameStates.Logo);  // mudando o estado para a tela inicial
                lista[1].Play();    // toca a musica da tela inicial
                placar.InitGame();   // resetando o placar
                ballController.Reset();  // resetando a bola
                ballController.resetaPinos();   // resetando os pinos
                ballController.pause(true);   // pausa os movimentos da bola
                placar.p1Board.GetComponent<Board>().clear();  // resetando os mostradores do pontos do player 1
                endGameScreen.SetActive(false);
                if(placar.twoPlayers)  // se o jogo esta habilitado para dois players
                    placar.p2Board.GetComponent<Board>().clear(); // resetando os mostradores do pontos do player 1
            }
        }
        else if(state == GameStates.Logo){  // ESTADO = LOGO/TELA INICIAL

            // fazendo as alteracoes do componente alfa do Texto "Pressione Enter"
            r += 5.0f * Time.deltaTime;
            alfa = 0.8f + 0.7f * Mathf.Sin(r);
            pressStart.GetComponent<Text>().color = new Color(1,0,0,alfa);

            ballController.pause(true); // pausando os movimentos da bola

            if(!pressed && pressSomeKey()){    // se pressionou a tecla para avancar, o estado muda para "MUDANCA DE PLAYER"
                lista[5].Stop();  // parando a musica da tela inicial

                // variavel temporaria para determinar qual sera a musica de fundo
                float r = Random.Range(0.0f, 100.0f);
                if(r <= 33.0f) lista[4].clip = theme1;
                else if(r <= 66.0f) lista[4].clip = theme2;
                else lista[4].clip = theme3;

                lista[4].Play();  // tocando a musica de fundo
                lista[0].Play();  // som de aplausos
                pressStart.SetActive(false);   // ocultando a frase de "Pressione Enter"
                pressed = true;   
                logo.SetActive(false);  // ocultando o logo
                // mudando o estado para "MUDANCA de PLAYER"
                setState(placar.twoPlayers || true ? GameStates.ChangePlayerScreen : GameStates.PlayBall); 
            }
        }
    }

    /* Muda o estado atual do game.
       Recebe:
            - st: o estado desejado
    */
    public void setState(GameStates st){
        this.state = st;
        pressed = true;
        
        if(st == GameStates.Logo){ // TELA INICIAL
            turnOff();  // desativando os elementos da tela de mudanca de player
            placar.hidePlacar(); // ocultando os quadro com o placar
            ballController.pause(true); // pausando a bola
            logo.SetActive(true);     // ocultando o logo
            pressStart.SetActive(true);  // ocultando a mensagem "Pressione enter"
            lista[5].Play();    // tocando a musica da tela inicial
        }
        else if(st == GameStates.PlayBall){   // JOGO
            turnOff(); // desativando os elementos da tela mudanca de player
            ballController.pause(false);  // pausando a bola
            placar.showPlacar();    // mostra os quadros com o placar
        }
        else if(st == GameStates.ChangePlayerScreen){   // TELA DE MUDANCA DE PLAYER
            turnOn();                 // ativando os elementos da tela mudanca de player
            placar.hidePlacar();                 // ocultando os quadros com o placar
            setText(placar.currentIsPlayer1() ? "Player 1" : "Player 2");  // muda o texto informando qual o player atual
            setRodada();                     // atualizando o texto com a rodada atual
            if(placar.ultimaRodada())         // se esta na ultima rodada...
                lista[3].Play();              // toca o som da ultima rodada
            else
                lista[2].Play();               // senao, toca o som da proxima rodada
        }
        else if(st == GameStates.EndGame){      // FIM DE JOGO
            // ativando os componentes da tela de mudanca de player (mas usara somente o objeto Text que informava o player atual,
            // os outros gameObject serao desativados
            turnOn();    

            lista[4].Stop();   // para a musica de fundo

            rodadaTexto.SetActive(false); 
            background.SetActive(false);

            ballController.pause(true);   // pausando a bola

            placar.hidePlacar();   // ocultando os quadros com a pontuacao

            endGameScreen.SetActive(true);   // mostrando a tela de fim de jogo
            
            // definindo o texto do final de jogo. Varia dependendo se houve vencedor ou empate
            string s = "FIM DE JOGO\n\n";

            if(placar.twoPlayers){   // se o jogo esta no modo para dois players
                s += "Jogador 1:  " + placar.getTotalScore()[0] + " pontos\n";
                s += "Jogador 2:  " + placar.getTotalScore()[1] + " pontos\n";

                if(placar.getTotalScore()[0] == placar.getTotalScore()[1]){  // se os players possuirem a mesma pontuacao (empate)
                    s+= "\nEMPATE";
                    lista[7].Play();  // toca o som de empate
                }
                else{   // senao, checa a maior pontuacao 
                    lista[6].Play();
                    s += "\nVENCEDOR\n" + (placar.getTotalScore()[0] > placar.getTotalScore()[1] ? "Jogador 1" : "Jogador 2");
                } 
                lista[0].Play();    // toca o som do vencedor
            }
            else{   // jogo esta no modo pra um player
                s += "Pontuacao\n" + placar.getTotalScore()[0];

                lista[6].Play();   // som de fim jogo
                if(placar.getTotalScore()[0] > 100)    // se a pontuacao total for maior que 100 pontos...
                    lista[0].Play();                // ...toca o som de aplausos forte
                else
                    lista[1].Play();               // senao, toca o som de aplausos fracos
            }
            
            setText(s);  // atribuido o texto de fim de jogo para o gameOnject Text
            texto.GetComponent<Text>().color = new Color(1,1,1,1);   // mudando a cor do texto para branco
        }
        else turnOn();

        
    }

    /* Desativa ou Ativa elementos da tela de mudanca do player  */
    private void on_off(bool v){
        rodadaTexto.SetActive(v);
        background.SetActive(v);
        texto.SetActive(v);
    }

    /* Desativa elementos da tela de mudanca do player  */
    public void turnOff(){
        on_off(false);
    }

    /* Ativa elementos da tela de mudanca de player  */
    public void turnOn(){
        on_off(true);
    }

    // Retorna true se a tecla espaco ou enter estiverem pressionadas
    bool pressSomeKey(){
        return Input.GetKey("space") || Input.GetKey("return");
    }

    // Muda o texto do player atual para o que esta informado em 's', alterando a cor de acordo com o player atual
    public void setText(string s){
        texto.GetComponent<Text>().text =  s;
        texto.GetComponent<Text>().color = placar.currentIsPlayer1() ? p1Color : p2Color;

    }

    // Atualiza o texto da rodada atual (altera tambem a cor de acordo com o player atual)
    public void setRodada(){
        rodadaTexto.GetComponent<Text>().text = placar.getRodada() != placar.getLenGame() ? "Rodada " + placar.getRodada() : "Rodada Final";
        rodadaTexto.GetComponent<Text>().color = placar.currentIsPlayer1() ? p1Color : p2Color;
    }

}
