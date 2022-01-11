using UnityEngine;
using Game.Weapons;
using Game.SO;
using TMPro;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace Game.Managers
{ 
    public class ShowWeapon : MonoBehaviour
    {
        [Header("Models")]
        [SerializeField] private GameObject[] weaponsModels;
        [SerializeField] private GameObject[] supports;

        private static readonly Dictionary<WeaponName, GameObject[]> _weaponsModels = new();

        [Space]
        [SerializeField] private WeaponStatsCaptions weaponStatsCaptions;

        private static TextMeshProUGUI[] _statsTexts;

        [Space]
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private Cinemachine.CinemachineVirtualCamera cinemachineCamera;

        private static List<WeaponName> _newWeapons = new();

        private void Awake()
        {
            //_statsTexts = new TextMeshProUGUI[] { weaponName, weaponType, damage, dps, accuracy, aimingAccuracy, weaponClip };

            for (var i = 0; i < weaponsModels.Length; i++)
            {
                if (_weaponsModels.ContainsKey((WeaponName)i + 1)) _weaponsModels[(WeaponName)i + 1] = new GameObject[] { weaponsModels[i], supports[i] };
                else _weaponsModels.Add((WeaponName)i + 1, new GameObject[] { weaponsModels[i], supports[i] });
            }

            var screenAspect = Screen.height / (float)Screen.width;
            cinemachineCamera.m_Lens.FieldOfView = Mathf.Pow(screenAspect, 0.6f) * 62;
        }

        private void Start()
        {
            if (NewWeaponSetted()) playableDirector.Play();
            else
            {
                Quit();
            }
        }

        private bool NewWeaponSetted()
        {
            var newWeapon = _newWeapons != null && _newWeapons.Count > 0;
            if (newWeapon)
            {
                WeaponShow(_newWeapons[0]);
                _newWeapons.RemoveAt(0);
            }

            return newWeapon;
        }

        internal static void SetNewWeapon(WeaponName newWeapon)
        {
            _newWeapons.Add(newWeapon);

            GameScenesManager.AddQueueScene(GameScene.ShowWeapon);
        }

        internal static void SetNewWeapon(IEnumerable<WeaponName> newWeapons)
        {
            foreach (var newWeapon in newWeapons)
            {
                SetNewWeapon(newWeapon);
            }
        }

        internal static void WeaponShow(WeaponName weaponName)
        {
            var configs = JsonSaver.WeaponConfigs;

            if (configs == null) return;
            
            if (_weaponsModels.TryGetValue(weaponName, out var weaponModel))
            {
                foreach (var part in weaponModel) part.SetActive(true);
            }

            //var stats = configs.CurrentWeaponStats(weaponName, out var name);

            //_statsTexts[0].text = name;
            //_statsTexts[1].text = "Primary";
            //_statsTexts[2].text = stats.damageMin + "-" + stats.damageMax;
            //_statsTexts[3].text = "60";
            //_statsTexts[4].text = (10 * stats.aimRecoveryTime * stats.aimSpreadIncrement).ToString("F2");
            //_statsTexts[5].text = (3 * stats.aimRecoveryTime * stats.aimSpreadIncrement).ToString("F2");
            //_statsTexts[6].text = stats.bulletInMagazine.ToString();
        }

        [Space]
        [SerializeField] private Animator skipAnimator;

        private protected static readonly int skipId = Animator.StringToHash("Skip");
        public void SkipAnimation()
        {
            if (playableDirector.time == playableDirector.duration) return;
            
            if (!skipAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanSkip")) skipAnimator.SetTrigger(skipId);
            else
            {
                skipAnimator.SetTrigger(skipId);
                playableDirector.time = playableDirector.duration;
            }
        }

        public void Quit()
        {
            GameScenesManager.LoadScene(GameScene.MainMenu);
        }
    }
}
