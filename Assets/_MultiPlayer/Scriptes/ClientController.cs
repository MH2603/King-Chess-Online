
namespace MH
{

    public enum ClientStatus
    {
        Offline,
        InLobby,
        InGame
    }

    [System.Serializable]
    public class ClientData
    {
        public string Id;
        public string Name; 
        public ClientStatus Status; 

        // InGame Data
        public TypeColor PlayerColor;
        public Photon.Realtime.Player RivalPlayer;
    }


}
