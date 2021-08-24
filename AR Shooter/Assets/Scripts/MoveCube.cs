using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoveCube : MonoBehaviour
{
    [SerializeField] private Vector3[] positions;
    [SerializeField] private float time = 5f;

    private void Start()
    {
        Move();
    }

    private int i = 0;
    private void Move()
    {
        transform.DOLocalMove(positions[i], time).SetEase(Ease.InOutSine).OnComplete(Move);
            
        if (i < positions.Length - 1) i++;
        else i = 0;
    }
}
