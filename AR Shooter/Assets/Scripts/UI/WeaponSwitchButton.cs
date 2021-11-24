using TMPro;
using UnityEngine;
using Game.Weapons;
using Game.Managers;

namespace Game.UI
{
    public class WeaponSwitchButton : UIButton
    {
        [Space]
        [SerializeField] private protected GameObject infSign;
        [SerializeField] private protected TextMeshProUGUI weaponName, bulletsText, totalBulletsText;

        internal WeaponType type;
        private protected override int Type => (int)type;

        public override void OnValueChanged(bool value)
        {
            buttonAnimator.SetBool(isActiveId, value);

            if (value)
            {
                OnButtonPressed();
                PlayerBehaviour.OnWeaponSwitchAction(Type);
            }
        }

        private void Activate(WeaponType weaponType)
        {
            if (type == weaponType)
            {
                gameObject.SetActive(true);
            }
        }

        internal WeaponSwitchButton SetButton(int type)
        {
            var script = Instantiate(gameObject, WeaponSetSpawner.ButtonsSaveFolder).GetComponent<WeaponSwitchButton>();
            script.type = (WeaponType)type;

            GameSceneLogic.ActivateWeaponButtonsAction += script.Activate;

            return script;
        }

        internal void OnUpdateBullets(int bullets, int totalBullets)
        {
            bulletsText.text = bullets + "/";

            if (totalBullets >= 0)
            {
                totalBulletsText.text = (totalBullets - bullets).ToString();
                totalBulletsText.gameObject.SetActive(true);
                infSign.SetActive(false);
            }
            else
            {
                totalBulletsText.gameObject.SetActive(false);
                infSign.SetActive(true);
            }
        }
    }
}
