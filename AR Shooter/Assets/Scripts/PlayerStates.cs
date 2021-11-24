using System;
using UnityEngine;

public class PlayerStates : StateMachineBehaviour
{
    internal static event Action<bool> WeaponReadyAction;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) =>
        WeaponReadyAction?.Invoke(false);

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) =>
        WeaponReadyAction?.Invoke(true);
}
