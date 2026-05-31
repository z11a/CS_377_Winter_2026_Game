using UnityEngine;
using System.Collections;
using static UnityEngine.UI.GridLayoutGroup;

public class Projectile : MonoBehaviour
{
    public float damage = 10.0f;
    public float knockbackStrength = 20.0f;
    public float knockbackDuration = 0.25f;
    public float maxTravelTime = 15.0f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ProjectileMove(Vector3 direction, float strength)
    {
            StartCoroutine(ProjectileMoveCoroutine(direction, strength));
        //rb.AddForce(direction * strength, ForceMode.Impulse);
    }
    public IEnumerator ProjectileMoveCoroutine(Vector3 direction, float strength)
    {
        yield return null;

        float travelTime = 0.0f;

        while (travelTime < maxTravelTime)
        {
            yield return null;
            if (this == null) { yield break; }
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

            Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
            playerHit.TakeKnockback(knockbackDirection, knockbackDuration, knockbackStrength);
        }
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
