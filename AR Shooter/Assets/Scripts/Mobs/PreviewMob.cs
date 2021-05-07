using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewMob : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;

    private void Awake()
    {
        transform.LookAt(player);

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    public void TapOnMob()
    {
        animator.SetTrigger("Scared");
    }
}
