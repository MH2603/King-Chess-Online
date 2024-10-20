using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ChessDataSO")]
public class ChessDataSO : ScriptableObject
{
    public List<ChessData> listDataChess;
}

[System.Serializable]
public struct ChessData
{
    public TypeColor color;
    public NameChess name;
    public List<Vector2> listPosInit;

    [Space]
    public GameObject modelChess;

}

public enum NameChess
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public enum TypeColor
{
    Black,
    White
}