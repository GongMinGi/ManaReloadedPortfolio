using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float HP { get; set; } = 100;        // Property 변수는 field: 를 붙여야 인스펙터에서 볼 수 있다.

    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log($"남은 체력: {HP}");

    }

}
