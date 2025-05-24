using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float hitForce = 10f; // Сила удара по кубикам
    
    void OnCollisionEnter(Collision collision)
    {
        // Проверяем, что столкнулись с кубиком
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody cubeRb = collision.gameObject.GetComponent<Rigidbody>();
            
            // Применяем силу к кубику в направлении удара
            cubeRb.AddForceAtPosition(
                GetComponent<Rigidbody>().linearVelocity.normalized * hitForce,
                collision.contacts[0].point,
                ForceMode.Impulse
            );
        }
    }
}