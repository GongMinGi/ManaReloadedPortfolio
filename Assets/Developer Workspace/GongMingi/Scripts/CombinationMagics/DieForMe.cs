using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class DieForMe : BaseCombinationMagic
{
    [Header("Projectile")]
    [SerializeField] private DieForMeGroundProjectile groundProjectile;
    private DieForMeGroundProjectile instanceProjectile;

    [Header("Targeting")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxRayDistance = 1000f;
    [SerializeField] private float fallbackPlaneY = 0f;

    [Header("AreaSetting")]
    [SerializeField] private float groundRadius = 2.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("time Bomb Settings")]
    [SerializeField] private float bombDuration = 4f;
    [SerializeField] private int bombDamage = 6666;
    [SerializeField] private int bombEffectId = 99;

    private Camera cam;

    public void Init()
    {
        if (GameModeManager.PoolManager.HasPool(groundProjectile))
        {
            return;
        }
        GameModeManager.PoolManager.CreatePool(groundProjectile, 5, 10);
    }

    public override void ExecuteSkill()
    {
        Init();

        if(canUseSkill == false)
        {
            return;
        }

        base.ExecuteSkill();
        cam = Camera.main;
        if(cam == null)
        {
            return; 
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out var hit, maxRayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0, fallbackPlaneY, 0));
            if(plane.Raycast(ray, out float dist))
            {
                return;
            }
            targetPoint = ray.GetPoint(dist);
        }

        PooledObject go = GameModeManager.PoolManager.GetPool(groundProjectile, targetPoint, Quaternion.identity);
        instanceProjectile = go as DieForMeGroundProjectile;

        var parameters = new DieForMeParam
        {
            center = targetPoint,
            radius = groundRadius,
            enemyLayer = enemyLayer,
            bombDuration = bombDuration,
            bombDamage = bombDamage,
            effectId = bombEffectId,
        };

        instanceProjectile.Setup(parameters);
    }
}
