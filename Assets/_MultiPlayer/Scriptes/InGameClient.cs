using MH;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameClient : MonoBehaviourPun
{
    [Header("------------- Component ---------------")]
    public PhotonView photonView;
    public CineManager chinemachineManager;
    public BoardManager boardManager;
    [Space]
    public Onl_GUI gui;

    [Header("------------- Status --------------------")]

    private ClientData clientData;
    public TypeColor colorTurn;
    public bool otherPlayerWantToPlayAgain = false;


    public TypeColor PlayerColor => clientData.PlayerColor;
    public Photon.Realtime.Player RivalPlayer => clientData.RivalPlayer;    

    public Action OnExitGame;

    #region( Init )

    private void Start()
    {
        gui.btnAgain.onClick.AddListener(SendPlayAgain);
        gui.btnExit.onClick.AddListener(InvokeExitGamePlayEvent);
        //gui.SetWaitingPopup(true);
    }

    //void Init()
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PlayerColor = TypeColor.White;
    //        Debug.Log("I am: Host !!!");
    //    }
    //    else
    //    {
    //        PlayerColor = TypeColor.Black;
    //        Debug.Log("I am: Client !!!");
    //    }


    //    chinemachineManager.SelectCinema(PlayerColor);

    //    boardManager.Init();

    //    gui.btnAgain.onClick.AddListener(SendPlayAgain);
    //    gui.btnExit.onClick.AddListener(ExitGamePlay);
    //    gui.SetWaitingPopup(true);

    //    if( PlayerColor == TypeColor.Black && PhotonNetwork.CountOfPlayers == 2) // player thu 2 se khoi dong tran dau
    //    {
    //        SendStartGame();   
    //    }
    //}

    public void EnterGame(ClientData clientData)
    {
        this.clientData = clientData;
        chinemachineManager.SelectCinema(PlayerColor);

        SetupToStartGame();
    }


    void SetupToStartGame()
    {
        otherPlayerWantToPlayAgain = false;

        colorTurn = TypeColor.White;

        boardManager.StartGame();

        gui.SetWaitingPopup(false);
        gui.SetTextTurn(colorTurn);
    }
    
    void SendStartGame()
    {
        SetupToStartGame();

        photonView.RPC("GetStartGame", RivalPlayer);
    }

    [PunRPC]
    public void GetStartGame()
    {
        SetupToStartGame();
    }

    #endregion

    #region( In Game Play )
    public bool IsMyTurn()
    {
        if(PlayerColor == colorTurn)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void EndMyTurn(Vector2 idChessOld, Vector2 idChessNew)
    {
        SendDataMoveChessForOtherPlayer(idChessOld, idChessNew);

        if(PlayerColor == TypeColor.Black)
        {
            colorTurn = TypeColor.White;
        }
        else
        {
            colorTurn = TypeColor.Black;
        }

        gui.SetTextTurn(colorTurn);
    }

    void SendDataMoveChessForOtherPlayer(Vector2 idChessOld, Vector2 idChessNew)
    {
        photonView.RPC("GetDataMoveChessFromOtherPlayer", RivalPlayer, idChessOld, idChessNew);
    }

    [PunRPC]
    public void GetDataMoveChessFromOtherPlayer(Vector2 idChessOld, Vector2 idChessNew)
    {
        boardManager.MoveChessOtherClient(idChessOld, idChessNew);

        colorTurn = PlayerColor;

        gui.SetTextTurn(colorTurn);

    }

    public void EndGame(bool isMeWin)
    {
        gui.ShowEndPopup(isMeWin);  
    }

    #endregion

    private void InvokeExitGamePlayEvent()
    {
        //PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene(0);

        OnExitGame?.Invoke();
    }

    public void ExitGamePlay()
    {
        boardManager.ExitBoard();
        gui.OffEndPopup();
    }

    public void SendPlayAgain()
    {
        if( !otherPlayerWantToPlayAgain)
        {
            ResetBoard();
            photonView.RPC("GetPlayAgain", RivalPlayer);
        }
        else
        {
            ResetBoard();

            SendStartGame();
        }
    }

    [PunRPC]
    public void GetPlayAgain()
    {
        if ( !otherPlayerWantToPlayAgain ) 
        {
            otherPlayerWantToPlayAgain = true;
        }
    }

    void ResetBoard()
    {
        boardManager.ResetBoard();

        gui.OffEndPopup();
        gui.SetWaitingPopup(true);
    }

}
