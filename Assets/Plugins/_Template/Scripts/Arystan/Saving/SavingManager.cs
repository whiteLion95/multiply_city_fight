using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Arystan.Saving
{
    /// <summary>
    /// Contains saving and loading methods
    /// </summary>
    public class SavingManager : Singleton<SavingManager>
    {
        private List<ISaveable> _saveableObjects;
        protected override void Awake()
        {
            base.Awake();

            _saveableObjects = new List<ISaveable>();
        }

        #region SavingLoadingMethods
        public void Save<T>(string key, T data)
        {
            string savedData = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, savedData);
            PlayerPrefs.Save();
        }

        public void Save(string key, int data)
        {
            PlayerPrefs.SetInt(key, data);
            PlayerPrefs.Save();
        }

        public void Save(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public void Save(string key, float data)
        {
            PlayerPrefs.SetFloat(key, data);
            PlayerPrefs.Save();
        }

        public void Save(string key, ulong data)
        {
            PlayerPrefs.SetFloat(key, data);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key)
        {
            T data = default;

            if (PlayerPrefs.HasKey(key))
            {
                string loadedJson = PlayerPrefs.GetString(key);
                data = JsonUtility.FromJson<T>(loadedJson);
            }
            else
            {
                Save(key, data);
                data = Load<T>(key);
            }

            return data;
        }

        public string LoadString(string key)
        {
            string data = PlayerPrefs.GetString(key);
            return data;
        }

        public int LoadInt(string key)
        {
            int data = PlayerPrefs.GetInt(key);
            return data;
        }

        public ulong LoadULong(string key)
        {
            ulong data = (ulong)PlayerPrefs.GetFloat(key);
            return data;
        }

        public float LoadFloat(string key)
        {
            float data = PlayerPrefs.GetFloat(key);
            return data;
        }
        #endregion

        #region WorkWithSaveableObjects
        public void AddToSaveableObjects(ISaveable saveable)
        {
            _saveableObjects.Add(saveable);
        }

        private void SaveAllSaveableData()
        {
            foreach (ISaveable saveable in _saveableObjects)
            {
                if (saveable != null) 
                    saveable.SaveData();
            }
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            SaveAllSaveableData();
        }

        private void OnApplicationPause(bool pause)
        {
            SaveAllSaveableData();
        }

        private void OnDestroy()
        {
            SaveAllSaveableData();
        }
        #endregion
    }
}
