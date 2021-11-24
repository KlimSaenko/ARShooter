using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Player;
using Game.UI;

public class NetworkedPlayerStatus : PlayerStatus
{
    public override int HP { get => networkedHP; set => base.HP = value; }
    public int networkedHP = 100;

    private int deathCounter = 0;

    private PhotonView photonView;

    private void Awake()
    {
        networkedHP = playerHP;
        photonView = transform.GetComponent<PhotonView>();
    }

    public override void ApplyDamage(int damage)
    {
        if (networkedHP - damage <= 0)
        {
            deathCounter++;

            CommonUI.KillsUI(deathCounter);
            Debug.Log(deathCounter);
        }

        photonView.RPC("ApplyDamageServer", RpcTarget.All, damage);
    }

    [PunRPC]
    private void ApplyDamageServer(int damage)
    {
        networkedHP -= damage;
        if (!IsAlive)
        {
            foreach (Collider hitBox in colliders) hitBox.enabled = false;
            Handheld.Vibrate();

            if (hpImages != null)
            {
                hpImages.gameObject.SetActive(false);

                StartCoroutine(Death(1));
            }
        }
        else if (hpImages != null)
        {
            HpUI();
        }
    }

    public void Respawn()
    {
        photonView.RPC("RespawnServer", RpcTarget.All);
    }

    [PunRPC]
    private void RespawnServer()
    {
        networkedHP = playerHP;
        currentHP = 96;

        if (hpImages != null) hpImages.gameObject.SetActive(true);
        foreach (Collider hitBox in colliders) hitBox.enabled = true;

        if (photonView.IsMine)
        {
            CommonUI.AliveStateUI.SetActive(true);
            CommonUI.DeadStateUI.SetActive(false);
        }
    }
}
