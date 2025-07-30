using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// * 개발자 : 공민기
/// - IDamageable 인터페이스를 상속
/// - Take Damage를 통해 체력/ 데미지를 관리하고 CurrentHp가 0이되면 Ondie 이벤트 호출
/// </summary>
public class EnemyStats : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float MaxHp { get; set; } = 500f;
    [field: SerializeField] public float CurrentHp { get; set; }

    [SerializeField] private UnityEvent OnDamaged = new();
    [SerializeField] private UnityEvent OnDie = new();

    #region Unity Event
    private void Awake()
    {
        CurrentHp = MaxHp;
    }
    #endregion

    /// <summary>
    /// - 해당 오브젝트에 데미지를 입힌다
    /// - 외부에서 호출한다.
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;
        Debug.Log($"남은 체력: {CurrentHp}");

        if (CurrentHp <= 0f)
            OnDie?.Invoke();
        else
            OnDamaged?.Invoke();
    }
}