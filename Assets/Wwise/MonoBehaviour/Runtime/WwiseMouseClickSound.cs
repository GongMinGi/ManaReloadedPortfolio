using UnityEngine;
using AK.Wwise;

/// <summary>
/// This MonoBehaviour script posts a Wwise event when the mouse is clicked.
/// </summary>
public class WwiseMouseClickSound : MonoBehaviour
{
    // Wwise 이벤트를 Inspector에서 설정하기 위한 public 변수
    [Tooltip("Wwise Event to post when the mouse is clicked.")]
    public AK.Wwise.Event onClickEvent;

    private void Update()
    {
        // 왼쪽 마우스 버튼이 눌리는 순간을 감지합니다.
        if (Input.GetMouseButtonDown(0))
        {
            // Inspector에 할당된 Wwise Event가 있다면 재생합니다.
            if (onClickEvent != null)
            {
                onClickEvent.Post(gameObject);
            }
        }
    }
}