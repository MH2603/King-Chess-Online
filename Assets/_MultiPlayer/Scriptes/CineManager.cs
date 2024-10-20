using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CineManager : MonoBehaviour
{
    public CinemachineVirtualCamera cineWhite;   
    public CinemachineVirtualCamera cineBlack;
    
    public void SelectCinema(TypeColor typeColor)
    {
        cineWhite.Priority = 0;
        cineBlack.Priority = 0;

        switch (typeColor)
        {
            case TypeColor.White:
                cineWhite.Priority = 1;
                break;
            case TypeColor.Black:
                cineBlack.Priority = 1;
                break;
        }
    }
}
