using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    /// <summary>
    /// Invokes when player starts or finishes firing
    /// </summary>
    internal static event Action<bool> FiringAction;
    
    public virtual void OnFiringAction(bool start) =>
        FiringAction?.Invoke(start);
    
    
    internal static event Action<Vector3> HitAction;

    private static void OnHitAction(Vector3 pos) =>
        HitAction?.Invoke(pos);
    
    /// <summary>
    /// Invokes when player aims weapon (or goes to free position)
    /// </summary>
    internal static event Action<bool> AimingAction;
    
    public virtual void OnAimingAction(bool toAim) =>
        AimingAction?.Invoke(toAim);
}
