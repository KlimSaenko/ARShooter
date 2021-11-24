using UnityEngine;
using TMPro;

namespace Game.Mobs
{
    public class MobMarker : MonoBehaviour
    {
        [SerializeField] private TextMeshPro distanceText;
        [SerializeField] private RectTransform arrowAnchor;
        [SerializeField] private GameObject arrow;
        [SerializeField] private Animator arrowAnimator;

        [Space]
        [SerializeField] private Transform attachedMobAnchor;

        private Camera _mainCamera;
        private bool MainCameraReady()
        {
            var ready = _mainCamera != null;
            if (ready) return ready;

            _mainCamera = Camera.main;
            return ready;
        }

        internal float DistanceToPlayer;
        private Vector3 _markerPos;

        private void OnEnable()
        {
            _markerPos = transform.position;

            if (MainCameraReady()) _markerVisible = MarkerVisible();
        }

        private void Update()
        {
            if (attachedMobAnchor)
            {
                transform.position = Vector3.Lerp(transform.position, attachedMobAnchor.position, 9 * Time.deltaTime);
            }

            if (MainCameraReady())
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_mainCamera.transform.position - transform.position, Vector3.up), 6 * Time.deltaTime);

                DistanceToPlayer = Vector3.Distance(transform.position, _mainCamera.transform.position);
                distanceText.text = DistanceToPlayer.ToString("F1") + " m";

                _markerPos = transform.position;

                if (MarkerVisible()) return;

                var marker2DPos = _mainCamera.transform.InverseTransformDirection(_markerPos - _mainCamera.transform.position);
                var currentAngle = Mathf.Sign(marker2DPos.x) * Vector3.Angle(Vector2.down, new Vector2(marker2DPos.x, marker2DPos.y));

                arrowAnchor.localEulerAngles = new Vector3(0, 0, currentAngle);
            }
        }

        private bool _markerVisible;
        private bool MarkerVisible()
        {
            var arrowScreenPos = _mainCamera.WorldToViewportPoint(_markerPos);
            var value = arrowScreenPos.x > 0 && arrowScreenPos.x < 1 && arrowScreenPos.y > 0 && arrowScreenPos.y < 1 && arrowScreenPos.z > 0;

            if (_markerVisible != value)
            {
                arrow.SetActive(value);
                arrowAnchor.gameObject.SetActive(!value);
                _markerVisible = value;
            }

            return value;
        }

        private protected static readonly int showHPId = Animator.StringToHash("ShowHP");
        private protected static readonly int blendHPId = Animator.StringToHash("BlendHP");
        internal void ChangeHP(float hpPercent)
        {
            arrowAnimator.SetBool(showHPId, true);
            arrowAnimator.SetFloat(blendHPId, hpPercent);
        }
    }
}
