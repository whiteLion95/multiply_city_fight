using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deslab.UI
{
    [Serializable]
    public class LeaderboardListElement
    {
        public bool isPlayer = false;
        public int hpValue;
        public string entityID;
        public string entityName;

        public LeaderboardListElement(bool isPlayer, string entityID, int hpValue,
            string entityName)
        {
            this.hpValue = hpValue;
            this.entityID = entityID;
            this.entityName = entityName;
            this.isPlayer = isPlayer;
        }
    }

    public class Leaderboard : MonoBehaviour
    {
        public static Leaderboard instance;
        [SerializeField] private List<LeaderboardListElement> sortedLeaderboard = new List<LeaderboardListElement>();
        [SerializeField] private List<LeaderboardElement> leaderboardElements = new List<LeaderboardElement>();

        private void Awake()
        {
            instance = this;
        }

        public void AddToLeaderboard(LeaderboardListElement leaderboardListElement)
        {
            sortedLeaderboard.Add(leaderboardListElement);
        }

        public void UpdateEntityHpValue(string entityID, int hpValue)
        {
            var updatedElement = sortedLeaderboard.Find(x => x.entityID == entityID);
            if (updatedElement != null) updatedElement.hpValue = hpValue;
            UpdateLeaderboardsElements();
        }

        public void UpdateLeaderboardsElements()
        {
            sortedLeaderboard = sortedLeaderboard.OrderByDescending(o => o.hpValue).ToList();

            var playerFound = false;

            for (var i = 0; i < sortedLeaderboard.Count; i++)
            {
                if (sortedLeaderboard[i].isPlayer)
                {
                    playerFound = true;
                    sortedLeaderboard[i].entityName = StaticManager.Instance.name;
                }
                    
                if (i >= leaderboardElements.Count - 1 && !playerFound)
                {
                    var player = sortedLeaderboard.Find(x => x.isPlayer == true);
                    var playerPlace = sortedLeaderboard.IndexOf(player);
                    leaderboardElements[i].SetPlacementID(playerPlace, player);
                }
                else
                    leaderboardElements[i].SetPlacementID(i, sortedLeaderboard[i]);
            }
        }

        public void ClearLeaderboard()
        {
            sortedLeaderboard.Clear();
        }
    }
}