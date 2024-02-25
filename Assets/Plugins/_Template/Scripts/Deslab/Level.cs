using Sirenix.OdinInspector;
using UnityEngine;

namespace Deslab.Level
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private bool startAndEnd;
        [ShowIf("startAndEnd")] [SerializeField] private Transform startLevelPoint;
        [ShowIf("startAndEnd")] [SerializeField] private Transform endLevelPoint;
        [SerializeField] private bool applyEnvironment;
        [ShowIf("applyEnvironment")] [SerializeField] private Transform environmentParent;

        public Transform StartLevelPoint { get => startLevelPoint;}
        public Transform EndLevelPoint { get => endLevelPoint;}
        public Transform EnvironmentParent { get => environmentParent;}

        private void Start()
        {
            LevelManager.OnLevelLoaded?.Invoke(this);
        }
    }
}
