using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class ChessBase : MonoBehaviour
{

    [Header("-------------- Setup by Hand ------------------")]
    public List<Vector2> listCheck_Knight;
    public List<Vector2> listCheck_King;
    
    [Header("-------------- Auto Setup ------------")]
    public TileController tileParent;
    public GameObject modelChess;
    public ChessData chessData;

    List<Vector2> listStep;
    bool isFirstStep;
    

    public void Init(ChessData newData ,TileController tileParent)
    {
        chessData = newData;    

        this.tileParent = tileParent;
        
        tileParent.GetChess(this);

        modelChess = Instantiate(chessData.modelChess, transform.position, Quaternion.identity, this.transform);
        isFirstStep = true;
        this.name = "Chess-" + chessData.name + "-" + chessData.color; 
    }

    public virtual void CheckTileToMove()
    {
        listStep = new List<Vector2>(); 

        switch (chessData.name)
        {
            case NameChess.Pawn:
                PawnCheck();
                break;
            case NameChess.Rook:
                RooKCheck();
                break;
            case NameChess.Knight:
                KnightCheck();
                break;
            case NameChess.Bishop:
                BishopCheck();
                break;
            case NameChess.Queen:
                QueenCheck();
                break;
            case NameChess.King:
                KingCheck();
                break;
        }

        BoardManager.Ins.SuggestTilesMove(listStep);
    }

    public virtual void MoveToTile(TileController newTileParent)
    {
        if(isFirstStep) isFirstStep = false;    

        tileParent.chessBase = null;

        tileParent = newTileParent;

        if (chessData.name == NameChess.Pawn)
        {
            CheckUpdatePawnToQueen();
        }

        tileParent.GetChess(this);  
    }

    void CheckUpdatePawnToQueen()
    {
        if ( (chessData.color == TypeColor.White && tileParent.id.y == 7 ) ||
             (chessData.color == TypeColor.Black && tileParent.id.y == 0 ) )
        {
            foreach (ChessData newChessData in BoardManager.Ins.chessDataSO.listDataChess)
            {
                if (newChessData.name == NameChess.Queen &&
                    newChessData.color == chessData.color)
                {
                    chessData = newChessData;
                    break;
                }
            }

            Destroy(modelChess.gameObject);

            modelChess = Instantiate(chessData.modelChess, transform.position, 
                         Quaternion.identity, this.transform);
        }
    }

    #region( Check Tile To Move)

    void AddIdStep(float x, float y)
    {
        if( x >= 0 && x <= 7 && y >= 0 && y <= 7)
        {
            listStep.Add(new Vector2(x, y));
        }
    }

    void PawnCheck()
    {
        Vector2 id = tileParent.id;

        if ( chessData.color == TypeColor.White)
        {
            if(isFirstStep)
            {
                if(CheckCanAddId( new Vector2 (id.x, id.y + 2) ) && !CheckCanAttack(new Vector2(id.x, id.y + 2)))
                    AddIdStep(id.x, id.y + 2);
            }

            if (CheckCanAddId(new Vector2(id.x, id.y + 1)) && !CheckCanAttack(new Vector2(id.x, id.y + 1)))
                AddIdStep(id.x, id.y + 1);
   
            if( CheckCanAttack(new Vector2 (id.x -1, id.y + 1)))
            {
                AddIdStep(id.x - 1, id.y + 1);
            }
            if( CheckCanAttack(new Vector2 (id.x +1, id.y + 1 )))
            {
                AddIdStep(id.x + 1, id.y + 1);
            }

        }
        else
        {
            if (isFirstStep)
            {
                if (CheckCanAddId(new Vector2(id.x, id.y - 2)) && !CheckCanAttack(new Vector2(id.x, id.y - 2)))
                    AddIdStep(id.x, id.y - 2);
            }

            if (CheckCanAddId(new Vector2(id.x, id.y - 1)) && !CheckCanAttack(new Vector2(id.x, id.y - 1)))
                AddIdStep(id.x, id.y - 1);

            if(CheckCanAttack(new Vector2(id.x - 1, id.y - 1)))
            {
                AddIdStep(id.x - 1, id.y - 1);
            }
            if( CheckCanAttack(new Vector2(id.x + 1, id.y - 1)))
            {
                AddIdStep(id.x + 1, id.y - 1);
            }

        }
    }

    void RooKCheck()
    {
        Vector2 id = tileParent.id;

        CheckStepByDirection(id, new Vector2(0,1));
        CheckStepByDirection(id, new Vector2(1,0));
        CheckStepByDirection(id, new Vector2(0,-1));
        CheckStepByDirection(id, new Vector2(-1,0));
    }

    void KnightCheck()
    {
        Vector2 id = tileParent.id;

        CheckStepByListPos(id, listCheck_Knight ); 
    }

    void BishopCheck()
    {
        Vector2 id = tileParent.id;

        CheckStepByDirection(id, new Vector2(-1,1));
        CheckStepByDirection(id, new Vector2(1,1));
        CheckStepByDirection(id, new Vector2(1,-1));
        CheckStepByDirection(id, new Vector2(-1,-1));

    }

    void QueenCheck()
    {
        Vector2 id = tileParent.id;

        BishopCheck();
        RooKCheck();
    }

    void KingCheck()
    {
        Vector2 id = tileParent.id;

        CheckStepByListPos(id, listCheck_King);
    }

    void CheckStepByDirection(Vector2 idChess, Vector2 direction)
    {
        int countStep = 0;

        Vector2 idStep = idChess;
        idStep += direction;

        while (CheckCanAddId(idStep) )
        {
            if ((BoardManager.Ins.CheckTileHaveChess(idStep)
                && BoardManager.Ins.GetTileCtrl(idStep).chessBase.chessData.color != chessData.color))
            {
                AddIdStep(idStep.x, idStep.y);
                return;
            }

            AddIdStep(idStep.x, idStep.y );
            idStep += direction;

            countStep++;
            if (countStep >= 1000) return;
        }
    }

    void CheckStepByListPos(Vector2 idChess, List<Vector2> posPlus)
    {
        for (int i=0; i<posPlus.Count; i++)
        {
            Vector2 v = idChess + posPlus[i];

            if (CheckCanAddId(v))
            {
                AddIdStep(v.x, v.y);
            }
        }
    }

    bool CheckCanAddId(Vector2 idStep)
    {
        if((idStep.x <= 7 && idStep.x >= 0 && idStep.y <= 7 && idStep.y >= 0) &&
                (!BoardManager.Ins.CheckTileHaveChess(idStep) ||
                (CheckCanAttack(idStep))
        )) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CheckCanAttack(Vector2 idStep)
    {
        if (BoardManager.Ins.CheckTileHaveChess(idStep)
               && BoardManager.Ins.GetTileCtrl(idStep).chessBase.chessData.color != chessData.color)
        {
            return true;
        }

        return false;
    }

    #endregion
}


