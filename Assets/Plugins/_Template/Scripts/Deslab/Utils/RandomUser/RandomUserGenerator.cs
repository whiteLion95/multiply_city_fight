using UnityEngine;
using System.Collections.Generic;
using Deslab.UI;

namespace Deslab.Utils
{
    public struct RandomUser
    {
        public Sprite FlagSprite;
        public string Username;
    }

    public class RandomUserGenerator : MonoBehaviour
    {
        [SerializeField] private RandomUserSO randomUserSO;
        [SerializeField] private int usersCount = 10;

        private static List<RandomUser> randomUsers = new List<RandomUser>();
        private static RandomNoRepeate randomNoRepeate = new RandomNoRepeate();

        internal RandomUser RandUser;
        
        private void Awake()
        {
            InitRandomUser();
            SetRandomUser();
        }

        private void InitRandomUser()
        {
            for (int i = 0; i < usersCount; i++)
            {
                RandomUser newRandomUser = randomUserSO.RandomUser();
                randomUsers.Add(newRandomUser);
                randomNoRepeate.SetCount(randomUsers.Count);
            }
        }

        private void SetRandomUser()
        {
            RandUser = GetRandomUser();
        }

        public static void SetUserbar(UserBar userBar, RandomUser randomUser) =>
            userBar.SetUserBar(randomUser); 

        public static RandomUser GetRandomUser() => randomUsers[randomNoRepeate.GetAvailable()];
    } 
}


