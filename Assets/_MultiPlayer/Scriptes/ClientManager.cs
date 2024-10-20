using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviourPun
{
    [Header("------------- Component ---------------")]
    public PhotonView photoView;
    public CineManager chinemachineManager;
    public BoardManager boardManager;
    [Space]
    public Onl_GUI gui;

    [Header("------------- Status --------------------")]

    public TypeColor colorPlayer;
    public TypeColor colorTurn;
    public bool otherPlayerWantToPlayAgain = false;

    #region( Init )

    private void Start()
    {
        Init();
    }

    void Init()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            colorPlayer = TypeColor.White;
            Debug.Log("I am: Host !!!");
        }
        else
        {
            colorPlayer = TypeColor.Black;
            Debug.Log("I am: Client !!!");
        }

        
        chinemachineManager.SelectCinema(colorPlayer);

        boardManager.Init();

        gui.btnAgain.onClick.AddListener(SendPlayAgain);
        gui.btnExit.onClick.AddListener(ExitGamePlay);
        gui.SetWaitingPopup(true);

        if( colorPlayer == TypeColor.Black && PhotonNetwork.CountOfPlayers == 2) // player thu 2 se khoi dong tran dau
        {
            SendStartGame();   
        }
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

        photonView.RPC("GetStartGame", RpcTarget.Others);
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
        if(colorPlayer == colorTurn)
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

        if(colorPlayer == TypeColor.Black)
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
        photonView.RPC("GetDataMoveChessFromOtherPlayer", RpcTarget.Others , idChessOld, idChessNew);
    }

    [PunRPC]
    public void GetDataMoveChessFromOtherPlayer(Vector2 idChessOld, Vector2 idChessNew)
    {
        boardManager.MoveChessOtherClient(idChessOld, idChessNew);

        colorTurn = colorPlayer;

        gui.SetTextTurn(colorTurn);

    }

    public void EndGame(bool isMeWin)
    {
        gui.ShowEndPopup(isMeWin);  
    }

    #endregion

    public void ExitGamePlay()
    {
        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene(0);
    }

    public void SendPlayAgain()
    {
        if( !otherPlayerWantToPlayAgain)
        {
            ResetBoard();
            photonView.RPC("GetPlayAgain", RpcTarget.Others);
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
