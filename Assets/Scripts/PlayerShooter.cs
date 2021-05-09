using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerShooter : MonoBehaviourPun
{
    public Gun gun; //사용할 총
    public Transform gunPivot;  //총 배치 기준점
    public Transform leftHandMount; //총 왼쪽 손잡이
    public Transform rightHandMount;    //총 오른쪽 손잡이

    private PlayerInput playerInput;    //플레이어
    private Animator playerAnimator;    //애니메이터 컴포넌트

    private void Start()
    {   //사용할 컴포넌트 가져옴
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {   //슈터가 활성화 될 때 총도 활성화
        gun.gameObject.SetActive(true);
    }

    private void OnDisable()
    {   //슈터가 비활성화 될 때 총도 비활성화
        gun.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!photonView.IsMine) { return; }

        if (playerInput.fire) { gun.Fire(); }   //입력을 감지하고 총 발사
        else if (playerInput.reload) {  //재장전 감지하고 재장전
            if (gun.Reload()) {
                playerAnimator.SetTrigger("Reload");    //재장전 성공시에만 재장전 애니메이션 실행
            }
        }
       // UpdateUI();
    }

    //private void UpdateUI() {
    //    if (gun != null && UIManager.instance != null) {
    //        UIManager.instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
    //    }
    //}

    private void OnAnimatorIK(int layerIndex)
    {
        //총의 기준점을 3D모델의 오른쪽 팔꿈치 위로 이동
        gunPivot.position = playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow);

        //IK를 사용하여 왼손의 위치와 회전을 왼쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        //IK를 사용하여 오른손의 위치와 회전을 오른쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);
    }
}
