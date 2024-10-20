using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    public GameObject roomPanel;

    public void OnRoomPanel()
    {
        roomPanel.SetActive(true); 
    }
}
