using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHealth : LivingEntity
{
    public Slider healthSlider; //체력을 표시할 UI슬라이더
    public AudioClip deathClip; //사망소리
    public AudioClip hitClip;   //피격 소리
    public AudioClip itemPickupClip;    //아이템 습득 소리
    private AudioSource playerAudioPlayer;  //플레이어 소리 재생기
    private Animator playerAnimator;    //플레이어의 애니메이터
    private PlayerMovement playerMovement;  //플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter;    //플레이어 슈터 컴포넌트

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        healthSlider.gameObject.SetActive(true);    //체력 슬라이더 활성화
        healthSlider.maxValue = startingHealth; //체력 슬라이더 최댓값 기본 체력값
        healthSlider.value = health;    //체력 슬라이더 값 현재 체력 값
        playerMovement.enabled = true;
        playerShooter.enabled = true;
    }

    [PunRPC]
    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
        healthSlider.value = health;    //갱신된 체력으로 슬라이더 갱신
    }

    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!dead) {
            playerAudioPlayer.PlayOneShot(hitClip); //사망하지 않은 경우 효과음 재생
        }
        base.OnDamage(damage, hitPoint, hitNormal);
        healthSlider.value = health;
    }

    public override void Die()
    {
        base.Die();
        healthSlider.gameObject.SetActive(false);   //체력슬라이더 비활성화
        playerAudioPlayer.PlayOneShot(deathClip);    //사망음 재생
        playerAnimator.SetTrigger("Die");
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        Invoke("Respawn", 5f);  //리스폰
    }

    private void OnTriggerEnter(Collider other)
    {
        //아이템과 충돌한 경우, 사망하지 않은 경우 아이템 사용 가능
        if (!dead) {
            IItem item = other.GetComponent<IItem>();   //컴포넌트 가져오기 시도
            if (item != null) {
                if (PhotonNetwork.IsMasterClient) { item.Use(gameObject); }
                playerAudioPlayer.PlayOneShot(itemPickupClip);  //아이템 습득 소리 재생
            }
        }
    }
    public void Respawn() {
        if (photonView.IsMine) {
            Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
            randomSpawnPos.y = 0f;
            transform.position = randomSpawnPos;
        }
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}
