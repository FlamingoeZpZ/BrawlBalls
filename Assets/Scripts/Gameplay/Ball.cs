using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    [SerializeField] private Weapon weapon;
    [SerializeField] private AbilityStats ability;

    private Rigidbody rb;
    private float currentHealth;

    [SerializeField] private VisualEffect onDestroy;
    public Weapon Weapon => weapon; // I really don't want to have to do this...
    public AbilityStats SpecialAbility => ability;
    public float MaxSpeed => stats.MaxSpeed;
    public float Drag => stats.Drag;
    
    private Player previousAttacker;

    private MeshRenderer mr;


    public float Speed => rb.velocity.magnitude;


     public float Acceleration { get; private set; }



    private void Awake()
    {
        //if the ball is the local player?
        //if the PLAYER is the local player, then it should move THIS ball...
        if (IsLocalPlayer)
        {
            print("Am I local?");
        }

        Acceleration = stats.Acceleration;

        rb = transform.GetChild(0).GetComponent<Rigidbody>();
        mr = transform.GetChild(0).GetComponent<MeshRenderer>();
        rb.drag = stats.Drag;
        rb.angularDrag = stats.AngularDrag;
        rb.mass = stats.Mass + weapon.Mass;

        currentHealth = stats.MaxHealth;
    }
    
    
    
    public void AddVelocity(Vector3 dir)
    {
        rb.AddForce(dir, ForceMode.Impulse);
    }

    /* Just take the collision point duh
    public void TakeDamage(float amount, float forceMul, Player attacker)
    {
        //Just push in negative direction
        TakeDamage(amount, -rb.velocity.normalized * forceMul, attacker);
    } */

    public void TakeDamage(float amount, Vector3 direction, Player attacker)
    {
        currentHealth = Mathf.Max(currentHealth-amount, stats.MaxHealth);
        print( name + "Ouchie! I took damage: " + amount +",  " + direction);
        if (currentHealth <= 0)
        {
            previousAttacker = attacker;
            Die();
            //return;
        }
        rb.AddForce(direction, ForceMode.Impulse);

    }

    private bool isDead;

    private void Die()
    {
        if (!isDead) return;
        isDead = true;
        
        if (previousAttacker)
        {
            //previousAttacker.AwardKill();
            //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
        }
        //Destroy(gameObject);
    }

    public void ApplySlow(Ball attacker, Material m)
    {
        //previousAttacker = attacker.owner
        Acceleration *= 0.7f;
        List<Material> mats = mr.materials.ToList();
        mats.Add(m);
        mr.SetMaterials(mats);
        
        
    }


}
