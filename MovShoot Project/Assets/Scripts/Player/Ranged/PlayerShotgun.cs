using UnityEngine;

public class PlayerShotgun : PlayerThrow
{
    public int shotgunDamage = 5;
    public float shotgunRange;
    public float shotgunSpread;

    public LayerMask enemyLayer;
    public void shotgunBlast()
    {
        Physics.SphereCast(cam.transform.position, shotgunSpread, cam.transform.forward, out var hit, shotgunRange, enemyLayer);

        Collider[] hitEnemies = Physics.OverlapCapsule(cam.transform.position, hit.point, shotgunRange, enemyLayer);
        foreach (Collider Enemy in hitEnemies)
        {
            if (!Enemy.TryGetComponent(out EnemyHealth health)) continue;

            health.TakeDamage(shotgunDamage);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        
    }
}
