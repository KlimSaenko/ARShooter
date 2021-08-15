using System;
using System.Collections.Generic;
using Mobs;
using UnityEngine;

namespace Weapons
{
    public class FlamethrowerTrigger : MonoBehaviour
    {
        [SerializeField] private Transform flamethrower;

        private Transform _thisTransform;
        internal static readonly List<HitZone> TriggeredEnemies = new();

        private void Awake()
        {
            _thisTransform = transform;
            
            _prevPos = _thisTransform.position;
            _prevRot = _thisTransform.rotation;
        }

        private Vector3 _prevPos;
        private Quaternion _prevRot;

        private void LateUpdate()
        {
            var position = flamethrower.position;
            position = Vector3.Lerp(_prevPos, position, 5f * Time.deltaTime);
            _thisTransform.position = position;
            _prevPos = position;
            
            var rotation = flamethrower.rotation;
            rotation = Quaternion.Lerp(_prevRot, rotation, 5f * Time.deltaTime);
            _thisTransform.rotation = rotation;
            _prevRot = rotation;
        }

        private void FixedUpdate()
        {
            TriggeredEnemies.Clear();
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out HitZone enemy)) TriggeredEnemies.Add(enemy);
        }
    }
}
