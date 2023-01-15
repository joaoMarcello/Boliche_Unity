/* Classe para implementacao dos movimentos da bola no jogo de boliche, entre outras funcoes.

   Joao Marcello, 2021.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* os possiveis estados em que a bola se encontra */
public enum BallStates{
    movingSide,    // o jogador esta movendo a bola de um lado pro outro
    choosingAngle,   // o jogador esta escolhendo a direcao de lancamento da bola
    paused          // movimentos da bola travados
}

public class controle : MonoBehaviour
{
    private Rigidbody rb;
    private BallStates currentState;
    private bool play;
    private bool keyPressed;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject frontArrow;
    public GameObject pinos;

    public Text my_text;
    public Text message;
    public Text strikeRowMessage;

    private AudioSource source;

    private AudioSource[] audioSources;

    // os sons utilizados pela bola
    public AudioClip ballRolling;
    public AudioClip strike;
    public AudioClip voice_strike;
    public AudioClip voice_spare;
    public AudioClip crowd_aw;
    public AudioClip crowd_cheer;
    public AudioClip crowd_fiufiu;
    public AudioClip crowd_slow_clap;
    public AudioClip crowd_uau;

    public ControleCamera camera;

    public float power;     // forca de lancamento que sera aplicado na bola
    private float r = 0.0f;
    public float arrowSpeed = 1;    // velocidade do movimento da flecha que decide a direcao do lancamento

    private int tentativas;    // o lance atual (se eh a primeira ou segunda bola da rodada)

    private bool hit;  // para saber se ja tocou o som de strike

    private int score;    // quantidade de pinos derrubados no lance
    private int totalScore=0;    // a soma da quanti. de pinos derrubados em ambos os lances

    private float time_collision;  // armazenara o tempo decorrido desde q a bola colidiu com os pinos
    private bool colidiu;      // flag que informa se a bola ja colidiu com os pinos

    private Vector3 initialPosition = Vector3.zero;

    private Placar placar;     // armazena os pontos da partida

    public GameObject manager;   // faz o controle das transicoes de tela

    // Start is called before the first frame update
    void Start()
    {
        if(initialPosition == Vector3.zero)
            initialPosition = transform.position;

        rb = GetComponent<Rigidbody>();   // Rigidbody associado a bola
        source = GetComponent<AudioSource>();   

        audioSources = GetComponents<AudioSource>();

        currentState = BallStates.movingSide;
        play = false;
        frontArrow.SetActive(false);
        score = 0;
        keyPressed = true;
        time_collision = 0.0f;
        colidiu = false;
        hit = false;

        tentativas = 1;  // lance atual (se eh a primeira ou a segunda bola lancada)

        message.gameObject.SetActive(false);
        strikeRowMessage.gameObject.SetActive(false);

        placar = GetComponent<Placar>();  // o componente placar da bola
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(currentState == BallStates.paused)   // se a bola esta pausada, nao eh possivel fazer nada
            return;


        my_text.text = "";//score.ToString() + " " + totalScore.ToString();

        // qual movimento a bola estah executando, depende do valor do estado atual
        if(currentState == BallStates.movingSide)
            movingSideUpdate();
        else if(currentState == BallStates.choosingAngle)
            choosingAngleUpdate();

        // se a bola colidiu com a parede no fundo da pista...
        if(colidiu || transform.position.z < -25){
            if(!colidiu){
                colidiu = true;
                tentativas++;
            }

            time_collision += Time.deltaTime;

            // se passou de 4 segundos desde que derrubou os pinos...
            if(time_collision >= 4.0f && time_collision < 1000.0f){
                totalScore += score;
                message.gameObject.SetActive(true);
                
                bool q = Random.Range(0.0f, 10.0f) >= 5.0f ? true : false;

                // estah na primeira tentativa (eh igual a dois porque o valor atualiza imediatamente quando a rodada termina)
                if(tentativas == 2){
                    placar.guardarPontos(score, 1);   // salva a quantidade de pinos derrubados no primeiro lance
                    
                    if(score == 10){   // se foi strike
                        strikeRowMessage.gameObject.SetActive(true);
                        message.text = "STRIKE";
                        int k = placar.strikeRow(placar.getRodada());
                        strikeRowMessage.text = strikeName(k);
                    }
                    else   // nao foi strike...
                        message.text = score.ToString() + (score != 1 ? "  PINOS" : "  PINO");

                    // tocando o som de aplausos conforme a quant. de pinos derrubados
                    if(score == 0)
                        audioSources[1].clip = crowd_aw;
                    else if(score <= 4)
                        audioSources[1].clip = crowd_slow_clap;
                    else if(score == 10){
                        audioSources[0].clip = voice_strike;
                        audioSources[0].Play();
                        audioSources[1].clip = crowd_uau;
                        
                    }
                    else
                        audioSources[1].clip = q ? crowd_cheer : crowd_fiufiu;

                    
                }
                else{  // Estah na segunda tentativa

                    placar.guardarPontos(score, 2);   // salvando a quant. de pinos derrubados no segundo lance

                    if(totalScore == 10)   // se a quant. de pinos derrubados no total foi 10 (juntando ambos os lances) - Spare
                        message.text = "SPARE";
                    else
                        message.text = score.ToString() + (score != 1 ? "  PINOS" : "  PINO");

                    // tocando o som de aplausos conforme a quant. de pinos derrubados
                    if(totalScore == 0)
                        audioSources[1].clip = crowd_aw;
                    else if(totalScore <= 4 || score == 0)
                        audioSources[1].clip = crowd_slow_clap;
                    else if(totalScore == 10){
                        audioSources[0].clip = voice_spare;
                        audioSources[0].Play();
                        audioSources[1].clip = crowd_uau;
                    }
                    else
                        audioSources[1].clip = q ? crowd_cheer : crowd_fiufiu;

                    
                }

                audioSources[1].Play();
                time_collision = 50000.0f;
            }

            // se ja passou 4 segundos desde que a bola derrubou os pinos
            if(time_collision-50000.0f >= 4.0f){
                placar.updateScore();    // atualiza a pontuacao da rodada
                
                if(score < 10 && tentativas<=2){  // nao conseguiu strike, entao vai pro segundo lance
                    Reset();                  // poe a bola na posicao inicial       
                    removePinosCaidos();         // remove os pinos que foram derrubados
                    tentativas = 3;             
                    
                    // se esta na ultima rodada do jogo (caso o player tenha feito strike na ultima e penultima rodada), ou
                    // o ultimo turno foi um spare e esta na ultima rodada
                    if(placar.getRodada() == placar.getLenGame() + 2 || 
                    (placar.spareInLastTurn() && placar.getRodada() == placar.getLenGame() + 1) )
                        if(placar.currentIsPlayer1() ){     // se o player atual eh o player um...
                            placar.updateScore();          // atualiza a pontuacao da rodada
                            placar.player1Ended();          // player um finalizou todos os seus lances
                            Reset();                      // reseta a posicao da bola
                            resetaPinos();                 // reseta a posicao dos pinos
                            
                            score = 0;
                            totalScore = 0;
                            if(placar.twoPlayers)
                                placar.setRodada(placar.getLenGame());   // voltando a rodada atual para a ultima rodada do player 2
                            placar.changePlayer();             // muda para o player 2
                        }
                        else{
                            placar.player2Ended();     // player dois finalizou todos os seus lances
                        }//*/
                }
                else{   // fez o strike ou estah na segunda tentativa
                    

                    if(placar.ultimaRodada()){  // se eh a ultima rodada

                        if(placar.getRodada() < placar.getLenGame() + 2  && tentativas <=2 && !placar.spareInLastTurn()){
                            placar.setRodada(placar.getRodada() + 1);
                        }
                        else if(placar.getRodada() == placar.getLenGame() && tentativas > 2 && placar.spareInLastTurn()){
                            placar.setRodada(placar.getRodada() + 1);
                        }//*/
                        else{
                            if(placar.currentIsPlayer1()){
                                placar.player1Ended();
                                placar.updateScore();
                                
                                placar.setRodada(placar.getLenGame());
                                placar.changePlayer();
                            }
                            else
                                placar.player2Ended();
                        }
                    }
                    else{   // ainda nao eh a ultima rodada
                        if(placar.currentIsPlayer1() && placar.twoPlayers)
                            placar.changePlayer();
                        else{
                            placar.nextRodada();
                            placar.changePlayer();
                        }
                    }

                    Reset();
                    resetaPinos();
                    score = 0;
                    totalScore = 0;
                    tentativas = 1;
                }

                placar.updateScore();

                if(placar.endGame()){
                    pause(true);
                    manager.GetComponent<Manager>().setState(GameStates.EndGame);
                }
            }
        }

    }

    /* Retona para o strike conforme a quantidade de strikes consecultivos informada  */
    string strikeName(int r){
        if(r == 2) return "Double";
        if(r == 3) return "Turkey";
        if(r == 4) return "Fourth";
        if(r == 5) return "Fifth";
        if(r == 6) return "Six-pack";
        if(r == 7) return "Seven-pack";
        if(r == 8) return "Eight in a row";
        if(r == 9) return "Nine in a row";
        if(r == 10) return "Ten in a row";
        if(r == 11) return "Eleven in a row";
        if(r == 12) return "PERFECT";
        else return "";
    }

    // Pausa o movimentos da bola se true for passado como parametro.
    public void pause(bool p){
        if(p){
            currentState = BallStates.paused;
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);
        }
        else{
            currentState = BallStates.movingSide;
            leftArrow.SetActive(true);
            rightArrow.SetActive(true);
        }
    }

    // Iniicio de colisao
    void OnCollisionEnter(Collision obj){
        if(!hit && obj.gameObject.tag == "Pino"){  // se colidiu com um pino...
            source.Stop();
            source.clip = strike;
            source.Play();
            hit = true;
        }

        if(!colidiu && obj.gameObject.tag == "Parede"){
            //colidiu = true;
            //tentativas++;
        }
    }

    // Coloca a bola na posicao inicial
    public void Reset(){
        message.gameObject.SetActive(false);
        strikeRowMessage.gameObject.SetActive(false);

        transform.position = initialPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        Start();
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
        camera.Reset();
    }

    // Retorna todos os pinos do jogo em uma lista de GameObject
    List<GameObject> getTodosPinos(){
        List<GameObject> lista = new List<GameObject>();

        foreach(Transform child in pinos.transform){
            if(child.tag == "Pino")
                lista.Add(child.gameObject);
        }

        return lista;
    }


    // Remove os pinos caidos do jogo
    void removePinosCaidos(){
        List<GameObject> lista = getTodosPinos();   // todos os pinos

        foreach(GameObject obj in lista){        // percorrendo cada pino da lista
            PinoScript s = obj.GetComponent<PinoScript>();
            if(s.isDown())         // se o pino caiu...
                obj.SetActive(false);    // oculta o pino
            else
                s.Reset();    // senao, reseta o pino
        }
    }

    // Coloca todos os pinos de volta a posicao inicial, mesmo que este ja tenha caido
    public void resetaPinos(){
        List<GameObject> lista = getTodosPinos();

        foreach(GameObject obj in lista){
            PinoScript s = obj.GetComponent<PinoScript>();
            s.Reset();
            obj.SetActive(true);
        }
    }

    // Funcao para mover a bola de um lado para outro
    private void movingSideUpdate(){
        if(Input.GetKey("a") && transform.position.x < 2.0f){
            transform.Translate(0.1f, 0.0f, 0.0f);
            rightArrow.SetActive(true);
            if (transform.position.x >= 2.0f){leftArrow.SetActive(false);}
        }
        if(Input.GetKey("d") && transform.position.x > -2.0f){
            transform.Translate(-0.1f, 0.0f, 0.0f);
            leftArrow.SetActive(true);
            if (transform.position.x <= -2.0f){rightArrow.SetActive(false);}
        }
        if(Input.GetKey("w")){
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);
            frontArrow.SetActive(true);
            currentState = BallStates.choosingAngle;
            keyPressed = true;
        }
    }

    // Funcao que move automaticamente a seta que direcionara a bola e executa  o movimento e lancamento da bola
    private void choosingAngleUpdate(){
        frontArrow.transform.Rotate(new Vector3(0,0,arrowSpeed));
        r = frontArrow.transform.localRotation.eulerAngles.z;

        if (arrowSpeed > 0 && r >= 180 + 25){
            arrowSpeed *= -1;
        }
        if (arrowSpeed < 0 && r <= 180 - 25){
            arrowSpeed *=-1;
        }

        if(!Input.GetKey("w") && keyPressed){
            keyPressed = false;
        }

        if(Input.GetKey("w") && !play && !keyPressed){   // se apertou w, lanca a bola de acordo com a posicao atual da seta
            transform.Rotate(0,-frontArrow.transform.localRotation.eulerAngles.z-180,0);
            rb.AddForce(transform.forward * Mathf.Abs(power) * -1);

            play = true;
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);
            frontArrow.SetActive(false);
            source.clip = ballRolling;
            source.Play();
        }
        else if (Input.GetKey("backspace")){   // se pressionar backspace, volta pro estado "movendo a bola esq/dir"
            leftArrow.SetActive(true);
            rightArrow.SetActive(true);
            frontArrow.SetActive(false);
            currentState = BallStates.movingSide;
        }
    }

    // funcao usada pelos pinos para somar a pontuacao da rodada atual
    public void addScore(){ 
        score++;
    }

    // informa se a bola ja foi lancada ou nao
    public bool played(){ return play; }
}
