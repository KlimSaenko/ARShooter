using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Animations;
using Game.UI;

public class NetworkedPlayer : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject currentAR;
    [SerializeField] private Transform camPos;
    [SerializeField] private GameObject mapMarkerPrefab;

    private RectTransform mapMarker;
    private Transform ownerPlayer;
    private Transform thisTransform;

    private IEnumerator Start()
    {
        thisTransform = transform;

        if (photonView.IsMine)
        {
            ConstraintSource constraintSource = new ConstraintSource
            {
                sourceTransform = Instantiate(currentAR, thisTransform).transform.GetComponentInChildren<Camera>().transform
            };
            constraintSource.weight = 1;

            PositionConstraint posConstraint = camPos.gameObject.AddComponent<PositionConstraint>();
            posConstraint.AddSource(constraintSource);
            posConstraint.constraintActive = true;

            RotationConstraint rotConstraint = camPos.gameObject.AddComponent<RotationConstraint>();
            rotConstraint.AddSource(constraintSource);
            rotConstraint.constraintActive = true;
        }
        else
        {
            Destroy(thisTransform.Find("Canvas").gameObject);

            mapMarker = Instantiate(mapMarkerPrefab, CommonUI.MapCircle).GetComponent<RectTransform>();

            yield return new WaitUntil(FindOwnerPlayer);

            ownerPlayer = MultiplayerController.ownerPlayer;

            Vector3 moveDir = ownerPlayer.position - camPos.position;
            mapMarker.anchoredPosition = new Vector2(moveDir.x, moveDir.z) * -70;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine && FindOwnerPlayer())
        {
            Vector3 moveDir = ownerPlayer.position - camPos.position;
            mapMarker.anchoredPosition = new Vector2(moveDir.x, moveDir.z) * -70;
        }
    }

    private bool FindOwnerPlayer()
    {
        if (ownerPlayer == null) ownerPlayer = MultiplayerController.ownerPlayer;

        return ownerPlayer != null;
    }
}
