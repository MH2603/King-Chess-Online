using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public enum BoardStatus
{
    Init,
    Prepare,
    Playing,
    End
}

public class BoardManager : MonoBehaviour
{

    public static BoardManager Ins;    

    [Header("------------- Inspector --------------")]
    public TileController tileCtrlPrefabs;
    public Transform TileParent;
    public InGameClient clientManager;

    [Header("-------------- Status -------------------")]
    public BoardStatus status;
    public TileController tileChosing;
    public List<TileController> listTileCtrl;

    [Header("--------------- Asset -------------------")]
    public ChessDataSO chessDataSO;
    public ChessBase chessCtrl;
    public LayerMask layerTile;
    private void Awake()
    {
        Ins = this;

        Init();
    }

    #region( Init )
    public void Init()
    {
        // tao ra 64 o co. Trang va den xen ke
        SpawnTile();

        // tao ra 16 quan co moi ben
        SpawnAllChess();

        // doi trang thai game thanh chuan bi
        status = BoardStatus.Prepare;

        TileParent.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        ResetBoard();   

        status = BoardStatus.Playing;
        TileParent.gameObject.SetActive(true);
    }
    
    void SpawnTile()
    {
        for (int i=0;i < 8;i++)
        {
            for (int j=0; j < 8; j++)
            {
                TileController newTile = Instantiate(tileCtrlPrefabs, new Vector3(i, 0, j), Quaternion.identity, this.TileParent);
                newTile.Init(i, j);

                listTileCtrl.Add(newTile);  
            }
        }
    }


    void SpawnAllChess()
    {
        for (int i=0; i< chessDataSO.listDataChess.Count; i++)
        {
            SpawnTypeChess(chessDataSO.listDataChess[i]);
        }
    }

    void SpawnTypeChess(ChessData chessData)
    {

        for (int i=0; i<chessData.listPosInit.Count; i++)
        {
            Vector3 pos = new Vector3(chessData.listPosInit[i].x, 0, chessData.listPosInit[i].y);

            ChessBase newChess = Instantiate(chessCtrl, pos, Quaternion.identity);

            newChess.Init(chessData, GetTileCtrl(chessData.listPosInit[i]) );
        }
    }

    #endregion

    private void Update()
    {
        if (status != BoardStatus.Playing) return;

        if( clientManager.IsMyTurn() &&
            Input.GetMouseButtonDown(0))
        {
            CheckChoseChessByRay();
        }
    }

    public TileController GetTileCtrl(Vector2 newId)
    {
        for (int i=0; i < listTileCtrl.Count; i++)
        {
            if(newId.x == listTileCtrl[i].id.x && newId.y == listTileCtrl[i].id.y)
            {
                return listTileCtrl[i];
            }
        }

        return null;
    }

    public bool CheckTileHaveChess(Vector2 id)
    {
        if( GetTileCtrl(id )  && GetTileCtrl(id).IsHaveChess())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CheckToChoseTile(TileController tile)
    {
        if( tileChosing == null &&
            CheckTileHaveChess(tile.id) &&
            tile.chessBase.chessData.color != clientManager.PlayerColor)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    void CheckChoseChessByRay()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, layerTile) )
        {
            if (hit.transform.TryGetComponent<TileController>(out TileController tileCtrl) &&
                CheckToChoseTile(tileCtrl) ) 
            {
                ChoseTile(tileCtrl);
                return;
            }
        }
        
    }

    void ChoseTile(TileController newTile)
    {
        
        if (tileChosing == null && newTile.IsHaveChess() ) // chon 1 quan co tren ban
        {
            tileChosing = newTile;
            tileChosing.chessBase.CheckTileToMove();
            Debug.Log("--> Step 01 ");
            return;
        }

        if( tileChosing != null &&
            newTile.status != TileController.TileStatus.None) // di chuyen quan co da chon
        {
            Debug.Log("Client : " + clientManager);
            Debug.Log("Old Tile : " + tileChosing.id);
            Debug.Log("New Tile : " + newTile.id);

            clientManager.EndMyTurn(tileChosing.id, newTile.id); // MH: gui data move cho other player

            CheckToEnd(newTile.id); // kiem tra xem co an dc King ko? de end game

            tileChosing.chessBase.MoveToTile(newTile); // Di chuyen quan co
            
            tileChosing = null;
            return;
        }

        OffSuggestTiles(); // tat highlight nuoc di
        tileChosing = null;
    }

    public void MoveChessOtherClient(Vector2 idOld, Vector2 idNew) // call by ClientManager
    {
        CheckToEnd(idNew);
        GetTileCtrl(idOld).chessBase.MoveToTile(GetTileCtrl(idNew));
    }


    #region( Suggest Move Step )
    public void SuggestTilesMove(List<Vector2> listIdSuggest)
    {
        for (int i=0; i < listIdSuggest.Count; i++)
        {
            CallSuggestTile(listIdSuggest[i]);
        }
    } 

    void CallSuggestTile(Vector2 id)
    {
        for (int i=0; i<listTileCtrl.Count; i++) 
        {
            if (listTileCtrl[i].id.x == id.x &&
                listTileCtrl[i].id.y == id.y)
            {
                listTileCtrl[i].OnSuggest();
            }
        
        }
    }

    public void OffSuggestTiles() //call by TileCtrl
    {
        tileChosing = null;

        for (int i=0; i<listTileCtrl.Count; i++)
        {
            listTileCtrl[i].OffSuggest();   
        }
    }

    #endregion

    void CheckToEnd(Vector2 idTileToMove)
    {
        TileController tileCtrl = GetTileCtrl(idTileToMove);
        if ( tileCtrl != null &&
             tileCtrl.IsHaveChess() &&
             tileCtrl.chessBase.chessData.name == NameChess.King)
        {
            EndGame(tileCtrl.chessBase.chessData.color);
        }
    }

    void EndGame(TypeColor colorOfWinner)
    {
        status = BoardStatus.End;

        if (colorOfWinner == clientManager.PlayerColor)
        {
            Debug.Log("--> Win !!!");
            clientManager.EndGame(true);
        }
        else
        {
            clientManager.EndGame(false);
        }
    }


    public void ResetBoard()
    {
        status = BoardStatus.Init;
        tileChosing = null;
        foreach (TileController tileCtrl in listTileCtrl)
        {
            tileCtrl.ResetTile();   
        }

        SpawnAllChess();

        status = BoardStatus.Prepare;
    }

    public void ExitBoard()
    {
        status = BoardStatus.Prepare;
        TileParent.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    [Button("Spawn Tile")]
    public void Call_SpawnTile()
    {
        SpawnTile();
    }

    [Button("Clear Tile")]
    public void ClearTile()
    {
        for (int i=0; i< listTileCtrl.Count; i++)
        {
            if (listTileCtrl[i])
            {
                DestroyImmediate(listTileCtrl[i].gameObject);
            }
        }

        listTileCtrl.Clear();
    }


    [Button("Spawn Chess")]
    public void Call_SpawnChess() 
    {
        SpawnAllChess();
    }
#endif
}


