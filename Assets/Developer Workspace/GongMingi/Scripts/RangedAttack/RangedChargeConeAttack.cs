using UnityEngine;



/// <summary>
/// * 작성자 : 공민기
///   - 캐스팅한 원소에 전기 속성이 포함되어 잇는 경우 시전되는 원거리 공격
///   - 마우스 좌클릭을 누르는 동안 충전하고 때는 순간에 마법을 발사한다
///   
/// </summary>
public class RangedChargeConeAttack : MonoBehaviour, IRangedAttack
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }



    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public void ExecuteAttack(E_CastingType type)
    {
        throw new System.NotImplementedException();
    }
}
