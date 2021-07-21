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
}
