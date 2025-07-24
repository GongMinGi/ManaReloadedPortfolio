using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable
{

    [field: SerializeField] public float HP { get; set; } = 500f;

    /// <summary>
    /// - 해당 오브젝트에 데미지를 입힌다
    /// - 외부에서 호출한다.
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log($"남은 체력: {HP}");
    }


}
