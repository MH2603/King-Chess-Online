using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MH.Lobby
{

    public class UILobbyWindow : MonoBehaviour
    {
        #region --------------- Fields ----------------
        [SerializeField] private GameObject _screen;
        [SerializeField] private UIPlayer _uiPlayerPrefab;
        [SerializeField] private Transform _uiPlayerParent;
        [Space]
        [SerializeField] private TextMeshProUGUI _playerNumText;
        [SerializeField] private TextMeshProUGUI _localPlayerNicNameText;

        #endregion

        #region ---------------- Propertis -------------------

        private Dictionary<string, UIPlayer> _clientDict = new();
        

        #endregion

        public void Open()
        {
            _screen.SetActive(true);
        }

        public void Close()
        {
            _screen.SetActive(false);
        }

        public void SetLocalPlayerNickName(string nickname)
        {
            _localPlayerNicNameText.text = nickname;
        }

        public void CreateNewUIPlayer(ClientData client, Action inviteAction)
        {
            UIPlayer newUIPlayer = Instantiate(_uiPlayerPrefab, _uiPlayerParent);

            newUIPlayer.Init(client.Name);
            newUIPlayer.UpdateStatus(client.Status.ToString());
            newUIPlayer.RegisterInviteBtn(inviteAction);

            _clientDict[client.Id] = newUIPlayer;
        }

        public void UpdateUIOtherClient(ClientData client)
        {
            UIPlayer UIClient = GetUIClient(client.Id);

            if (UIClient == null) return;

            UIClient.ToggleInviteBtn(client.Status == ClientStatus.InLobby);
            UIClient.UpdateStatus(client.Status.ToString());
           
        }

        public void SetPlayerNum(int currentNum)
        {
            _playerNumText.text = currentNum.ToString() + " / 20";
        }


        private UIPlayer GetUIClient(string id)
        {
            if ( !_clientDict.ContainsKey(id) )
            {
                return null;    
            }

            return _clientDict[id]; 
        }
    }

}
