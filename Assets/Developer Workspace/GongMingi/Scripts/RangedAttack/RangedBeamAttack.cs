using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///   - 캐스팅한 원소에 빛이나 어둠속성이 포함되어 있을 때 실행되는 원거리 공격
///   - 마우스 좌클릭을 누르고 있는 동안 빔 형태의 공격이 계속 나가는 홀드형 공겨
///   - 계속 홀드하고 있더라도 마법 시전시간이 끝나면 공격이 끝난다.
/// </summary>
public class RangedBeamAttack : MonoBehaviour, IRangedAttack
{
    public void ExecuteAttack(E_CastingType type)
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
