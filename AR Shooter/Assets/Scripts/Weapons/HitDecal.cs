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
        [SerializeField] private Animation markerAnimation;
        [SerializeField] private AnimationClip[] animationClips;
        [SerializeField] private RectTransform hitMarker;
        [SerializeField] private TextMeshPro decalText;
        
        private static Camera MainCam;
    
        internal void ActivateHitMarker(int damage, HitZone.ZoneType type, bool withMarker)
        {
            MainCam = MainCam != null ? MainCam : Camera.main;

            if (MainCam is null) return;

            markerAnimation.clip = type switch
            {
                HitZone.ZoneType.Standard => animationClips[0],
                HitZone.ZoneType.Critical => animationClips[1],
                _ => animationClips[0]
            };

            if (withMarker)
            {
                UpdateMarkerPos();
                markerAnimation.Play();
            }

            decalText.text = damage.ToString();

            decalText.alpha = 1;
            decalText.DOFade(0, 0.18f).SetDelay(0.24f).SetEase(Ease.InOutQuad);
            
            var decalTransform = decalText.transform;
            var scale = Mathf.Pow(Vector3.Distance(decalTransform.position, MainCam.transform.position), 0.6f);
            decalTransform.localPosition = Vector3.zero;
            decalTransform.localScale = new Vector3(-1, 1, 1) * scale;
            decalTransform.LookAt(MainCam.transform);
            var random = Random.Range(-1f, 1f);
            decalTransform.DOLocalJump(0.03f * scale * ((random + Mathf.Sign(random)) * decalTransform.right - decalTransform.up), 0.08f * scale, 1, 0.42f)
                .SetEase(Ease.OutSine).OnComplete(DisableDecal);
        }

        private void UpdateMarkerPos() =>
            hitMarker.position = MainCam.WorldToScreenPoint(transform.position);

        private void DisableDecal()
        {
            Decals.DeactivateHitMarker(this);
            gameObject.SetActive(false);
        }
    }
}
