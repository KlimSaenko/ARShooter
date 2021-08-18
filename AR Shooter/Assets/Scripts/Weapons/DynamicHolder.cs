using UnityEngine;

namespace Weapons
{
    public class DynamicHolder : MonoBehaviour
    {
        [SerializeField] private Transform joint;
        private Transform _thisTransform;

        private void Awake() =>
            _thisTransform = transform;

        private void Update()
        {
            if (!joint) return;
        
            var thisPosition = _thisTransform.position;
            var inertia = Mathf.Pow(MainWeapon.ActiveWeaponStats.mass, 0.6f);
            
            // thisPosition = Vector3.Lerp(thisPosition, jointPosition, 
            //     Mathf.Lerp(15, 29, 29 * Vector3.Distance(thisPosition, jointPosition)) * Time.deltaTime);
            thisPosition = Vector3.Lerp(thisPosition, joint.position, 30 / inertia * Time.deltaTime);
            
            _thisTransform.position = thisPosition;
            _thisTransform.rotation = Quaternion.Lerp(_thisTransform.rotation, joint.rotation, 25 / inertia * Time.deltaTime);
        }
    }
}
