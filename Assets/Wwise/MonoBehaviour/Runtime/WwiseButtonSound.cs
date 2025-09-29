using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

[RequireComponent(typeof(Button))]
public class WwiseButtonSound : MonoBehaviour
{
    // Wwise 이벤트 할당을 위한 public 변수
    public AK.Wwise.Event onClickEvent;

    private Button button;

    private void Awake()
    {
        // 스크립트가 붙어있는 GameObject에서 Button 컴포넌트를 가져옵니다.
        button = GetComponent<Button>();
        
        // 버튼의 OnClick 리스너에 사운드 재생 함수를 추가합니다.
        if (button != null && onClickEvent != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    // 버튼이 클릭될 때 호출될 함수
    private void PlaySound()
    {
        // Wwise 이벤트를 재생합니다.
        onClickEvent.Post(gameObject);
    }

    private void OnDestroy()
    {
        // GameObject가 파괴될 때 메모리 누수를 방지하기 위해 리스너를 제거합니다.
        if (button != null)
        {
            button.onClick.RemoveListener(PlaySound);
        }
    }
}