using System;
using UnityEngine;
using Game.Weapons;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private Animator playerStateMachine;
    
    internal static Animator PlayerStateMachine { get; private set; }

    private void Awake()
    {
        Initialization();
    }

    private void Initialization()
    {
        PlayerStateMachine = playerStateMachine;
    }

    /// <summary>
    /// Invokes when player starts or finishes firing
    /// </summary>
    internal static event Action<bool> FiringAction;
    internal static bool Firing;

    private static int _firingPointers;
    private static void SetFiringPointers(int value)
    {
        value += _firingPointers;

        if (value <= 0)
        {
            _firingPointers = 0;
            Firing = false;
            FiringAction?.Invoke(false);
        }
        else
        {
            _firingPointers = value;
            Firing = true;
            FiringAction?.Invoke(true);
        }
    }

    public void OnFiringAction(bool start)
    {
        SetFiringPointers(start ? 1 : -1);
    }
    
    /// <summary>
    /// Invokes when player aims weapon (or goes to free position)
    /// </summary>
    internal static event Action<bool> AimingAction;

    private static readonly int aimedId = Animator.StringToHash("Aimed");

    private static int _aimingPointers;
    private static void SetAimingPointers(int value)
    {
        value += _aimingPointers;

        if (value <= 0)
        {
            _aimingPointers = 0;
            PlayerStateMachine.SetBool(aimedId, false);
            AimingAction?.Invoke(false);
        }
        else
        {
            _aimingPointers = value;
            PlayerStateMachine.SetBool(aimedId, true);
            AimingAction?.Invoke(true);
        }
    }

    public void OnAimingAction(bool toAim)
    {
        //AimingAction?.Invoke(toAim);
        SetAimingPointers(toAim ? 1 : -1);
    }
    
    /// <summary>
    /// Invokes when player switches weapon
    /// </summary>
    internal static event Action<WeaponName> WeaponSwitchAction;

    private static readonly int weaponIndexId = Animator.StringToHash("Weapon Index");
    /// <summary>
    /// 0 - None;
    /// 1 - Pistol;
    /// 2 - M4;
    /// 3 - Shotgun;
    /// 4 - Flamethrower
    /// </summary>
    public static void OnWeaponSwitchAction(int toWeapon)
    {
        //WeaponSwitchAction?.Invoke((WeaponType)toWeapon);
        PlayerStateMachine?.SetInteger(weaponIndexId, toWeapon);
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
        // else if ((Input.GetKeyDown(KeyCode.S))) OnFiringAction(true);
    }
        
#endif    
}
