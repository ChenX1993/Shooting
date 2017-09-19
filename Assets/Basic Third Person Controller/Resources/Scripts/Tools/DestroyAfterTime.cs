using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour {

    public float time;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (transform.parent == null)
        {
            Destroy(gameObject, time);
        }
	}
}
