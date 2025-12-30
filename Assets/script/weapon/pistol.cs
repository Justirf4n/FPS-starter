using System.Collections;
using UnityEngine;

public class Pistol : Gun
{
    #region Shoot
    protected override void OnShoot()
    {
        Vector3 camDir = GetShootDirection();
        Ray camRay = new Ray(CameraTransform.position, camDir);

        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit hit, Data.shootingRange))
        {
            targetPoint = hit.point;
            SpawnHitFX(hit);
        }
        else
        {
            targetPoint = camRay.origin + camRay.direction * Data.shootingRange;
        }

        Vector3 muzzleToTargetDir = (targetPoint - Muzzle.position).normalized;

        StartCoroutine(BulletTrailRoutine(targetPoint, muzzleToTargetDir));
    }
    #endregion

    #region Bullet Trail
    private IEnumerator BulletTrailRoutine(Vector3 target, Vector3 dir)
    {
        GameObject trail = BulletTrailPool.Instance.Get();
        trail.transform.position = Muzzle.position;
        trail.transform.rotation = Quaternion.LookRotation(dir);

        TrailRenderer tr = trail.GetComponent<TrailRenderer>();
        if (tr != null)
        {
            tr.Clear();
            tr.time = 0f;
            tr.time = 0.1f;
        }

        while (Vector3.Distance(trail.transform.position, target) > 0.05f)
        {
            trail.transform.position = Vector3.MoveTowards(
                trail.transform.position,
                target,
                Data.bulletSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
    #endregion
}
