using System;
using UnityEngine;
using Game.SO;

public class Temp : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private GameObject ok;
    internal static Action<WeaponsList> WeaponsUpdateEvent;

    private void Awake()
    {
        //WeaponsUpdateEvent.Invoke()
        Debug.Log("op");
    }

    public void OnBeforeSerialize()
    {
        Debug.Log("ok");
    }

    public void OnAfterDeserialize()
    {
        throw new NotImplementedException();
    }
}
