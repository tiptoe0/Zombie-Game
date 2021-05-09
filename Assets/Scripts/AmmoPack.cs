using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AmmoPack : MonoBehaviourPun, IItem
{
    public int ammo = 30;
    public void Use(GameObject target)
    {
        PlayerShooter playerShooter = target.GetComponent<PlayerShooter>(); //전달받은 게임 오브젝트로 부터 playershooter컴포넌트 가져옴
        if (playerShooter != null && playerShooter.gun != null)
        {
            //오브젝트가 존재하면
            playerShooter.gun.photonView.RPC("AddAmmo", RpcTarget.All, ammo);   //총 남은 탄환수를 ammo만큼 더하기, 모든 클라이언트 실행
        }
        PhotonNetwork.Destroy(gameObject);  //모든 클라이언트에서 자신 파괴
    }
}
