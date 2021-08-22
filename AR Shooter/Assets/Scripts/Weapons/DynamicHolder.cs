using UnityEngine;

namespace Weapons
{
    public class DynamicHolder : MonoBehaviour
    {
        [SerializeField] private Transform joint;
        private Transform _thisTransform;

        internal static float Inertia = 1;

        private void Awake() =>
            _thisTransform = transform;

        private void LateUpdate()
        {
            if (!joint) return;
            
            // thisPosition = Vector3.Lerp(thisPosition, jointPosition, 
            //     Mathf.Lerp(15, 29, 29 * Vector3.Distance(thisPosition, jointPosition)) * Time.deltaTime);
            
            _thisTransform.position = Vector3.Lerp(_thisTransform.position, joint.position, 30 / Inertia * Time.deltaTime);
            _thisTransform.rotation = Quaternion.Lerp(_thisTransform.rotation, joint.rotation, 25 / Inertia * Time.deltaTime);
        }
    }
}
