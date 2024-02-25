using System.Collections.Generic;
using UnityEngine;

namespace Deslab.Utils
{
    public class RandomNoRepeate
    {
        private readonly List<int> available = new List<int>();
        private int count;

        public int Count { get => count; }

        public void Init()
        {
            available.Clear();
            for (var i = 0; i < count; i++) available.Add(i);
        }

        public void SetCount(int value)
        {
            count = value;
            Init();
        }

        public void RemoveID(int removeID)
        {
            available.Remove(removeID);
        }

        public int GetAvailable(int startingNumber = 0)
        {
            CheckAvailableIds(startingNumber);

            var availableId = Random.Range(startingNumber, available.Count);
            var id = available[availableId];
            available.RemoveAt(availableId);
            return id;
        }

        public int GetAvailableExcept(int except, int minIndex = 0)
        {
            CheckAvailableIds(minIndex);

            if (available.Contains(except))
                available.RemoveAt(available.IndexOf(except));

            var availableId = Random.Range(minIndex, available.Count);
            var id = available[availableId];
            available.RemoveAt(availableId);
            return id;
        }

        private void CheckAvailableIds(int minID)
        {
            if (available.Count == minID) Init();
        }
    }
}