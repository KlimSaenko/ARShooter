using Game.SO;
using TMPro;
using UnityEngine;

namespace Game.Weapons
{
    public class WeaponToShow : MonoBehaviour, IWeapon
    {
        [SerializeField] private WeaponName weaponName;

        private int _index;

        private Weapon _weaponConfig;
        public Weapon WeaponConfig => _weaponConfig;

        public GameObject InstantiateWeapon(Transform saveFolder, int index, TextMeshPro bulletsText = null)
        {
            var configs = JsonSaver.WeaponConfigs;
            _index = index;

            if (configs == null) return null;

            //if (_weaponsModels.TryGetValue(weaponName, out var weaponModel))

            _weaponConfig = configs.CurrentWeaponStats(weaponName);

            return gameObject;
        }
    }

    [System.Serializable]
    public struct WeaponStatsCaptions
    {
        [Header("Label")]
        [SerializeField] internal TextMeshProUGUI weaponName;
        [SerializeField] internal TextMeshProUGUI weaponType;

        [Header("Stats")]
        [SerializeField] internal TextMeshProUGUI damage;
        [SerializeField] internal TextMeshProUGUI dps;
        [SerializeField] internal TextMeshProUGUI accuracy;
        [SerializeField] internal TextMeshProUGUI aimingAccuracy;
        [SerializeField] internal TextMeshProUGUI weaponClip;
    }
}
