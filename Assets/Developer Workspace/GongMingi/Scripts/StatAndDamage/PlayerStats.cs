using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float HP { get; set; } = 100;        // Property 변수는 field: 를 붙여야 인스펙터에서 볼 수 있다.
    [SerializeField] public UnityEvent OnDie = new(); 

    /// <summary>
    ///  - 해당 오브젝트에 데미지를 입힌다.
    ///  - 외부에서 호출
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
            OnDie?.Invoke();
        }
        Debug.Log($"남은 체력: {HP}");

    }

}
