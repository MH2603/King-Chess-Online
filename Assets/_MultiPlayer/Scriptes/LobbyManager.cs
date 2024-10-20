using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace MH.Lobby
{

    public class LobbyManager : MonoBehaviourPunCallbacks
    {

        #region --------------- Fields ---------------------

        public TMPro.TMP_InputField NickNameInput;
        public UILobbyWindow UILobbyWindow;

        [Space]
        public UnityEvent ConnectedCloud;
        public UnityEvent JoinedLobby;
        public UnityEvent JoinedDefaultRoom;

        #endregion

        #region --------------- Properties ---------------------

        private const string DefaultRoomName = "Default Room";
        private const int MaxPlayersPerRoom = 20; // Increased max players
        [SerializeField] private ClientData _client;
        

        private Photon.Realtime.Player LocalPlayer => PhotonNetwork.LocalPlayer;

        #endregion

        #region --------------- Public Methods -----------------

        public void ConnectPhotonCloud()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogError("Disconnected from Photon with reason: " + cause.ToString());

            // Xu ly khi ko ket noi duọc. Ex: hien message box, ...
            //HandleDisconnection(cause);
        }

        // ham nay duoc goi khi ket noi voi Photon Cloud thanh cong
        public override void OnConnectedToMaster() // call by Event
        {
            ConnectedCloud?.Invoke();
            
        }

        public void JoinLobby()
        {
            if(!CanJoinLobby()) return;

            PhotonNetwork.JoinLobby();

            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text; 
        }

        public override void OnJoinedLobby() // call by event
        {
            JoinedLobby?.Invoke();

            Debug.Log($"Lobby : {PhotonNetwork.CurrentLobby.Name} - Count Room : {PhotonNetwork.CountOfRooms}");

            CreateOrJoinDefaultRoom();
        }

        public override void OnJoinedRoom()
        {
            JoinedDefaultRoom?.Invoke();

            InitClientData();
            UpdateClientDataToCloud();

            InitLobbyWindow();
            UILobbyWindow.Open();

            Debug.Log($" Joind Room {PhotonNetwork.CurrentRoom.Name} with {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        // Gọi khi một người chơi mới tham gia phòng
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($" New Player just join room : {newPlayer.NickName}");
        }

        #endregion

        #region -------------- RPC Methods --------------

        
        #endregion


        #region -------------- Private Methods -----------

        private void CreateOrJoinDefaultRoom()
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = MaxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            };
            PhotonNetwork.JoinOrCreateRoom(DefaultRoomName, roomOptions, TypedLobby.Default);

            
            //if(  PhotonNetwork.CountOfRooms == 0)
            //{
            //    PhotonNetwork.CreateRoom(DefaultRoomName);
            //    return;
            //}

            //PhotonNetwork.JoinRoom(DefaultRoomName);
            

        }

        private bool CanJoinLobby()
        {
            if ( string.IsNullOrEmpty( NickNameInput.text) ) return false;

            if (PhotonNetwork.PlayerList.Length >= 19) return false; 

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if( player.NickName == NickNameInput.text)
                {
                    Debug.Log(" Same Nick Name: " + NickNameInput.text);
                    return false;
                }
            }

            return true;    
        }

        private void InitLobbyWindow()
        {
            foreach (Player player in PhotonNetwork.PlayerListOthers)
            {
                ClientData client = GetClientDataFromCloud(player.UserId);
                if (client != null) continue;

                UILobbyWindow.CreateNewUIPlayer(client);
            }

            UILobbyWindow.SetPlayerNum(PhotonNetwork.PlayerList.Length);
            UILobbyWindow.SetLocalPlayerNickName(NickNameInput.text);
        }

        private void InitClientData()
        {
            _client = new ClientData
            {
                Id = LocalPlayer.UserId,
                Name = LocalPlayer.NickName,
                Status = ClientStatus.InLobby
            };

            
        }

        private void UpdateClientDataToCloud()
        {
            // convert to json
            string jsonData = JsonUtility.ToJson(_client);

            // Tạo đối tượng HashTable để chứa Custom Properties
            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
            roomProperties[_client.Id] = jsonData;

            // Đặt Custom Properties cho Room
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }

        private ClientData GetClientDataFromCloud(string id)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(id, out object jsonDataFromRoom))
            {
                string jsonData = (string)jsonDataFromRoom;
                ClientData clientDataFromRoom = JsonUtility.FromJson<ClientData>(jsonData);

                return clientDataFromRoom;
            }

            Debug.Log(" BUG : NOt found client data with id - " + id);
            return null;
        }
            


        #endregion
    }

}
