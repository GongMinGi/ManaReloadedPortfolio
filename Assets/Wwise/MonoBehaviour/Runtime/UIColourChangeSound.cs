using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

[RequireComponent(typeof(Image))]
public class UIColourChangeSound : MonoBehaviour
{
    // Wwise 이벤트 할당을 위한 public 변수
    public AK.Wwise.Event onColorChangeFromBlackEvent;

    private Image image;
    private Color previousColor;

    private void Awake()
    {
        // 스크립트가 붙어있는 GameObject에서 Image 컴포넌트를 가져옵니다.
        image = GetComponent<Image>();
        
        // 초기 색상을 저장합니다.
        if (image != null)
        {
            previousColor = image.color;
        }
    }

    private void Update()
    {
        if (image == null)
            return;

        Color currentColor = image.color;

        // 현재 색상이 이전 색상과 다르고, 이전 색상이 검정색일 때
        if (currentColor != previousColor && IsBlack(previousColor))
        {
            // Wwise 이벤트를 재생합니다.
            if (onColorChangeFromBlackEvent != null)
            {
                onColorChangeFromBlackEvent.Post(gameObject);
            }
        }

        // 현재 색상을 이전 색상으로 업데이트합니다.
        previousColor = currentColor;
    }

    // 색상이 검정색인지 확인하는 헬퍼 함수
    private bool IsBlack(Color color)
    {
        // 검정색의 RGB 값은 0, 0, 0 이지만, 부동 소수점 오차를 고려해 약간의 허용 오차를 줍니다.
        return color.r < 0.01f && color.g < 0.01f && color.b < 0.01f;
    }
}