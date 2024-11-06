using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace MH.Lobby
{
    public class UIInvite : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _acceptBtn;
        [SerializeField] private Button _rejectBtn;

        [SerializeField] private float Timer = 10;

        private Action _rejectCallback;

        private void Start()
        {
            _rejectBtn.onClick.AddListener( () => _rejectCallback());
        }

        //public void Update()
        //{
        //    Timer -= Time.deltaTime;

        //    if (Timer <= 0)
        //    {
        //        _rejectCallback?.Invoke();  
        //    }
        //}

        public void SetTitle(string inviterName)
        {
            _titleText.text = inviterName + " want to play a game with you ?";
        }

        public void RegisterAcceptBtn(Action callback)
        {
            _acceptBtn.onClick.AddListener(() => callback());
        }

        public void RegisterRejectCallback(Action callback)
        {
            _rejectCallback = callback;
        }

        

    }
}
