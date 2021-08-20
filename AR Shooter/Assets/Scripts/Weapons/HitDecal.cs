using System;
using System.Collections;
using DG.Tweening;
using static Common.Pool;
using Mobs;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class HitDecal : MonoBehaviour
    {
        [SerializeField] private Animation markerAnimation;
        [SerializeField] private AnimationClip[] animationClips;
        [SerializeField] private RectTransform hitMarker;
        [SerializeField] private TextMeshPro decalText;
    
        private static Camera MainCam => Camera.main;
    
        internal void ActivateHitMarker(int damage, HitZone.ZoneType type, bool withMarker)
        {
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
            decalText.DOFade(0, 0.3f).SetDelay(0.2f).SetEase(Ease.InOutQuad);
            
            var decalTransform = decalText.transform;
            decalTransform.localPosition = Vector3.zero;
            decalTransform.LookAt(MainCam.transform);
            var random = Random.Range(-0.11f, 0.11f);
            decalTransform.DOLocalJump(decalTransform.right * (random + Mathf.Sign(random) * 0.06f), 0.2f, 1, 0.5f)
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
