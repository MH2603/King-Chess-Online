using UnityEngine;
using MH;
using TMPro;
using UnityEngine.UI;

namespace MH.Lobby
{

    public class UIPlayer : MonoBehaviour
    {
        #region ------------ Fields ------------------

        [SerializeField] private TextMeshProUGUI NickNameText;
        [SerializeField] private TextMeshProUGUI StatusText;
        [SerializeField] private Button InviteBtn;

        #endregion


        #region --------------- Properties ---------------

        private string _nickName;

        #endregion

        #region --------------- Public Methods ---------------

        public void Init(string nickName)
        {
            _nickName = nickName;   
            NickNameText.text = nickName;
            
        }

        public void ToggleInviteBtn(bool on)
        {
            InviteBtn.gameObject.SetActive(on); 
        }

        public void UpdateStatus(string nameStatus ) 
        {
            StatusText.text = nameStatus;   
        }

        #endregion
    }

}
