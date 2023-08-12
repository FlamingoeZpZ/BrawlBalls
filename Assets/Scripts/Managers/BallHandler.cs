using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class BallHandler : NetworkBehaviour
{

    public static BallHandler Instance { get; private set; }
    
    

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    
    public Ball[] SpawnBalls()
    {
        Ball[] balls = new Ball[3];
        int i = 0;
        foreach (PlayerBallInfo.BallStructure bs in PlayerBallInfo.Balls)
        {
            Vector3 p = Level.Instance.PodiumPoints[i].position;
            Ball b = Instantiate(GameManager.Balls[bs.Ball], p, Quaternion.LookRotation(Vector3.up));
            Instantiate(GameManager.Hull, b.transform).GetComponent<MeshRenderer>().material = b.BaseMaterial;
            Instantiate(GameManager.Weapons[bs.Weapon],b.transform);
            b.SetAbility(GameManager.Abilities[bs.Ability]);
            balls[i++] = b;
        }
        return balls;
        /*
        foreach (PlayerBallInfo.BallStructure b in PlayerBallInfo.Balls)
        {
            print("Attempting to spawn ball.." + b.Ball);
            SpawnBallServerRpc(b.Ball, b.Weapon);
        }*/
        //The ability can be saved entirely locally.
    }


    private static int _ballsSpawned;
    [ServerRpc(RequireOwnership = false)]
    public void SpawnBallServerRpc(string ball, string weapon, ServerRpcParams id =default)
    {
        
        print("Ball successfully spawned: " + id.Receive.SenderClientId);
        Ball b = Instantiate(GameManager.Balls[ball]);
        NetworkObject nb = b.GetComponent<NetworkObject>();
        nb.SpawnAsPlayerObject(id.Receive.SenderClientId, true);
        
        NetworkObject hull = Instantiate(GameManager.Hull, Level.GetNextSpawnPoint(), Quaternion.LookRotation(Vector3.zero));
        hull.SpawnWithOwnership(id.Receive.SenderClientId, true);
        hull.TrySetParent(nb);
        hull.ChangeOwnership(id.Receive.SenderClientId); //?
        
        Weapon w = Instantiate(GameManager.Weapons[weapon]);
        NetworkObject nw = w.GetComponent<NetworkObject>();
        nw.SpawnWithOwnership(id.Receive.SenderClientId, true);
        
        nw.TrySetParent(nb);

        b.FinalizeClientRpc();
        
        //b.SetAbility(GameManager.Abilities[ability]);
        
        _ballsSpawned += 1;
        print("Clients spawned: " + _ballsSpawned);
    }
}
