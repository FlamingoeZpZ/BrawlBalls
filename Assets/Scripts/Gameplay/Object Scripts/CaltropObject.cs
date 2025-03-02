using Gameplay.Balls;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class CaltropObject : PlaceableObject
    {

        private void Start()
        {
            if(!IsOwnedByServer) return;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);
        }

        protected override void OnHit(NetworkBall hit)
        {
            //Again verify with upgrades and whatnot...
            //hit.TakeDamage(50, 15, owner.player);
            hit.TakeDamageClientRpc(hit.Speed*3, hit.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId);
        }
    }
}
