using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Balls;
using Managers;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

namespace Level
{
    public class Level : NetworkBehaviour
    {
        [SerializeField] private float bottomY;
        [SerializeField] private bool offMapKills;
        public static Level Instance { get; private set; }
        [field: SerializeField] public Transform [] PodiumPoints { get; private set; }
        [field: SerializeField] public Transform [] SpawnPoints { get; private set; }
    

        [SerializeField] private float distance;
        [SerializeField] private float travelTime;
        [SerializeField] private Transform coinStart;
        [SerializeField] private Scoreboard scoreboard;
    
        private Transform _coin;

        private static int SpawnedIdx { get; set; }
        public static Vector3 GetNextSpawnPoint() => Instance.SpawnPoints[SpawnedIdx++ % Instance.SpawnPoints.Length].position;
    


        private void SpawnCoin()
        {
            Debug.Log("Spawning Map Coin");
            int r = Random.Range(0, 100);
            GameObject spawned;
            switch (r)
            {
                case <= 10:
                    spawned = ParticleManager.SummonObjects["SpecialCoin"];
                    break;
                case <= 35:
                    spawned = ParticleManager.SummonObjects["WeaponCoin"];
                    break;
                case <= 70:
                    spawned = ParticleManager.SummonObjects["BallCoin"];
                    break;
                default:
                    spawned = ParticleManager.SummonObjects["AbilityCoin"];
                    break;
            }

            //This is only running on server anyways.
            SendMessageClientRpc("A <color=#d4bb00>coin</color> has spawned", 2);
            _coin = Instantiate(spawned, coinStart.position, Quaternion.identity).transform;

            _coin.GetComponent<PositionConstraint>().constraintActive = false;
            _coin.GetComponent<NetworkObject>().Spawn();
        }

        [ClientRpc]
        public void SendMessageClientRpc(string s, float d, ClientRpcParams x = default) => _ = MessageHandler.Instance.HandleScreenMessage(s, d);


        //All levels drop coins from center...
        private readonly HashSet<ulong> _readyPlayers = new();
        private void Awake()
        {
            if(Instance != null) Destroy(Instance.gameObject);
            Instance = this;

        
            //When the player awakes... the server will   
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckGameStartServerRpc(ServerRpcParams @params = default)
        {
            _readyPlayers.Add(@params.Receive.SenderClientId);
            CheckStartGame();
        }

        void CheckStartGame()
        {
            print("Checking players connected: " + _readyPlayers.Count + " == " + NetworkManager.ConnectedClients.Count);
            if (_readyPlayers.Count == NetworkManager.ConnectedClients.Count)
            {
            
                StartGameClientRpc();
                _ = CoinTravel();
            }
        
        }

        private void Start()
        {
        
            print("Level awake");
        
            _readyPlayers.Add(OwnerClientId);
            CheckGameStartServerRpc();
        
            if(!IsOwner) return;
            print("SpawningCoin");
        
        
        
            NetworkManager.OnClientConnectedCallback += id =>
            {
                print("Player connected: " + id);
            };
       
            NetworkManager.OnClientDisconnectCallback += id =>
            {
                print("Player disconnected: " + id);
                _readyPlayers.Remove(id);
                CheckStartGame();
            };
        }

        void Update()
        {
            if (!BallPlayer.Alive) return;
            //Every player is responsible for checking if they've fallen off the map? Or should it be the server... probably the server...
            if (BallPlayer.LocalBallPlayer.BallY < bottomY)
            {
                BallPlayer.LocalBallPlayer.Respawn(offMapKills);
            }
        }

        private async UniTask CoinTravel()
        {
            await UniTask.Delay(30000);
            SpawnCoin();
            float curTravelTime = 0;
            float y = coinStart.position.y;
            while (curTravelTime < travelTime && _coin) //Logic to just check that the coin hasn't been destroyed, or reached it's destination
            {
            
                curTravelTime += Time.deltaTime;
                _coin.position = y * Vector3.up +(distance * (curTravelTime / travelTime) * Vector3.down);
                await UniTask.Yield();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayParticleGlobally_ServerRpc(string particleName, Vector3 location) => PlayParticleGlobally_ClientRpc(particleName, location);
    
        [ServerRpc(RequireOwnership = false)]
        public void SpawnObjectGlobally_ServerRpc(string objectName, Vector3 location, ServerRpcParams @params = default)
        {
            GameObject go = Instantiate(ParticleManager.SummonObjects[objectName], location, Quaternion.identity).gameObject;
            go.GetComponent<NetworkObject>().SpawnWithOwnership(@params.Receive.SenderClientId);
        }

        [ClientRpc]
        private void PlayParticleGlobally_ClientRpc(string particleName, Vector3 location) => ParticleManager.InvokeParticle(particleName, location);



        [ClientRpc]
        private void StartGameClientRpc()
        {
            //Then we go back to a server rpc :(
            scoreboard.Initialize();
            BallPlayer.LocalBallPlayer.Initialize();
            LoadingHelper.Deactivate();
            GameManager.GameStarted = true;
        }


#if UNITY_EDITOR
        [SerializeField] private bool display;
        private void OnDrawGizmos()
        {
            if (!display) return;
            Gizmos.color = Color.black;
            Gizmos.DrawCube(Vector3.up * bottomY, new Vector3(100000f,0.1f,100000f));
            Gizmos.color = Color.green;
            Vector3 position = coinStart.position;
            Gizmos.DrawRay(position, Vector3.down * distance);
            Gizmos.DrawSphere(position, 4);
        }
#endif
    
    }
}
