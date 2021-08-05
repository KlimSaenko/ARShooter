using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Weapons;

public class NetworkedWeaponHolder : MonoBehaviourPunCallbacks
{
    // protected override void Awake()
    // {
    //     if (photonView.IsMine) UI.WeaponHolderScript = this;
    // }
    //
    // public override void Shoot(bool start)
    // {
    //     CurrentWeaponShoot = start;
    //     photonView.RPC("ShootServer", RpcTarget.All, start);
    // }
    //
    // [PunRPC]
    // private void ShootServer(bool start)
    // {
    //     if (!SwitchAnimation && MainWeapons.TryGetValue(CurrentWeaponIndex, out MainWeapon mainWeaponScript)) mainWeaponScript.Shoot(start, photonView.IsMine);
    // }
    //
    // public override void SwitchWeapon(int toWeaponIndex)
    // {
    //     if (toWeaponIndex != CurrentWeaponIndex)
    //     {
    //         photonView.RPC("SwitchWeaponServer", RpcTarget.All, toWeaponIndex);
    //     }
    // }
    //
    // [PunRPC]
    // private void SwitchWeaponServer(int toWeaponIndex)
    // {
    //     CurrentWeaponIndex = toWeaponIndex;
    //     CurrentWeaponScript.Shoot(false);
    //     StopAllCoroutines();
    //     MainWeapons.TryGetValue(CurrentWeaponIndex, out MainWeapon newWeaponScript);
    // }
}
