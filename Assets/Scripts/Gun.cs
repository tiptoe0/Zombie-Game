using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Gun : MonoBehaviourPun, IPunObservable
{
    public enum State {
        Ready,  //발사 준비
        Empty,  //탄창 빔
        Relodading  //재장전
    }

    public State state { get; private set; }    //현재 총 상태

    public Transform fireTransform; //총알 발사될 위치

    public ParticleSystem muzzleFlashEffect;    //총구 화염 효과
    public ParticleSystem shellEjectEffect;     //탄피 배출 효과

    private LineRenderer bulletLineRenderer;    //총알 궤적 렌더러

    private AudioSource gunAudioPlayer; //총 소리
    public AudioClip shotClip;          //발사 소리
    public AudioClip reloadClip;        //재장전 소리

    public float damage = 25;           //공격력

    private float fireDistance = 50f;   //사정거리

    public int ammoRemain = 100;    //남은 전체 탄창
    public int magCapacity = 25;    //탄창 용량
    public int magAmmo;             //현재 탄창에 남아있는 탄약
    
    public float timeBetFire = 0.12f;   //총알 발사 간격
    public float reloadTime = 1.8f;     //재장전 소요
    private float lastFireTime;         //총을 마지막으로 발사한 시점

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting)   //로컬오브젝트면 쓰기 부분 실행
        {
            stream.SendNext(ammoRemain);    //남은 탄약수
            stream.SendNext(magAmmo);       //탄창의 탄약수
            stream.SendNext(state);     //현재 총 상태
        }
        else {  
            //리모트 오브젝트인 경우
            ammoRemain = (int)stream.ReceiveNext(); //받음
            magAmmo = (int)stream.ReceiveNext();    //받음
            state = (State)stream.ReceiveNext();    //받음
        }
    }

    [PunRPC]
    public void AddAmmo(int ammo) {
        ammoRemain += ammo;
    }

    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();
        bulletLineRenderer.positionCount = 2;   //사용할 점 2개
        bulletLineRenderer.enabled = false; //라인 렌더링 비활성화
    }

    private void OnEnable()
    { //총 상태 초기화
        magAmmo = magCapacity;  //현재 탄창 가득 채움
        state = State.Ready;    //준비 상태 설정
        lastFireTime = 0;       //총쏜 시점 초기화
    }

    public void Fire() {
        if (state == State.Ready && Time.time >= lastFireTime + timeBetFire) {
            //발사 가능한 상태와 마지막 총 발사 시점에서 총알 발사 간격 지남
            lastFireTime = Time.time;   //마지막 총 발사 시점 갱신
            Shot(); //실제 발사
        }
    }

    private void Shot() {
        photonView.RPC("ShotProcessOnServer", RpcTarget.MasterClient);

        magAmmo--;  //남은 탄알 -1
        if (magAmmo <= 0) { state = State.Empty; }  //만약 탄알 없으면 비어있는상태로 갱신
    }

    [PunRPC]
    private void ShotProcessOnServer() {
        RaycastHit hit;
        Vector3 hitPosition = Vector3.zero;

        if (Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null) { target.OnDamage(damage, hit.point, hit.normal); }   

            hitPosition = hit.point;
        }

        else {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        photonView.RPC("ShotEffectProcessOnClients", RpcTarget.All, hitPosition);   //발사 이펙트 모든 클라이언트 재생
    }

    [PunRPC]
    private void ShotEffectProcessOnClients(Vector3 hitPosition) {  //이벤트 재생 코루틴 랩핑
        StartCoroutine(ShotEffect(hitPosition));
    }

    private IEnumerator ShotEffect(Vector3 hitPosition) {
        muzzleFlashEffect.Play();   //화염 발생
        shellEjectEffect.Play();    //배출 효과 발생
        gunAudioPlayer.PlayOneShot(shotClip);   //총격 소리 발생
        bulletLineRenderer.SetPosition(0, fireTransform.position);  //선의 시작점 : 총구 위치
        bulletLineRenderer.SetPosition(1, hitPosition); //선의 끝점 : 입력으로 들어온 충돌 위치
        bulletLineRenderer.enabled = true;  //라인 렌더러를 활성화해 총알 궤적 그림

        yield return new WaitForSeconds(0.03f); //0.03초 동안 처리 대기

        bulletLineRenderer.enabled = false; //라인 렌더러 비활성화 총알 궤적 삭제
    }

    public bool Reload() {
        //재장전
        if (state == State.Relodading || ammoRemain <= 0 || magAmmo >= magCapacity)
        {
            return false;   //재장전 중이거나 탄알이 없거나 탄알이 가득한 경우 재장전 불가
        }

        StartCoroutine(ReloadRoutine());    //재장전 시작
        return true;
    }

    private IEnumerator ReloadRoutine() { 
        //실제 재장전 처리
        state = State.Relodading;   //재장전 상태로 전환
        gunAudioPlayer.PlayOneShot(reloadClip); //재장전 소리 재생

        yield return new WaitForSeconds(reloadTime);    //재장전 소요시간만큼 처리 쉼

        int ammoToFill = magCapacity - magAmmo; //탄창에 채울 탄알 계산

        //탄창에 채워야할 탄알이 남은 탄알보다 많으면 채워야할 탄안수를 남은 탄알에 맞춰 줄임
        if (ammoRemain < ammoToFill) { ammoToFill = ammoRemain; }

        magAmmo += ammoToFill;  //탄창을 채움

        ammoRemain -= ammoToFill;   //남은 탄알에서 탄창에 채운만큼 뺌

        state = State.Ready;    //발사 준비 상태 전환
    }
}
