using UnityEngine;
using Game.Weapons;

namespace Game.UI
{
    public class ShowWeaponSwitchButton : UIButton
    {
        [SerializeField] private WeaponName weaponName;

        private protected override int Type => (int)weaponName;
    }
}
