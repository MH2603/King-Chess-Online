using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Onl_GUI : MonoBehaviour
{
    [Header("------------------------- Component ----------------")]
    public TextMeshProUGUI textTurn;
    public GameObject popupWaiting;
    [Space]
    [Header("-------------------- End Popup --------------------")]
    public GameObject endPopup;
    public TextMeshProUGUI endContentText;
    public Button btnAgain;
    public Button btnExit;

    public void SetTextTurn(TypeColor typeColor)
    {
        if (typeColor == TypeColor.White)
        {
            textTurn.text = " This Turn : White";
            textTurn.color = Color.white;
        }
        else
        {
            textTurn.text = " This Turn : Black";
            textTurn.color = Color.black;
        }

    }

    public void SetWaitingPopup(bool status)
    {
        popupWaiting.SetActive(status); 
    }

    public void ShowEndPopup(bool isWin)
    {
        if (isWin)
        {
            endContentText.text = "You Win" + "\n" +
                                   "Do you want to play a again game";
        }
        else
        {
            endContentText.text = "You Lose" + "\n" +
                                   "Do you want to play a again game";
        }


        endPopup.SetActive(true);
    }

    public void OffEndPopup()
    {
        endPopup.SetActive(false);
    }
}
