using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICastingDisplay : MonoBehaviour
{

    #region field and property
    [SerializeField] private Image[] slotImages;
    [SerializeField] private Color defaultColor = Color.black;

    [System.Serializable] private struct ElementColor
    {
        public E_CastingType type;
        public Color color;
    }

    [SerializeField] private ElementColor[] elementColors;

    private readonly Dictionary<E_CastingType, Color> elementColorMap = new();


    #endregion

    private void Awake()
    {
        foreach(var element in elementColors)
        {
            elementColorMap[element.type] = element.color;
        }
        ResetAll();
    }



    public void UpdateSlot(E_CastingType type, int index)
    {
        if (index < 0 || index >= slotImages.Length) return;

        Color baseCol = elementColorMap.TryGetValue(type, out var color) ? color : Color.white;

        baseCol.a = 1f;

        slotImages[index].color = baseCol;
    }


    public void ResetAll()
    {
        foreach (Image img in slotImages) img.color = defaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
