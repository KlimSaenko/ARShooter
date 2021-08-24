using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CubeSetter : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Transform transparentCube;
    [SerializeField] private GameObject normalCube;

    private readonly List<ARRaycastHit> _raycastHits = new ();
    public void SetCube(bool value)
    {
        transparentCube.gameObject.SetActive(value);

        if (!value)
        {
            normalCube.transform.position = transparentCube.position;
            normalCube.transform.rotation = transparentCube.rotation;
            normalCube.SetActive(true);
            normalCube.transform.DOPunchScale(new Vector3(0.03f, 0.03f, 0.03f), 0.2f, 0);
        }
        else if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), _raycastHits,
            TrackableType.PlaneWithinBounds | TrackableType.PlaneWithinPolygon))
        {
            transparentCube.position = _raycastHits[0].pose.position;
        }
    }

    private void Update()
    {
        if (!transparentCube.gameObject.activeSelf) return;

        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), _raycastHits,
            TrackableType.PlaneWithinBounds | TrackableType.PlaneWithinPolygon))
        {
            transparentCube.position = _raycastHits[0].pose.position;
        }
    }
}
