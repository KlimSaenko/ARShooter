using System.Collections.Generic;
using UnityEngine;
using Game.Weapons;

namespace Game.UI
{
    public class MenuShowWeapon : MonoBehaviour
    {
        [SerializeField] private Animator showWeaponAnimator;
        [SerializeField] private ShowWeaponSwitchButton[] showWeaponSwitchButtons;

        [Space]
        [SerializeField] private WeaponStatsCaptions weaponStatsCaptions;

        private (Weapon, GameObject) _prevWeapon;
        private (Weapon, GameObject) _currentWeapon;

        private bool _weaponVisible;

        private static readonly Dictionary<WeaponName, (Weapon, GameObject)> _weaponsModels = new();
        internal static Dictionary<WeaponName, (Weapon, GameObject)> WeaponsModels => _weaponsModels;

        private void Awake()
        {
            var weaponsToShow = GetComponentsInChildren<IWeapon>(true);

            var i = 0;

            foreach (var weaponToShow in weaponsToShow)
            {
                var go = weaponToShow.InstantiateWeapon(null, i);

                if (go == null) continue;
                
                var stats = weaponToShow.WeaponConfig;

                if (_weaponsModels.ContainsKey(stats.weaponName)) _weaponsModels[stats.weaponName] = (stats, go);
                else _weaponsModels.Add(stats.weaponName, (stats, go));

                i++;
            }
        }

        private void OnEnable()
        {
            foreach (var showWeaponSwitchButton in showWeaponSwitchButtons)
            {
                showWeaponSwitchButton.PressedAction += ChangeWeapon;
            }
        }

        private void OnDisable()
        {
            foreach (var showWeaponSwitchButton in showWeaponSwitchButtons)
            {
                showWeaponSwitchButton.PressedAction -= ChangeWeapon;
            }
        }

        //private static readonly int panelFadeIn = Animator.StringToHash("MM Panel In");
        //private static readonly int panelFadeOut = Animator.StringToHash("MM Panel Out");
        private static readonly int panelVisibleId = Animator.StringToHash("Panel Visible");
        private static readonly int panelExpandInId = Animator.StringToHash("Panel Expand In");
        private static readonly int panelExpandOutId = Animator.StringToHash("Panel Expand Out");

        private static readonly int changeId = Animator.StringToHash("Change Weapon");
        private void ChangeWeapon(int type)
        {
            if (_weaponsModels.TryGetValue((WeaponName)type, out var currentWeapon) && currentWeapon != _currentWeapon)
            {
                _prevWeapon = _currentWeapon;
                _currentWeapon = currentWeapon;

                if (_weaponVisible) showWeaponAnimator.SetTrigger(changeId);
            }
        }

        internal void ChangeWeapon(WeaponName type) =>
            ChangeWeapon((int)type);

        internal void SetFavoriteWeapon()
        {
            //if (_weaponsModels.TryGetValue(WeaponName.M4Carabine, out var currentWeapon))
            //{
            //    _prevWeapon = _currentWeapon;
            //    _currentWeapon = currentWeapon;

            //    if (_weaponVisible) showWeaponAnimator.SetTrigger(changeId);
            //}

            if (_weaponVisible) showWeaponAnimator.SetTrigger(changeId);
            //ChangeWeapon(WeaponName.M4Carabine);
        }

        private void SetWeapon()
        {
            switch (MenuPanelsManager.CurrentPanel)
            {
                case MenuPanel.Inventory:
                    _prevWeapon.Item2?.SetActive(false);
                    _currentWeapon.Item2?.SetActive(true);
                    UpdateStatsPanel(_currentWeapon.Item1.stats);
                    break;

                case MenuPanel.Profile:
                    _currentWeapon.Item2?.SetActive(false);
                    if (_weaponsModels.TryGetValue(WeaponName.M4Carabine, out var currentWeapon)) 
                    {
                        _prevWeapon = currentWeapon;
                        currentWeapon.Item2?.SetActive(true);
                    }
                    break;

                default:
                    break;
            }
            //_prevWeapon.Item2?.SetActive(false);
            //_currentWeapon.Item2?.SetActive(true);
            
            //UpdateStatsPanel(_currentWeapon.Item1.stats);
        }

        private void SetFavorite()
        {
            _currentWeapon.Item2?.SetActive(false);

            if (_weaponsModels.TryGetValue(WeaponName.M4Carabine, out var currentWeapon)) currentWeapon.Item2?.SetActive(true);
        }

        private void UpdateStatsPanel(WeaponStats newWeaponStats)
        {
            //weaponStatsCaptions.weaponName.text = name;
            //weaponStatsCaptions.weaponType.text = "Primary";
            weaponStatsCaptions.damage.text = newWeaponStats.damageMin + "-" + newWeaponStats.damageMax;
            weaponStatsCaptions.dps.text = "60";
            weaponStatsCaptions.accuracy.text = (10 * newWeaponStats.aimRecoveryTime * newWeaponStats.aimSpreadIncrement).ToString("F2");
            weaponStatsCaptions.aimingAccuracy.text = (3 * newWeaponStats.aimRecoveryTime * newWeaponStats.aimSpreadIncrement).ToString("F2");
            weaponStatsCaptions.weaponClip.text = newWeaponStats.bulletInMagazine.ToString();
        }

        internal void SetActivePanel(bool open)
        {
            _weaponVisible = open;
            showWeaponAnimator.SetBool(panelVisibleId, open);

            if (open) showWeaponAnimator.SetTrigger(changeId);
            else ExpandPanel(false);
        }

        public void ExpandPanel(bool value)
        {
            //menuPanelBase.OpenPanel(!value);
            showWeaponAnimator.Play(value ? panelExpandInId : panelExpandOutId);
            //menuPanelBase.PanelAnimator.SetBool(panelExpandId, value);
            //MenuMainPanelsManager.PanelOpen(MenuPanel.Empty);
        }
    }
}
