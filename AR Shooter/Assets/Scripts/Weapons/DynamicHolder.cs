using Game.UI;
using UnityEngine;

namespace Game.Weapons
{
    public class DynamicHolder : MonoBehaviour
    {
        [SerializeField] private Transform swayRef;
        [SerializeField] private Transform bulletsLabel;

        [Space]
        [SerializeField] private Camera localCamera;

        internal static Transform BulletsLabelRef;

        private Transform _thisTransform;

        private Vector3 _initialPosition;
        private Vector3 _prevSwayPosition;

        private float _lerpValue = 1 / 30f;

        private void Awake()
        {
            _thisTransform = transform;

            _initialPosition = _thisTransform.localPosition;
            _prevSwayPosition = swayRef.position;
        }

        private void LateUpdate()
        {
            _lerpValue = 6 * Time.deltaTime;

            localCamera.fieldOfView = Mathf.Atan(Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad) / (1 - 4.8f * localCamera.transform.localPosition.z)) * Mathf.Rad2Deg;

            var deltaRotation = swayRef.InverseTransformPoint(_prevSwayPosition);
            deltaRotation = Vector3.ClampMagnitude(_lerpValue * deltaRotation, 0.04f);
            _prevSwayPosition = swayRef.position;
            
            MoveSway(deltaRotation);
            TiltSway(deltaRotation);
        }

        private void MoveSway(Vector3 deltaRotation)
        {
            var newDeltaPosition = new Vector3(2.5f * deltaRotation.x, deltaRotation.y, 2 * deltaRotation.z);
            _thisTransform.localPosition = Vector3.Lerp(_thisTransform.localPosition, _initialPosition + newDeltaPosition, _lerpValue);

            if (BulletsLabelRef is not null)
            {
                bulletsLabel.localPosition = Vector3.Lerp(bulletsLabel.localPosition,
                    _thisTransform.InverseTransformPoint(BulletsLabelRef.position) + 1.1f * newDeltaPosition, _lerpValue);
                bulletsLabel.rotation = Quaternion.Lerp(bulletsLabel.rotation, localCamera.transform.rotation, 0.8f * _lerpValue);
            }

            CommonUI.Aim.UpdateSize(10 * newDeltaPosition.magnitude);
        }

        private void TiltSway(Vector3 deltaRotation)
        {
            var newRotation = Quaternion.Euler(new Vector3(-0.3f * deltaRotation.y, 0, deltaRotation.x) * 600);
            
            _thisTransform.localRotation = Quaternion.Slerp(_thisTransform.localRotation, newRotation, _lerpValue);
        }
    }
}