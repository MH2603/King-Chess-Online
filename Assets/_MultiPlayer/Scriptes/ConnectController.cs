using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectController : MonoBehaviourPunCallbacks
{

    public TMPro.TMP_InputField nameRoom;

    public UnityEvent eventConnected;
    public UnityEvent eventJoinLobby;

    public void ConnectToSever() // call by Multiplayer on main screen
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // ham nay duoc goi khi ket noi voi server thanh cong
    public override void OnConnectedToMaster() // call by Event
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() // call by event
    {
        eventJoinLobby?.Invoke();
    }

    // tao ra 1 phong moi 
    public void CreateRoom() // call by btn 
    {
        if (nameRoom.text == "") return;
        PhotonNetwork.CreateRoom(nameRoom.text);
    }

    // tham gia vao 1 phong da tao san
    public void JoinRoom() // call by button
    {
        if (nameRoom.text == "") return;
        PhotonNetwork.JoinRoom(nameRoom.text);  
    }

    // ham nay se duoc goi khi 1 player joined room
    // khoi tao 1 scene moi ten "GamePlay_Online"
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GamePlay_Online");
    }
}
