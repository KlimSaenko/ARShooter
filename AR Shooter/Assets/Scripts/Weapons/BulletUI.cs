using TMPro;
using UnityEngine;

namespace Weapons
{
    public class BulletUI : MonoBehaviour
    {
        [SerializeField] private TextMeshPro bulletText;

        private static TextMeshPro BulletText { get; set; }

        private void Awake() =>
            BulletText = bulletText;
        
        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, MainWeapon.ActiveWeaponStats.bulletUI.position, 0.5f);
        }

        internal static void UpdateCount(int to)
        {
            BulletText.text = to.ToString();
        }
    }
}
