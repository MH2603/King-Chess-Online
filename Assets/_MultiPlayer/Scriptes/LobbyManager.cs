using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace MH.Lobby
{

    public class LobbyManager : MonoBehaviourPunCallbacks
    {

        #region --------------- Inspector ---------------------

        public TMPro.TMP_InputField NickNameInput;
        [Space]
        public UILobbyWindow UILobbyWindow;
        public InGameClient InGameClient;

        [Space]
        public UnityEvent ConnectedCloud;
        public UnityEvent JoinedLobby;
        public UnityEvent JoinedDefaultRoom;

        #endregion

        #region --------------- Properties ---------------------

        private const string DefaultRoomName = "Default Room";
        private const string ClientDataName = "ClientData";
        private const int MaxPlayersPerRoom = 20; // Increased max players
        [SerializeField] private ClientData localClient;
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

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
            Debug.Log("Connected Cloud !!!");
        }

        public void JoinLobby()
        {
            if(!CanJoinLobby()) return;

            PhotonNetwork.JoinLobby();
            
        }

        public override void OnJoinedLobby() // call by event
        {
            JoinedLobby?.Invoke();

            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;

            InitClientData();
            UpdateClientDataToCloud();

            Debug.Log($" Joined Lobby : {PhotonNetwork.CurrentLobby.Name} - Count Room : {PhotonNetwork.CountOfRooms}");

            CreateOrJoinDefaultRoom();
        }

        public override void OnJoinedRoom()
        {
            JoinedDefaultRoom?.Invoke();

            Debug.Log($" Joind Room ; {PhotonNetwork.CurrentRoom.Name} - Count Clients :  {PhotonNetwork.CurrentRoom.PlayerCount}");

            //PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;

            //InitClientData();
            //UpdateClientDataToCloud();

            InitLobbyWindow();
            UILobbyWindow.Open();

            InGameClient.OnExitGame += SendExitGameForRival;
        }

        // Gọi khi một người chơi mới tham gia phòng
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($" New Player just join room : {newPlayer.NickName}");

            ClientData client = GetClientDataFromCloud(newPlayer);
            if (client == null) return;

            UILobbyWindow.CreateNewUIPlayer(client, () => SendInvite(newPlayer.UserId));
            //UILobbyWindow.CreateNewUIPlayer(client, () => TestSendInvite() );

            UILobbyWindow.SetPlayerNum(PhotonNetwork.PlayerList.Length);

        }

        // Gọi khi một người chơi rời khỏi phòng
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($" LOG: {otherPlayer.NickName} just left room ");

            UILobbyWindow.RemoveUIPlayer(otherPlayer.UserId);
            //UILobbyWindow.CreateNewUIPlayer(client, () => TestSendInvite() );

            UILobbyWindow.SetPlayerNum(PhotonNetwork.PlayerList.Length);

        }

        #endregion

        #region -------------- RPC Methods --------------


        public void SendUpdateClient()
        {
            photonView.RPC(nameof(UpdateClientStatusViaId), RpcTarget.Others);
        }

        [PunRPC]
        public void UpdateClientStatusViaId(PhotonMessageInfo info)
        {
            ClientData client = GetClientDataFromCloud(info.Sender);
            if (client == null) return;

            UILobbyWindow.UpdateUIOtherClient(client);  
        }

        public void SendInvite(string clientId)
        {
            Photon.Realtime.Player targetPlayer = GetClientByUserId(clientId);
            photonView.RPC("ListenInvite", targetPlayer);
        }

        //public void TestSendInvite()
        //{
        //    photonView.RPC("ListenInvite", RpcTarget.Others);
        //}

        [PunRPC]
        public void ListenInvite(PhotonMessageInfo info)
        {
            Photon.Realtime.Player inviter = info.Sender;
            ClientData client = GetClientDataFromCloud(inviter);
            string inviterName = info.Sender.NickName;

            Debug.Log($" LOG : You take invite from {inviterName}");

            UILobbyWindow.CreateNewUIInvition( client, 
                                                () => AcceptInviteFromInviter(inviter), 
                                                () => RejectInviteFromInviter(inviter));

        }

        public void AcceptInviteFromInviter(Photon.Realtime.Player inviter)
        {
            ClientData inviterData = GetClientDataFromCloud(inviter);
            if (inviterData.Status == ClientStatus.InGame) return;

            // Set data for cilent before in game
            localClient.Status = ClientStatus.InGame;
            localClient.PlayerColor = TypeColor.White;
            localClient.RivalPlayer = inviter;

            UpdateClientDataToCloud();
            SendUpdateClient();

            photonView.RPC(nameof(ListenAcceptCallback), inviter);

            UILobbyWindow.Close();
            InGameClient.EnterGame(localClient);
        }

        public void RejectInviteFromInviter(Photon.Realtime.Player inviter)
        {
            photonView.RPC(nameof(ListenRejectCallback), inviter);
        }


        [PunRPC]
        public void ListenAcceptCallback(PhotonMessageInfo info)
        {
            Debug.Log($" LOG: {info.Sender.NickName} accepted your invite - Status : {localClient.Status.ToString()}");

            if (localClient.Status == ClientStatus.InGame) return;

            localClient.Status = ClientStatus.InGame;
            localClient.PlayerColor = TypeColor.Black;
            localClient.RivalPlayer = info.Sender;

            UpdateClientDataToCloud();
            SendUpdateClient();

            UILobbyWindow.Close();
            InGameClient.EnterGame(localClient);
        }

        [PunRPC]
        public void ListenRejectCallback(PhotonMessageInfo info)
        {
            Debug.Log($" LOG: {info.Sender.NickName} reject your invite - Status : {localClient.Status.ToString()}");
        }

        public void ExitChessGame()
        {
            localClient.Status = ClientStatus.InLobby;

            UpdateClientDataToCloud();
            SendUpdateClient();

            InGameClient.ExitGamePlay();
            UILobbyWindow.Open();
        }

        private void SendExitGameForRival()
        {
            photonView.RPC(nameof(ListenRivalExitGame), localClient.RivalPlayer);

            ExitChessGame();
        }

        [PunRPC]
        public void ListenRivalExitGame()
        {
            ExitChessGame();
        }
        
        #endregion


        #region -------------- Private Methods -----------

        private void CreateOrJoinDefaultRoom()
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = MaxPlayersPerRoom,
                IsVisible = true,
                PublishUserId = true,
                IsOpen = true
            };
            PhotonNetwork.JoinOrCreateRoom(DefaultRoomName, roomOptions, TypedLobby.Default);

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
                Debug.Log($" Other Client: {player.NickName}");

                ClientData client = GetClientDataFromCloud(player);
                if (client == null) continue;

                UILobbyWindow.CreateNewUIPlayer(client, () => SendInvite(player.UserId));
                //UILobbyWindow.CreateNewUIPlayer(client, () => TestSendInvite());

            }

            UILobbyWindow.SetPlayerNum(PhotonNetwork.PlayerList.Length);
            UILobbyWindow.SetLocalPlayerNickName(NickNameInput.text);
        }

        private void InitClientData()
        {
            localClient = new ClientData
            {
                Id = LocalPlayer.UserId,
                Name = LocalPlayer.NickName,
                Status = ClientStatus.InLobby
            };

            
        }

        private Photon.Realtime.Player GetClientByUserId(string userId)
        {
            foreach (Player client in PhotonNetwork.PlayerList)
            {
                if(client.UserId == userId)
                {
                    return client;
                }
            }

            Debug.Log(" BUG : Not found clint with id - " + userId);
            return null;
        }


        private void UpdateClientDataToCloud()
        {
            // Convert client data to JSON
            string jsonData = JsonUtility.ToJson(localClient);

            // Set the client data as a custom property of the local player
            
            playerProperties[ClientDataName] = jsonData;
            PhotonNetwork.LocalPlayer.CustomProperties = playerProperties;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);  

            Debug.Log($"  - Send Client Data: {PhotonNetwork.LocalPlayer.CustomProperties[ClientDataName]}");
        }

        private ClientData GetClientDataFromCloud(Photon.Realtime.Player client)
        {
            //if (client.CustomProperties.ContainsKey("ClientData"))
            //{
            //    string jsonData = (string)client.CustomProperties["ClientData"];
            //    ClientData clientDataFromPlayer = JsonUtility.FromJson<ClientData>(jsonData);
            //    return clientDataFromPlayer;
            //}

            if (client.CustomProperties.TryGetValue(ClientDataName, out object data))
            {
                string jsonData = (string)data;
                ClientData clientDataFromPlayer = JsonUtility.FromJson<ClientData>(jsonData);
                return clientDataFromPlayer;
            }

            Debug.Log($"BUG: Client data not found with player - {client.NickName}");
            return null;
        }

        #endregion
    }

}
