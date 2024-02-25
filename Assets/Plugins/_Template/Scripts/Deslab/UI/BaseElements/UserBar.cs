using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deslab.Utils;

namespace Deslab.UI
{
    public class UserBar : MonoBehaviour
    {
        [SerializeField] private Image _flagSprite;
        [SerializeField] private TMP_Text _username;

        public void SetUserBar(RandomUser randomUser)
        {
            _flagSprite.sprite = randomUser.FlagSprite;
            _username.text = randomUser.Username;
        }
    }
}