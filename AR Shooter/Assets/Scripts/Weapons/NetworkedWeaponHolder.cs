using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedWeaponHolder : WeaponHolderAndSwitcher
{
    protected override void Awake()
    {
        if (photonView.IsMine) UI.weaponHolderScript = this;
    }

    public override void Shoot(bool start)
    {
        currentWeaponShoot = start;
        photonView.RPC("ShootServer", RpcTarget.All, start);
    }

    [PunRPC]
    private void ShootServer(bool start)
    {
        if (!switchAnimation && mainWeapons.TryGetValue(currentWeaponIndex, out MainWeapon mainWeaponScript)) mainWeaponScript.Shoot(start, photonView.IsMine);
    }

    public override void SwitchWeapon(int toWeaponIndex)
    {
        if (toWeaponIndex != currentWeaponIndex)
        {
            photonView.RPC("SwitchWeaponServer", RpcTarget.All, toWeaponIndex);
        }
    }

    [PunRPC]
    private void SwitchWeaponServer(int toWeaponIndex)
    {
        currentWeaponIndex = toWeaponIndex;
        currentWeaponScript.Shoot(false);
        StopAllCoroutines();
        mainWeapons.TryGetValue(currentWeaponIndex, out MainWeapon newWeaponScript);
        StartCoroutine(Switching(newWeaponScript));
    }
}
