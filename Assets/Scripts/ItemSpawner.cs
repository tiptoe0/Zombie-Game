using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;   //내비매쉬 관련
using Photon.Pun;

//추가적으로아이템을 플레이어 근처에 스폰
public class ItemSpawner : MonoBehaviourPun
{
    public GameObject[] items;  //생성할 아이템
    public Transform playerTransform;   //플레이어의 트랜스폼

    public float maxDistance = 5f;  //플레이어 위치로부터 아이템이 배치될 최대 반경

    public float timeBetSpawnMax = 7f;  //최대 간격 시간
    public float timeBetSpawnMin = 2f;  //최소 간격 시간
    private float timeBetSpawn; //생성 간격

    private float lastSpawnTime;    //마지막 생성 시점

    private void Start()
    {
        //생성 간격과 마지막 생성 시점 초기화
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = 0;
    }
    //주기적으로 아이템 생성 처리 실행
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) { return; }  //호스트에서만 아이템 직접생성 가능

        //현재 시점이 마지막 생성 시점에서 생성 주기 이상 지남
        //&&플레이어 캐릭터가 존재함
        if (Time.time >= lastSpawnTime + timeBetSpawn) {
            lastSpawnTime = Time.time;  //마지막 생성시간 갱신
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);  //생성주기 설정
            Spawn();    //아이템 생성
        }
    }
    private void Spawn()
    {
        //0,0,0을 기준으로 maxDistance안에 내비메시 위 랜덤위치 지정
        Vector3 spawnPosition = GetRandomPointOnNavMesh(Vector3.zero, maxDistance);

        spawnPosition += Vector3.up * 0.5f; //바닥에서 0.5만큼 위로 올림

        //아이템 중 하나를 무작위로골라 랜덤위치 생성
        GameObject itemToCreate = items[Random.Range(0, items.Length)];
        GameObject item = PhotonNetwork.Instantiate(itemToCreate.name, spawnPosition, Quaternion.identity);

        StartCoroutine(DestroyAfter(item, 5f));  //5초뒤 아이템 파괴
    }

    IEnumerator DestroyAfter(GameObject target, float delay) {
        yield return new WaitForSeconds(delay);
        if (target != null) { PhotonNetwork.Destroy(target); }
    }


    //center을 중심으로 distance 반경 안에 랜덤한 위치를 찾음
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) {
        //center을 중심으로 반지름이 maxDistance인 구 안에서 랜덤한 위치를 하나 저장
        Vector2 randomPos = Random.insideUnitSphere * distance + center;
        NavMeshHit hit;         //내비메시 샘플링 결과 정보 저장
        //maxDistance반경 안에서 randompos에 가장 가까운 내비 메시 위 한점을 찾음
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);
        return hit.position;    //찾은점 반환
    }
}
