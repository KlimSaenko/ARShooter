using System;
using Common;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Weapons
{
    public class HitDecal : MonoBehaviour
    {
        [Range(0.0f, 5.0f)]
        [SerializeField] private float power;
    
        [Range(0.0f, 1.0f)]
        [SerializeField] private float time;
    
        [SerializeField] private RectTransform hitMarker;
        [SerializeField] private Animation[] animations;

        // private Transform _thisTransform;
    
        private static Camera MainCam => Camera.main;

        private void Start()
        {
            // _thisTransform = transform;
            // _currentVelocity = power;
            // normalMarker.gameObject.
            
            // var hitMarker = this.hitMarker;
            // if (hitMarker.gameObject.TryGetComponent(out Image image))
            //     image.color = Color.red;
            // _currentMarker = 
        }

        // private float _currentVelocity;
        // private float _currentTime;
        //
        // private void Update()
        // {
        //     var newPosition = _thisTransform.position;
        //     newPosition += new Vector3(0, newPosition.y + _currentVelocity);
        //     _thisTransform.position = newPosition;
        //
        //     _currentVelocity -= 0.1f;
        //     _currentTime += Time.deltaTime;
        //     
        //     if (_currentTime >= time)
        //         EndDecal();
        // }
    
        internal void ActivateHitMarker(int damage, HitZone.ZoneType type)
        {
            hitMarker.position = MainCam.WorldToScreenPoint(transform.position);

            foreach (var animation in animations) animation.Play();
        }

        public void DisableDecal()
        {
            Pool.Decals.DeactivateHitMarker(this);
            gameObject.SetActive(false);
        }
    }
}
