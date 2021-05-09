using System.Collections;
using System.Collections.Generic;
using System;
using Photon.Pun;
using UnityEngine;

public class LivingEntity : MonoBehaviourPun, IDamageable
{
    public float startingHealth = 100f; //시작 체력
    public float health { get; protected set; } //현재 체력
    public bool dead { get; protected set; }    //사망 상태
    public event Action onDeath;    //사망시 발동할 이벤트

    //호스트
    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead) {
        health = newHealth;
        dead = newDead;
    }

    protected virtual void OnEnable() {
        //생명체가 활성화 될때 상태를 리셋
        dead = false;   //사망하지 않은 상태로 시작
        health = startingHealth;    //체력을 시작 체력으로 초기화
    }

    [PunRPC]
    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            health -= damage;//데미지를 입는 기능
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, health, dead);
            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPoint, hitNormal);
        }

        if (health <= 0 && !dead) { Die(); }    //체력이 0이하고 죽지 않으면 사망처리
    }

    [PunRPC]
    public virtual void RestoreHealth(float newHealth) {
        //체력을 회복하는 기능
        if (dead) { return; }   //이미 사망한경우 체력 회복 불가

        if (PhotonNetwork.IsMasterClient) {
            health += newHealth;    //체력 추가
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, health, dead);
            photonView.RPC("RestoreHealth", RpcTarget.Others, newHealth);
        }
        
    }

    public virtual void Die() {
        //사망 처리
        if (onDeath != null) { onDeath(); } //onDeath 이벤트에 등록된 메서드가 있으면 실행
        dead = true;    //사망상태 참
    }
}
