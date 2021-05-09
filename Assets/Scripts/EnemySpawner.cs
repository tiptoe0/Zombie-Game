using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;

public class EnemySpawner : MonoBehaviour, IPunObservable
{
    public Enemy enemyPrefab;
    public Transform[] spawnPoints;

    public float damageMax = 40f;
    public float damageMin = 20;

    public float healthMax = 200f;
    public float healthMin = 100;

    public float speedMax = 3f;
    public float speedMin = 1f;

    public Color stringEnemyColor = Color.red;

    private List<Enemy> enemies = new List<Enemy>();
    private int enemyCount = 0; //남은적의 수
    private int wave;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting)
        {
            stream.SendNext(enemies.Count);
            stream.SendNext(wave);
        }
        else {
            enemyCount = (int)stream.ReceiveNext();
            wave = (int)stream.ReceiveNext();
        }
    }

    void Awake() {
        PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }

    private void Update()
    {
        //호스트만 적 생성
        if (PhotonNetwork.IsMasterClient) {
            if (GameManager.instance != null && GameManager.instance.isGameover) { return;  }
            if (enemies.Count <= 0) { SpawnWave(); }
        }
        UpdateUI();
    }

    private void UpdateUI() {
        if (PhotonNetwork.IsMasterClient) { UIManager.instance.UpdateWaveText(wave, enemies.Count); }
        else { UIManager.instance.UpdateWaveText(wave, enemyCount); }
    }

    private void SpawnWave() {
        wave++;
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);
        for (int i = 0; i < spawnCount; i++) {
            float enemyIntensity = Random.Range(0f, 1f);
            CreateEnemy(enemyIntensity);
        }
    }

    private void CreateEnemy(float intensity) {
        //능력치 결정
        float health = Mathf.Lerp(healthMin, healthMax, intensity);
        float damage = Mathf.Lerp(damageMin, damageMax, intensity);
        float speed = Mathf.Lerp(speedMin, speedMax, intensity);

        Color skinColor = Color.Lerp(Color.white, stringEnemyColor, intensity); //피부색 결정

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];    //생성 위치 결정

        GameObject createdEnemy = PhotonNetwork.Instantiate(enemyPrefab.gameObject.name, spawnPoint.position, spawnPoint.rotation);
        Enemy enemy = createdEnemy.GetComponent<Enemy>();

        enemy.photonView.RPC("Setup", RpcTarget.All, health, damage, speed, skinColor);

        enemies.Add(enemy);

        enemy.onDeath += () => enemies.Remove(enemy);

        enemy.onDeath += () => Destroy(enemy.gameObject, 10f);

        enemy.onDeath += () => GameManager.instance.AddScore(100);
    }

    IEnumerator DestoryAfter(GameObject target, float delay) {
        yield return new WaitForSeconds(delay);
        if (target != null) {
            PhotonNetwork.Destroy(target);
        }
    }
}
