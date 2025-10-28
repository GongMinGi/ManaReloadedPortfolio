using Game.Combat.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이정
/// 어둠 속성 원거리 투사체
/// </summary>
public class RangedDarkProjectile : AbstractProjectile
{
    private ProjectileParams rangedDarkParam;
    private HashSet<UnitStats> hitUnits = new HashSet<UnitStats>();
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + transform.forward * rangedDarkParam.speed * Time.fixedDeltaTime);
    }

    public override void Setup(ProjectileParams p)
    {
        rangedDarkParam = p;
        autoRelease = true;
        releaseTime = p.maxRange / p.speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & rangedDarkParam.enemyL) != 0) // 적 레이어와 충돌했는지 확인
        {
            UnitStats unitStats = other.GetComponent<UnitStats>();
            if (unitStats != null && hitUnits.Contains(unitStats) == false)
            {
                hitUnits.Add(unitStats); // 이미 맞은 유닛으로 추가
                unitStats.TakeDamage(rangedDarkParam.damage);
            }
        }
    }

    protected override void OnDeactivated(Action onComplete = null)
    {
        hitUnits.Clear();
        base.OnDeactivated(onComplete);
    }
}

