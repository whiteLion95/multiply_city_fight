using UnityEngine;
using Lean.Touch;
using Deslab.UI;
using System.Collections;
using System;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

namespace Deslab.Level
{
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private List<GameObject> levels;
        [SerializeField] private GameObject playerToLoad;
        public LevelProgression levelProgression;
        public GameObject currentLevel;
        public GameObject currentPlayer;

        public static Action<Level> OnLevelLoaded;
        public static Action OnNewLevel;
        public static Action OnLevelStarted;
        public static Action OnLevelEnded;

        public List<GameObject> Levels { get { return levels; } }
        public bool LevelStarted { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            StaticManager.OnWin += () => OnLevelEnded?.Invoke();
            StaticManager.OnLose += () => OnLevelEnded?.Invoke();
        }

        /// <summary>
        /// Loading level from Resources/Levels/ by current Level ID.
        /// Level will be instanced in zero position world coordinates.
        /// 
        /// Loading player from Resources/PlayerController/.
        /// Player will be instanced in zero position world coordinates.
        /// </summary>
        public void LoadLevelFromResources()
        {
            OnNewLevel?.Invoke();

            if (currentLevel != null) Destroy(currentLevel);
            if (currentPlayer != null) Destroy(currentPlayer);

            GameObject _instance;
            _instance = Instantiate(levels[StaticManager.GetLevelID() - 1]);
            ResetTransform(_instance.transform);
            currentLevel = _instance;

            Level levelSettings = _instance.GetComponent<Level>();

            if (playerToLoad != null)
            {
                GameObject playerInstance;
                playerInstance = Instantiate(playerToLoad);
                SetTransform(playerInstance.transform, levelSettings.StartLevelPoint.position);
                currentPlayer = playerInstance;
            }

            StaticManager.Instance.SavePlayerData();
            LevelStarted = false;
        }

        /// <summary>
        /// Reset transform position, scale and rotation to zero.
        /// </summary>
        /// <param name="_transform">Transform for reset</param>
        private void ResetTransform(Transform _transform)
        {
            _transform.position = Vector3.zero;
            _transform.localScale = Vector3.one;
            _transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Sets transform's position to starting position and reset scale and rotation
        /// </summary>
        /// <param name="_transform">Transform to set</param>
        /// <param name="startPos">Starting position</param>
        private void SetTransform(Transform _transform, Vector3 startPos)
        {
            _transform.position = startPos;
        }

        /// <summary>
        /// Called after started play level.
        /// </summary>
        internal void StartLevel()
        {
            OnLevelStarted?.Invoke();
            LevelStarted = true;
        }
    }
}