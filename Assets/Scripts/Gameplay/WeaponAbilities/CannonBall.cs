using UnityEngine;

public class CannonBall : Ball
{
    private const float MaxDist = 10;

    public override void OnDestroy()
    {
        base.OnDestroy();
        Vector3 pos = transform.GetChild(0).position;
        Collider[] cols=Physics.OverlapSphere(pos, 5, GameManager.PlayerLayers);
        foreach (Collider c in cols)
        {
            Vector3 ePos = c.ClosestPoint(pos);
            Vector3 dir = ePos - pos;
            float damage = ParticleManager.EvalauteExplosiveDistance(dir.magnitude / MaxDist)*200;
            c.transform.parent.GetComponent<Ball>().TakeDamage(damage, damage * dir, Player.LocalPlayer);
        }
    }
}
