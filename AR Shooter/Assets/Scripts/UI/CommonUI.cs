using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Weapons;
using Random = UnityEngine.Random;

namespace Game.UI
{
    public class CommonUI : MonoBehaviour
    {
        [SerializeField] private Image mapLookImage;
        [SerializeField] private Transform mapCircle;
        [SerializeField] private TextMeshProUGUI killsText;

        internal static GameObject AliveStateUI;
        internal static GameObject DeadStateUI;

        internal static Transform MapCircle { get; set; }
        private static Camera MainCam => Camera.main;
        private static TextMeshProUGUI KillsText { get; set; }

        private Transform _player;

        private void OnEnable()
        {
            _aimTransform = aim;
            _measurableAimTransform = measurableAim;
        }

        private void Awake()
        {
            MapCircle = mapCircle;

            KillsText = killsText;
            Config.MobsKills = 0;

            AliveStateUI = transform.GetChild(0).gameObject;
            DeadStateUI = transform.GetChild(1).gameObject;
        }

        private void Update()
        {
            if (_player != null)
            {
                //mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - player.rotation.eulerAngles.y);
                mapCircle.localRotation = Quaternion.Euler(0, 0, _player.rotation.eulerAngles.y);
            }
            else if (MainCam != null)
            {
                _player = MainCam.transform;
                mapLookImage.fillAmount = Screen.width / Screen.height * MainCam.fieldOfView / 360f;
                mapLookImage.rectTransform.localRotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180);
            }

            // if (IsTranslating()) Translation();
        }

        public void Quit()
        {
            IsPaused = false;

            SceneManager.LoadScene(0);
        }

        internal static bool IsPaused = false;

        public void Restart()
        {
            IsPaused = false;

            SceneManager.LoadScene(1);
        }

        internal static void KillsUI(int kills)
        {
            KillsText.text = kills.ToString();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) Config.SaveGame();
            else Config.LoadGame();
        }

        #region Aim

        [Header("Aim")]
        [SerializeField] private RectTransform aim;
        [SerializeField] private RectTransform measurableAim;

        private static RectTransform _aimTransform;
        private static RectTransform _measurableAimTransform;

        public class Aim
        {
            public Aim(WeaponStats weaponStats)
            {
                _weaponStats = weaponStats;
            }

            private readonly WeaponStats _weaponStats;

            internal static float CurrentAimSpreadDiameter => _measurableAimTransform.rect.width;
            private static float currentAimSpreadDiameter = 90;

            internal Ray GetRay() =>
                Camera.main.ScreenPointToRay(RawRaycastPoint);

            internal Vector2 RawRaycastPoint
            {
                get
                {
                    var aimPos = _aimTransform.position;
                    return CurrentAimSpreadDiameter * Random.insideUnitCircle / 2f + new Vector2(aimPos.x, aimPos.y);
                }
            }

            private const int TweenId = 100;

            internal void AimAnimation()
            {
                DOTween.Kill(TweenId);

                var aimSpreadIncrement = Mathf.Lerp(0, 400, _weaponStats.aimSpreadIncrement);

                DOTween.To(() => currentAimSpreadDiameter, x => currentAimSpreadDiameter = x, currentAimSpreadDiameter + aimSpreadIncrement, 0.04f).SetEase(Ease.OutBack)
                    .OnComplete(() => DOTween.To(() => currentAimSpreadDiameter, x => currentAimSpreadDiameter = x, 90, _weaponStats.aimRecoveryTime * Mathf.Pow(currentAimSpreadDiameter * 0.002f, 0.4f))
                        .SetEase(Ease.OutSine).SetId(TweenId));
            }

            private static float _prevAdditional = 0;

            internal static void UpdateSize(float additional)
            {
                additional = Mathf.Lerp(0, 300, additional);
                additional = Mathf.Lerp(_prevAdditional, additional, Time.deltaTime * 5);
                _prevAdditional = additional;

                _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentAimSpreadDiameter + additional);
            }
        }

        #endregion
    }
}
