using System.Collections.Generic;
using UnityEngine;

public class TapUIController : MonoBehaviour
{
    [SerializeField] List<GameObject> showObjList = new();
    [SerializeField] List<GameObject> hideObjList = new();
    
    public void ShowTapUI()
    {
        foreach (GameObject obj in hideObjList)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in showObjList)
        {
            obj.SetActive(true);
        }
    }
}