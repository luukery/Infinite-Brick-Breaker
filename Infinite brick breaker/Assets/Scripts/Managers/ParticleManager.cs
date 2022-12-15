using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
/// <summary>
/// The ParticleManager tracks all the particle systems and effects and makes sure that they're used correctly. 
/// Only sad thing is at the moment I only use 1 particle effect so it's a bit of overkill that I made an entire manager for it that handles and tracks it.
/// </summary>
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    public GameObject[] particleEffects;    //use for generating everyting in the next list

    List<GameObject> activeParticleEffects = new List<GameObject>();    //use for the ingame

    private void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        GameObject effectHolder = new GameObject("Particle effects holder");

        for (int i = 0; i < particleEffects.Length; i++)    //spawns in all particle effects (without triggering them)
        {
            GameObject e = Instantiate(particleEffects[i], Vector2.zero, Quaternion.identity);
            e.transform.parent = effectHolder.transform;
            activeParticleEffects.Add(e);
        }
    }

 
    public void ExplodingTile(Vector2 location)     //plays exploding Tile effect.
    {
        activeParticleEffects[0].transform.position = location;
        activeParticleEffects[0].GetComponent<ParticleSystem>().Play();
    }

    //this was build for more effects, but in the end there was only one.
}
