using UnityEngine;

namespace Weapons
{
    public class DynamicHolder : MonoBehaviour
    {
        [SerializeField] private Transform joint;
        private Transform _thisTransform;

        internal static float Inertia = 1f;

        private void Awake() =>
            _thisTransform = transform;

        private void LateUpdate()
        {
            if (!joint) return;
            
            // thisPosition = Vector3.Lerp(thisPosition, jointPosition, 
            //     Mathf.Lerp(15, 29, 29 * Vector3.Distance(thisPosition, jointPosition)) * Time.deltaTime);

            var position = _thisTransform.position;
            position = Vector3.Lerp(position, joint.position, 55 / Inertia * Mathf.Pow(Vector3.Distance(position, joint.position), 0.54f) * Time.deltaTime);
            _thisTransform.position = position;

            var rotation = _thisTransform.rotation;
            rotation = Quaternion.Lerp(rotation, joint.rotation, 7 / Inertia * Mathf.Pow(Quaternion.Angle(rotation, joint.rotation), 0.4f) * Time.deltaTime);
            _thisTransform.rotation = rotation;
        }
    }
}
