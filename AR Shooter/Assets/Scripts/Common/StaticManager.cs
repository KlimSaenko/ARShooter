using UnityEngine;

namespace Game.SO
{
    public class StaticManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] managersPrefabs;

        private static StaticManager _instance;
        private static GameObject[] _managersPrefabs;
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _instance = this;

            _managersPrefabs = new GameObject[managersPrefabs.Length];
            
            for (var i = 0; i < managersPrefabs.Length; i++)
            {
                var manager = Instantiate(managersPrefabs[i], transform);
                _managersPrefabs[i] = manager;
            }
        }

        //private static StaticManager instance;
        //internal static StaticManager Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = FindObjectOfType<StaticManager>();
        //            if (instance == null)
        //            {
        //                GameObject obj = new GameObject();
        //                obj.name = "StaticManager";
        //                instance = obj.AddComponent<StaticManager>();
        //            }
        //        }

        //        return instance;
        //    }
        //}
    }
}
