using UnityEngine;

public static class Extension
{
    /// <summary>
    /// 지정된 LayerMask에 특정 레이어가 포함되어 있는지를 확인하는 메서드.
    /// </summary>
    /// <param name="layerMask">확인 대상이 되는 레이어 마스크</param>
    /// <param name="layer">포함 여부를 확인할 레이어 번호 (0~31)</param>
    /// <returns>
    /// 해당 레이어가 layerMask에 포함되어 있으면 true, 포함되어 있지 않으면 false를 반환함.
    /// </returns>
    public static bool Contain(this LayerMask layerMask, int layer)
    {
        return ((1 << layer) & layerMask) != 0;
    }
}