using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

public class PlayerBehaviour : MonoBehaviour
{
    private void Awake()
    {
        Initialization();
    }

    private static void Initialization()
    {
        Vibration.Init();
    }

    /// <summary>
    /// Invokes when player starts or finishes firing
    /// </summary>
    internal static event Action<bool> FiringAction;
    
    public void OnFiringAction(bool start) =>
        FiringAction?.Invoke(start);
    
    
    internal static event Action<Vector3> HitAction;

    private static void OnHitAction(Vector3 pos) =>
        HitAction?.Invoke(pos);
    
    
    /// <summary>
    /// Invokes when player aims weapon (or goes to free position)
    /// </summary>
    internal static event Action<bool> AimingAction;
    
    public void OnAimingAction(bool toAim) =>
        AimingAction?.Invoke(toAim);
    
    
    /// <summary>
    /// Invokes when player switches weapon
    /// </summary>
    internal static event Action<WeaponType> WeaponSwitchAction;
    
    /// <summary>
    /// 0 - None;
    /// 1 - Pistol;
    /// 2 - M4;
    /// 3 - Shotgun;
    /// 4 - Flamethrower
    /// </summary>
    public void OnWeaponSwitchAction(int toWeapon)
    {
        WeaponSwitchAction?.Invoke((WeaponType)toWeapon);
    }
    
#if UNITY_EDITOR
        
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) OnAimingAction(true);
        else if (Input.GetKeyUp(KeyCode.E)) OnAimingAction(false);

        if (Input.GetKeyDown(KeyCode.Alpha1)) OnWeaponSwitchAction(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) OnWeaponSwitchAction(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) OnWeaponSwitchAction(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) OnWeaponSwitchAction(4);
    }
        
#endif    
}
