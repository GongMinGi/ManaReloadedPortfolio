using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// * 개발자 : 공민기
/// - 플레이어의 체력을 관리하는 스크립트
/// - IDamageable 인터페이스를 상속
/// - Take Damage를 통해 체력/ 데미지를 관리하고 CurrentHp가 0이되면 Ondie 이벤트 호출
/// - Slilder UI를 통해서 currentHp / MaxHp 를 화면에 띄운다.
/// </summary>
public class PlayerStats : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float MaxHp { get; set; } = 100;        // Property 변수는 field: 를 붙여야 인스펙터에서 볼 수 있다.
    [field: SerializeField] public float CurrentHp { get; set; }
    [SerializeField] public UnityEvent OnDie = new();
    [SerializeField] public Slider playerHPBar;


    private void Awake()
    {
        CurrentHp = MaxHp;
        playerHPBar.maxValue = MaxHp;
        playerHPBar.value = CurrentHp;
    }


    /// <summary>
    ///  - 해당 오브젝트에 데미지를 입힌다.
    ///  - 외부에서 호출
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;

        playerHPBar.value = CurrentHp;

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            OnDie?.Invoke();
        }
        Debug.Log($"남은 체력: {CurrentHp}");

    }

}
