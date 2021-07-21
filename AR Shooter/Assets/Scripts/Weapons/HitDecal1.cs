using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDecal1 : MonoBehaviour
{
    [Range(0.0f, 5.0f)]
    [SerializeField] private float power;
    
    [Range(0.0f, 1.0f)]
    [SerializeField] private float time;

    private Transform _thisTransform;

    private void Start()
    {
        _thisTransform = transform;
        _currentVelocity = power;
    }

    private float _currentVelocity;
    private float _currentTime;
    
    private void Update()
    {
        var newPosition = _thisTransform.position;
        newPosition += new Vector3(0, newPosition.y + _currentVelocity);
        _thisTransform.position = newPosition;

        _currentVelocity -= 0.1f;
        _currentTime += Time.deltaTime;
        
        if (_currentTime >= time)
            EndDecal();
    }

    private void EndDecal()
    {
        gameObject.SetActive(false);
    }
}
