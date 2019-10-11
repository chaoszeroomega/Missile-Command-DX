﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    //Serialize field values are present for easy adjustment in the inspector.
    [SerializeField] private float speed = 5f;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private GameObject explosionPrefab = null;
    //[SerializeField] private GameObject collisionSplosion = null;
    GameObject[] defenders; 
    Transform target = null;
    Animator death = null;
    GameController controller = null;
    private AudioSource deathsound = null;
    // Start is called before the first frame update
    void Start()
    {
        defenders = GameObject.FindGameObjectsWithTag("Defenders");
        int ran = Random.Range(0, defenders.Length);
        controller = FindObjectOfType<GameController>();
        target = defenders[ran].transform; //This gets all of the defenders, setting them as potential attackable objects
        death = defenders[ran].GetComponent<Animator>(); //This gets the animator of the chosen defender, making it so they change state when they're hit
        speed = controller.enemymissileSpeed;
        deathsound = defenders[ran].GetComponent<AudioSource>();

        //Rotation calcs. I had to get someone else's advice for this part as I'm not good with math, so trigonometry is something I'll
        //have to revise for my own studies. 
        Vector2 direction = (target.position - transform.position);
        float angle = 0;
        if ( direction.x > 0 )
            angle = 90 - Mathf.Acos(direction.x / direction.magnitude) / Mathf.PI * 180.0f;
        else
            angle =  (Mathf.Acos(-direction.x / direction.magnitude) / Mathf.PI * 180.0f - 90);

        transform.rotation = Quaternion.Euler(0, 0, angle );

    }

    // Update is called once per frame
    void Update()
    {
        //Makes the enemy missile move towards the selected allied city. 
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        
        checkDistance();
        
    }


    
    void checkDistance()
    {
        float distance = Vector3.Distance(gameObject.transform.position, target.transform.position);
        //Debug.Log(distance);
        if (distance < radius) //Note: the cities only get up to a distance of 0.6 for some reason, so the radius has to be more than that.
        {
            
            //Debug.Log("Hit " + target.gameObject.name);
            Object.Instantiate(explosionPrefab, new Vector3( transform.position.x, transform.position.y, -2), Quaternion.identity);
            Destroy(gameObject);
            if ((target.name == "MidSilo" || target.name == "LeftSilo" || target.name == "RightSilo") && death.GetCurrentAnimatorStateInfo(0).IsName("siloalive") )
            {
                controller.MissileDestroyedSilo();
                death.SetTrigger("DeathTrigger");
                deathsound.Play();
            }
            else if (death.GetCurrentAnimatorStateInfo(0).IsName("cityaliveidle"))
            {
                controller.MissileDestroyedCity();
                death.SetTrigger("DeathTrigger");
                deathsound.Play();
            }
            else
            {
                controller.MissileHitDead();
            }
            
        }
    }

    /*void checkInterrupt()
    {
        if (transform.position < )
    }
    */

    /*void contactHit(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        int i = 0;
        while (i < hitColliders.Length)
        {

            Debug.Log("Hit " + hitColliders[i].gameObject.tag);
            ++i;
           // hitColliders[i].SendMessage("methodName"); Calls every moinobehaviour (class) in this object. Probably won't be used. 
        }
    }

    */


    /*private void OnTriggerEnter2D(Collider2D collision) - This code doesn't work because I can't have rigidbodies
    {
        Debug.Log("Hit something");
        if (collision.tag == "Defenders")
        {
            Debug.Log("Enemy Missile hit defender");
        }
        else
        {
            Debug.Log("Whoops.");
        }
    } */
}
