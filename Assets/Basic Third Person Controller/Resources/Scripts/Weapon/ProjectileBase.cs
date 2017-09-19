using UnityEngine;
using System.Collections;

public class ProjectileBase : MonoBehaviour {

    public float force = 2000;
    public DoForce addForce = DoForce.AtStart;

    private Rigidbody rb;

    public float damage = 70;
    public float explosionForce = 1000;
    public float explosionRadius = 30;

    public GameObject particle;
    public GameObject trail;

    public enum DoForce
    {
        AtStart,
        InFixedUpdate
    }

    // Use this for initialization
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        if (addForce == DoForce.AtStart)
        {
            rb.AddForce(transform.forward * force, ForceMode.Acceleration);
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (addForce == DoForce.InFixedUpdate)
        {
            rb.AddForce(transform.forward * force, ForceMode.Acceleration);
        }
	}
    void OnCollisionEnter()
    {
        //get all the colliders inside the radius
        Collider[] collider = Physics.OverlapSphere(transform.position, explosionRadius);

        for(int i = 0; i < collider.Length; i++)
        {
            //
            
            //your code


            //
            Rigidbody r = collider[i].GetComponent<Rigidbody>();
            if (r)
            {
                PlayerBehaviour pB = r.GetComponent<PlayerBehaviour>();

                if (pB)
                {
                    pB.Damage(damage / Vector3.Distance(transform.position, pB.transform.position));
                    if (!pB.ragdollh.ragdolled)
                    {
                        pB.ToggleRagdoll();
                       
                    }
                }
                r.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        Instantiate(particle, transform.position, Quaternion.identity);
        if (trail)
        {
            trail.transform.parent = null;
        }
        Destroy(gameObject);
    }
}
