using DG.Tweening;
using static Game.Managers.Pool;
using Game.Mobs;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Weapons
{
    public class HitDecal : MonoBehaviour
    {
        [SerializeField] private Animator markerAnimator;
        [SerializeField] private RectTransform hitMarker;
        [SerializeField] private TextMeshPro decalText;

        private static Camera _mainCamera;
        private bool MainCameraReady()
        {
            if (_mainCamera != null) return true;

            _mainCamera = Camera.main;
            return _mainCamera != null;
        }

        private static readonly int normalId = Animator.StringToHash("Normal");
        private static readonly int criticalId = Animator.StringToHash("Critical");
        internal void ActivateHitMarker(int damage, HitZone.ZoneType type, bool withMarker)
        {
            if (!MainCameraReady()) return;

            if (withMarker)
            {
                UpdateMarkerPos();

                var typeId = type switch
                {
                    HitZone.ZoneType.Standard => normalId,
                    HitZone.ZoneType.Critical => criticalId,
                    _ => normalId
                };
                markerAnimator.SetTrigger(typeId);
            }

            decalText.text = damage.ToString();

            decalText.alpha = 1;
            decalText.DOFade(0, 0.18f).SetDelay(0.24f).SetEase(Ease.InOutQuad);
            
            var decalTransform = decalText.transform;
            var scale = Mathf.Pow(Vector3.Distance(decalTransform.position, _mainCamera.transform.position), 0.6f);
            decalTransform.localPosition = Vector3.zero;
            decalTransform.localScale = new Vector3(-1, 1, 1) * scale;
            decalTransform.LookAt(_mainCamera.transform);
            var random = Random.Range(-1f, 1f);
            decalTransform.DOLocalJump(0.03f * scale * ((random + Mathf.Sign(random)) * decalTransform.right - decalTransform.up), 0.08f * scale, 1, 0.42f)
                .SetEase(Ease.OutSine).OnComplete(DisableDecal);
        }

        private void UpdateMarkerPos() =>
            hitMarker.position = _mainCamera.WorldToScreenPoint(transform.position);

        private void DisableDecal()
        {
            Decals.DeactivateHitMarker(this);
            gameObject.SetActive(false);
        }
    }
}
