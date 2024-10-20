using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TileController : MonoBehaviour
{
    [Header("---------- Component ----------------")]
    public Transform posChess;
    public GameObject modelBlack;
    public GameObject modelWhite;
    [Space]
    public GameObject modelAttack;
    public GameObject modelMove;

    [Header("------------ Stats --------------------")]
    public Vector2 id;

    [Header("------------ Status -------------------")]
    public TileStatus status;
    public ChessBase chessBase;

    public enum TileStatus
    {
        None,
        PreMove,
        PreAttack
    }

    public void Init(int x, int y)
    {
        id.x = x; id.y = y;
        this.gameObject.name = "Tile (" + id.x + "-" + id.y + ")";

        if (id.x % 2 != id.y % 2)
        {
            modelBlack.gameObject.SetActive(true);
            modelWhite.gameObject.SetActive(false);
        }
        else
        {
            modelBlack.gameObject.SetActive(false);
            modelWhite.gameObject.SetActive(true);
        }
    }

    public void OnSuggest()
    {
        if( chessBase == null)
        {
            modelAttack.gameObject.SetActive(false);    
            modelMove.gameObject.SetActive(true);

            status = TileStatus.PreMove;
        }
        else
        {
            modelAttack.gameObject.SetActive(true);
            modelMove.gameObject.SetActive(false);

            status = TileStatus.PreAttack;
        }
    }

    public void OffSuggest()
    {
        status = TileStatus.None;

        modelAttack.gameObject.SetActive(false);
        modelMove.gameObject.SetActive(false);
    }


    public void GetChess(ChessBase chess)
    {
        if (chessBase != null)
        {
            Destroy(chessBase.gameObject);  
        }

        chessBase = chess;

        chessBase.transform.parent = this.transform;
        if(chess.transform.position != posChess.position)
        {
            if (chess.chessData.name == NameChess.Knight) chess.transform.DOJump(posChess.position, 2, 1, 0.5f);
            else chess.transform.DOMove(posChess.position, 0.5f);
        }
            

        BoardManager.Ins.OffSuggestTiles();
    }

    public bool IsHaveChess()
    {
        if (chessBase != null) return true;
        else return false;
    }

    public void ResetTile()
    {
        if( IsHaveChess())
        {
            Destroy(chessBase.gameObject);
            chessBase = null;
        }
        
        status = TileStatus.None;   
    }
}
