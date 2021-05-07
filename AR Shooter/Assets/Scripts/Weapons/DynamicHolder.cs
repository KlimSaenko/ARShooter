using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicHolder : MonoBehaviour
{
    [SerializeField] private Transform joint;
    private Transform thisTransform;

    private void Awake()
    {
        thisTransform = transform;
    }

    private void Update()
    {
        if (joint != null)
        {
            thisTransform.position = Vector3.Lerp(thisTransform.position, joint.position, Mathf.Lerp(15, 29, 29 * Vector3.Distance(thisTransform.position, joint.position)) * Time.deltaTime);
            thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, joint.rotation, 25 * Time.deltaTime);
        }
    }
}
