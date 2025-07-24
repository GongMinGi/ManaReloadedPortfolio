using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable
{

    [field: SerializeField] public float HP { get; set; } = 500f;

    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log($"남은 체력: {HP}");
    }


}
