using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float damage = 10.0f;
    public float maxTravelTime = 15.0f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator ProjectileMove(Vector3 direction, float strength)
    {
        yield return null;

        float travelTime = 0.0f;

        while (travelTime < maxTravelTime)
        {
            yield return null;
            rb.MovePosition(transform.position + direction * strength * Time.deltaTime);
            travelTime += Time.deltaTime;
        }
        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerHandler playerHit = other.GetComponent<PlayerHandler>();
        Debug.Log("Hit something");
        if (playerHit != null)
        {
            playerHit.TakeDamage(damage);
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}
