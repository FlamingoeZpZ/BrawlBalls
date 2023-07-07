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
    
    private Player previousAttacker;

    private MeshRenderer mr;
    private int ballLayer;
    private int groundlayers;

    public float Speed => Velocity.magnitude;
    public Vector3 Velocity => rb.velocity;


     public float Acceleration { get; private set; }


     private void Awake()
    {
        //if the ball is the local player?
        //if the PLAYER is the local player, then it should move THIS ball...
        if (IsLocalPlayer)
        {
            print("Am I local?");
        }

        Transform t = transform.GetChild(0);
        
        groundlayers = GameManager.GroundLayers +  (1<< t.gameObject.layer);
        Acceleration = stats.Acceleration;

        rb = t.GetComponent<Rigidbody>();
        mr = t.GetComponent<MeshRenderer>();
        rb.drag = stats.Drag;
        rb.angularDrag = stats.AngularDrag;
        rb.mass = stats.Mass + weapon.Mass;

        currentHealth = stats.MaxHealth;
    }

     private void FixedUpdate()
     {
         HandleDrag();
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

    private void HandleDrag()
    {
        bool hit = Physics.Raycast(rb.position, Vector3.down, out RaycastHit h, 1.5f, groundlayers);
        //#if UNITY_EDITOR
        Debug.DrawRay(rb.position, Vector3.down * 1.5f, hit?Color.blue:Color.yellow);
        //#endif
        //Handle squishing
        if (hit)
        {
            Transform n = h.transform.parent;
            if (n && n.TryGetComponent(out Ball b))
            {
                Debug.LogWarning("LANDED ON EM: " + b.name +", " + name);
                b.TakeDamage(1000000, Vector3.zero, Player.LocalPlayer);
                rb.drag = stats.Drag;
            }
            else
            {
                rb.drag =  0;
            }
        }
        else
        {
            rb.drag =  0;
        }

        
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
        int l = mr.materials.Length;
        Material[] mats = new Material[l+1];
        for (int index = 0; index < l; index++)
        {
            mats [index]= mr.materials[index];
        }

        Material createdMat = new Material(ParticleManager.GlueBallMat);
        
        //Kill me :(
        createdMat.SetFloat(ParticleManager.ColorID, m.GetFloat(ParticleManager.ColorID));
        createdMat.SetInt(ParticleManager.RandomTexID, m.GetInt(ParticleManager.RandomTexID));
        createdMat.SetVector(ParticleManager.RandomOffsetID, m.GetVector(ParticleManager.RandomOffsetID));
        
        mats[l]=createdMat;

        mr.materials = mats;


    }


}
