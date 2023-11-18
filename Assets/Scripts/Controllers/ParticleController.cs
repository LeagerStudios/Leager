using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour {


    IEnumerator Particle()
    {
        yield return new WaitForSeconds(Random.Range(1.1f, 4f));
        Destroy(gameObject);
    }

    void Update()
    {
        transform.Translate(new Vector2(0, Random.Range(0.1f, 2f) * Time.deltaTime));
    }

    public void Spawn()
    {
        transform.Translate(new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
        StartCoroutine(Particle());
    }
}
