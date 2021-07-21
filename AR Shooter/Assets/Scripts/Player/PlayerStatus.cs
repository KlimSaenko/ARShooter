using System;
using System.Collections;
using Mobs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerStatus : MonoBehaviour, IDamageable
    {
        [SerializeField] protected Transform hpImages;
        [SerializeField] protected Collider[] colliders;

        protected const int playerHP = 100;

        public virtual int HP { get; set; }
        public bool IsAlive => HP > 0;

        protected int currentHP = 96;
    
        private void Awake()
        {
            HP = playerHP;
        }

        public event Action<MobStats> OnApplyDamage;

        public virtual void ApplyDamage(int damage)
        {
            HP -= damage;
            if (!IsAlive)
            {
                foreach (Collider hitBox in colliders) hitBox.enabled = false;
                Handheld.Vibrate();
                hpImages.gameObject.SetActive(false);

                StartCoroutine(Death(0));
            }
            else
            {
                HpUI();
            }
        }

        public IEnumerator Death(int deathType)
        {
            UI.AliveStateUI.SetActive(false);

            for (int i = 0; i < hpImages.childCount; i++)
            {
                hpImages.GetChild(i).gameObject.SetActive(true);
            }

            GameObject deadStateUI = UI.DeadStateUI;
            Image dark = deadStateUI.GetComponent<Image>();
            TextMeshProUGUI[] texts = deadStateUI.GetComponentsInChildren<TextMeshProUGUI>();

            deadStateUI.SetActive(true);

            while (dark.color.a < 0.42f)
            {
                dark.color += new Color(0, 0, 0, Time.deltaTime);
                foreach (TextMeshProUGUI text in texts) text.alpha += Time.deltaTime / 0.446f;

                yield return new WaitForEndOfFrame();
            }

            dark.color = new Color(0, 0, 0, 110f / 255f);
            foreach (TextMeshProUGUI text in texts) text.alpha = 240f / 255f;
        }

        protected void HpUI()
        {
            while (HP <= currentHP)
            {
                Handheld.Vibrate();
                hpImages.GetChild(currentHP / 4).gameObject.SetActive(false);
                currentHP -= 4;
            }
        }
    }
}
