using System.Collections;
using UnityEngine;

public class Pistol : Gun
{
    protected override void OnShoot()
    {
        Vector3 direction = GetShootDirection();
        Ray ray = new Ray(Camera.main.transform.position, direction);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, Data.shootingRange))
        {
            targetPoint = hit.point;
            Debug.Log("HIT: " + hit.collider.name);
            SpawnHitFX(hit);
        }
        else
        {
            targetPoint = ray.origin + ray.direction * Data.shootingRange;
        }

        StartCoroutine(BulletTrailRoutine(targetPoint));
    }

    private IEnumerator BulletTrailRoutine(Vector3 target)
    {
        GameObject trail = Instantiate(
            Data.bulletTrailPrefab,
            Muzzle.position,
            Quaternion.identity
        );

        while (Vector3.Distance(trail.transform.position, target) > 0.05f)
        {
            trail.transform.position = Vector3.MoveTowards(
                trail.transform.position,
                target,
                Data.bulletSpeed * Time.deltaTime
            );
            yield return null;
        }

        Destroy(trail);
    }
}
