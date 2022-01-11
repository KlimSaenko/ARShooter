using UnityEngine;

namespace Game.SO
{
    [CreateAssetMenu(menuName = "Event/StaticManager")]
    public class StaticManager : GameEvent
    {
        [SerializeField] private GameObject[] managersPrefabs;

        private static StaticManager _instance;
        private static GameObject[] _managers;
        internal static GameObject[] Managers => _managers;

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogError("There is existing manager in project.");
                DestroyImmediate(this);
                return;
            }

            _instance = this;
        }

        private static bool _membersSetted;

        private void SetMembers()
        {
            if (_membersSetted) return;
            _membersSetted = true;

            _managers = new GameObject[managersPrefabs.Length];

            for (var i = 0; i < managersPrefabs.Length; i++)
            {
                var manager = Instantiate(managersPrefabs[i]);
                DontDestroyOnLoad(manager);
                _managers[i] = manager;
            }
        }

        internal override void Raise()
        {
            SetMembers();
        }
    }
}
