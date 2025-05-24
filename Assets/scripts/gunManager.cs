using UnityEngine;

public class gunManager : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject projectile;
    public Transform spawnPoint;
    public float firePower = 2000f;
    
    private float can_fire = 0;

    // Update is called once per frame
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (spawnPoint == null)
        {
            spawnPoint = transform.Find("SpawnPoint");
            if (spawnPoint == null)
            {
                Debug.LogError("Spawn point not found! Create a child object named 'SpawnPoint'");
            }
        }
    }

    void Update()
    {
        float fire = Input.GetAxis("Fire1");
        if (fire == 1 && fire != can_fire && spawnPoint != null)
        {
            Fire();
        }
        can_fire = fire;
    }

    void Fire()
    {
        if (projectile == null)
        {
            Debug.LogError("Projectile prefab not assigned!");
            return;
        }

        GameObject projectile_clone = Instantiate(projectile, spawnPoint.position, spawnPoint.rotation);
        Rigidbody projectileRb = projectile_clone.GetComponent<Rigidbody>();

        if (projectileRb == null)
        {
            projectileRb = projectile_clone.AddComponent<Rigidbody>();
        }

        projectileRb.AddForce(spawnPoint.forward * firePower);
    }
}
