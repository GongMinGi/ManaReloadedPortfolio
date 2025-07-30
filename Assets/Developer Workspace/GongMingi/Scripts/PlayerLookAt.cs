using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookAt : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] float turnSmoothTime = 0.05f;
    private bool isDie = false;


    private void Awake()
    {
        playerCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        PlayerRotate();   
    }

    private void PlayerRotate()
    {
        if (isDie) return;


        Vector3 screenPos = Mouse.current.position.ReadValue();             
        screenPos.z = playerCamera.WorldToScreenPoint(transform.position).z;
        //Vector3 mousePos  = Camera.main.ScreenToWorldPoint(screenPos);
        Vector3 mousePos = playerCamera.ScreenToWorldPoint(screenPos);

        Vector3 rotationDir = mousePos - transform.position;
        rotationDir.y = 0;
        if (rotationDir.sqrMagnitude < 0.1f) return;

        //transform.rotation = Quaternion.LookRotation(rotationDir);

        Quaternion targetRot = Quaternion.LookRotation(rotationDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime / turnSmoothTime);


    }
    
    public void OnDie()
    {
        isDie = true;
    }

}
