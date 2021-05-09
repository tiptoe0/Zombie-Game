using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }
            return m_instance;
        }
    }

    private static GameManager m_instance;  //싱글톤이 할당될 static 변수

    public GameObject playerPrefab;    //생성할 플레이어 캐릭터 프리팹

    private int score = 0;
    public bool isGameover { get; private set; }

    //주기적으로 자동 실행, 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) { stream.SendNext(score); }   //로컬 오브젝트라면 네트워크로 score값 보냄
        else {
            score = (int)stream.ReceiveNext();
            UIManager.instance.UpdateScoreText(score);
        }
    }

    private void Awake()
    {
        if (instance != this) { Destroy(gameObject); }
    }

    private void Start()
    {
        //생성할 랜덤 위치 생성
        Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;  //생성할 랜덤 위치
        randomSpawnPos.y = 0f;  //위치 y값은 0

        //네트워크 상 모든 클라이언트에서 실행
        PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
    }

    public void AddScore(int newSocre) {
        if (!isGameover) {
            score += newSocre;
            UIManager.instance.UpdateScoreText(score);
        }
    }

    public void EndGame() {
        isGameover = true;
        UIManager.instance.SetActiveGameoverUI(true);
    }

    // ESC누르면 꺼지도록 함
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { PhotonNetwork.LeaveRoom(); }
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
