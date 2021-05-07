using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PreviewScene : MonoBehaviour
{
    [SerializeField] private GameObject previewMob;
    [SerializeField] private Camera mainCam;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject prediction;

    private float spawnHeight = -0.1f;
    private Vector2 screenCenter;

    private Pose? currentPose = null;

    private void Start()
    {
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    private List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

    private void Update()
    {
        if (raycastManager.Raycast(screenCenter, hitResults, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
        {
            if (currentPose == null) 
            { 
                currentPose = hitResults[0].pose;
                prediction.SetActive(false);

                spawnHeight = hitResults[0].pose.position.y;

                previewMob.SetActive(true);
            }
            else if (currentPose == hitResults[0].pose)
            {
                spawnHeight = hitResults[0].pose.position.y;
            }

            previewMob.transform.position = new Vector3(previewMob.transform.position.x, spawnHeight, previewMob.transform.position.z);
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (Physics.Raycast(mainCam.ScreenPointToRay(touch.position), out RaycastHit hitInfo) && hitInfo.transform.gameObject.TryGetComponent(out PreviewMob previewMobScript))
                {
                    previewMobScript.TapOnMob();
                }
            }
        }
    }
}
