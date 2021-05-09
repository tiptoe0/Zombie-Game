using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//데미지를 입을 수 있는 타입들이 공통적으로 가지는 인터페이스
public interface IDamageable {
    //데미지 크기, 맞은 지점, 맞은 표면의방향을 받음
    void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
}
