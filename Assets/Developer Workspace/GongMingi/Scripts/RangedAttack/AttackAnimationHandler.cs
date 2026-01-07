using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationHandler : IDisposable
{
    private readonly IAnimationDriver animDriver;                                       // 인터페이스만 바라보고 있기 때문에, 애니메이터를 바꾼다고해도 그 애니메이터가 SetBool과 같이 같은 이름의 함수만 가지고 있으면 바로 교체가능                                       // Animator를 감싼 추상 드라이버
    private readonly List<AttackBinding> attackBindings = new();                        // 이벤트 구독 핸들 보관

    public AttackAnimationHandler(IAnimationDriver driver) => animDriver = driver;

    public void Register(IAttackSignal attackSignal)
    {
        AttackBinding attackBinding = new AttackBinding(attackSignal);                              // 실제 구독/해제를 관리할 AttackBinding 인스턴스 생성

        // 행동 지침(Action) 정의: "이 함수를 실행하면 애니메이션이 제어된다"
        if (attackSignal.AnimationBoolHash.HasValue)
        {
            int hash = attackSignal.AnimationBoolHash.Value;
            attackBinding.PlayStartAnimation = () => animDriver.SetBool(hash, true);               // 공격 시작 => bool ON
            attackBinding.PlayEndAnimation = () => animDriver.SetBool(hash, false);                // 정상 종료 => Bool OFF
            attackBinding.PlayInterruptedAnimation = () => animDriver.SetBool(hash, false);          // 강제 취소 => Bool OFF
        }
        
        // 즉발형 애니메이션 처리
        if (attackSignal.AnimationTriggerHash.HasValue)
        {
            attackBinding.PlayStartAnimation += () => animDriver.SetTrigger(attackSignal.AnimationTriggerHash.Value);
        }

        // 진행도가 존재하는 애니메이션 처리
        if (attackSignal.UseProgress)                                                               // 차지, 채널링 진행도 매핑
        {
            attackBinding.PlayProgressAnimation = (attackProgressedRate) => animDriver.SetFloat(AnimParams.RangedProgress, attackProgressedRate);
        }

        attackBinding.Subscribe();                                                                   // AttackBinding에서 실제 event와 실행할 Action을 연결
        attackBindings.Add(attackBinding);                                                           // 컨트롤러에서 Dispose 할 수 있도록 반환
    }

    public void Dispose()
    {
        foreach (var binding in attackBindings)
        {
            binding.Dispose();
        }

        attackBindings.Clear();
    }

    /// <summary>
    /// 이벤트를 모아서 구독하고 해제하기 위해 만든 IDisposable 클래스
    /// 추후 다른 종류의 IDisposable과 모아서 한꺼번에 해제 가능
    /// </summary>
    private sealed class AttackBinding : IDisposable                                    // IDisposable을 구현한 이벤트 - 바인딩 한 덩어리 를 나타내는 내부 전용(sealed) 클래스
    {
        public static readonly AttackBinding Empty = new(null);                         // 시그널이 아예 없는 경우에 쓰는 싱글턴 더미 객체 

        private readonly IAttackSignal attackSignal;                                    // 실제로 구독할 공격 시그널(started, end 등 )을 보관하는 읽기 전용 참조
        public Action PlayStartAnimation;                                               // 공격이 시작될 때 호출될 델리게이트(Action)
        public Action PlayEndAnimation;                                                 // 공격이 정상 종료될 때 호출
        public Action PlayInterruptedAnimation;                                         // 외부 요인(Stop 등)으로 끊겼을때 호출
        public Action<float> PlayProgressAnimation;                                     // 차지, 채널링 진행도(0~1)를 전달 ( 필요시 사용) 

        public AttackBinding(IAttackSignal signal) => attackSignal = signal;            // 생성자. 컨트롤러가 전달한 signal을 받아온다.

        public void Subscribe()                                                         // signal(이벤트)과 그 이벤트에서 호출할 함수(Acition)을 묶는다.
        {
            // 배선 연결 : "방송(Event)이들리면 -> 이 행동 (Action)을 해라
            if (attackSignal == null) { return; }                                       // signal(이벤트)이 없으면 아무것도 하지 않음
            if (PlayStartAnimation            != null) { attackSignal.OnAttackStarted     += PlayStartAnimation; }          // 시작 이벤트 연결
            if (PlayEndAnimation              != null) { attackSignal.OnAttackEnded       += PlayEndAnimation; }            // 종료 이벤트 연결
            if (PlayInterruptedAnimation      != null) { attackSignal.OnAttackInterrupted += PlayInterruptedAnimation; }    // 취소 이벤트 연결    
            if (PlayProgressAnimation         != null) { attackSignal.OnProgressUpdated   += PlayProgressAnimation; }       // 진행도 이벤트 연결
        }

        public void Dispose()                                                                                               // IDisposeable을 이용하여 안전하게 구독 해제 
        {
            if (attackSignal == null) { return; }                                                                           // 이미 해제된 경우 바로 종료 
            if (PlayStartAnimation            != null) { attackSignal.OnAttackStarted     -= PlayStartAnimation; }          // 시작 이벤트 해제
            if (PlayEndAnimation              != null) { attackSignal.OnAttackEnded       -= PlayEndAnimation; }            // 종료 이벤트 해제
            if (PlayInterruptedAnimation      != null) { attackSignal.OnAttackInterrupted -= PlayInterruptedAnimation; }    // 취소 이벤트 해제
            if (PlayProgressAnimation         != null) { attackSignal.OnProgressUpdated   -= PlayProgressAnimation; }       // 진행도 이벤트 해제
            PlayStartAnimation = PlayEndAnimation = PlayInterruptedAnimation = null;                                        // 델리게이트 참조를 제거해 GC 대상화
            PlayProgressAnimation = null;
        }
    }
}
