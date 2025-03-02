using TMPro;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : NetworkBehaviour
{
    //Being static, means as long as the game doesn't close, you can see who was in the lobby.
    private static readonly ScoreHolders[] Holders = new ScoreHolders[8];
    private static int _playerCount;
    private static ulong _myID;
    private static Scoreboard _sb;

    [SerializeField] private Color myColor;
    [SerializeField] private Color otherColor;

    private static Color _myColor;
    private static Color _otherColor;


    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitialize()
    {
        _playerCount = 0;
        _myID = 0;
        _sb = null;
        _myColor = new Color32(255, 255, 255, 255);
        _otherColor = new Color32(255, 0, 0, 255);
    }


    void Start()
    {
        _myColor = myColor;
        _otherColor = otherColor;
        
        //Store all objects & Reset.
        _playerCount = 0;
        _sb = this;
        for (int i = 0; i < Holders.Length; ++i)
        {
            Holders[i] = new ScoreHolders(transform.GetChild(i));
            Holders[i].Disable();
        }

        _myID = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ctx =>
                AddUserToScoreboardClientRpc(ctx, PlayerBallInfo.UserName);
            NetworkManager.Singleton.OnClientDisconnectCallback += RemoveUserFromScoreboardClientRpc;
        }
        //Unfortunate
        transform.parent.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void RemoveUserFromScoreboardClientRpc(ulong id)
    {
        //Iterate from the top index, to the amount of players in the game.
        for (int index = 0; index < _playerCount; index++)
        {
            //Once we found the player we're looking for.
            if ( Holders[index].Id == id)
            {
                //Iterate through the remaining slots -1 for the removed slot
                while (index < _playerCount-1)
                {
                    //And change the current one, to the next one.
                    Holders[index].ChangeTo(Holders[index + 1]);
                    ++index;
                }
                //Disable the last slot.
                Holders[_playerCount - 1].Disable();
                return;
            }
        }
    }

    [ClientRpc]
    private void AddUserToScoreboardClientRpc(ulong id, string playerName)
    {
        print("adding user to scoreboard: " + playerName);
        Holders[_playerCount++].ChangeTo(playerName, id, 0); //TODO: value will be given by game manager in the future
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(ulong playerID, int change)
    {

        for (int index = 0; index < Holders.Length; index++)
        {
            ScoreHolders current = Holders[index];
            if ( current.Id == playerID)
            {
                print("Updating score: " + (current.Value+change) + " for ID: " + playerID);

                current.ModifyScoreHolder(current.Value+change, index+1);
                
                //Length back down
                while (--index >= 0)
                {
                    ScoreHolders above = Holders[index];
                    if (current.Value > above.Value)
                    {
                        //SWAP
                        Holders[index+1] = above;
                        Holders[index] = current;
                    }
                }
                
                return;
            }
        }
    }



    //Update scores using a bubble up approach.
    public static void UpdateScore(ulong playerID, int change)
    {
        _sb.UpdateScoreClientRpc(playerID, change);
    }


    public void Initialize()
    {
        transform.parent.gameObject.SetActive(true);
        print("Initializing Scoreboard: " + PlayerBallInfo.UserName);
        InitializeServerRpc(PlayerBallInfo.UserName);
    }

    //Anyone can call this.
    [ServerRpc(RequireOwnership = false)]
    private void InitializeServerRpc(string userName, ServerRpcParams caller = default)
    {
        
        print("Initializing Scoreboard locally");
        AddUserToScoreboardClientRpc(caller.Receive.SenderClientId, userName);
    }

    #region ScoreHolderInfo
    private class ScoreHolders
    {
        
        public int Value { get; private set; }
        public ulong Id { get; private set; }
        private string _playerName;
        
        private readonly Image _root;
        private readonly TextMeshProUGUI _scoreText;
        private readonly TextMeshProUGUI _rankText;
        private readonly TextMeshProUGUI _nameText;

        public ScoreHolders(Transform root)
        {
            Value = 0;
            Id = 0;
            _root = root.GetComponent<Image>();
            _rankText = root.GetChild(0).GetComponent<TextMeshProUGUI>();
            _nameText = root.GetChild(1).GetComponent<TextMeshProUGUI>();
            _scoreText = root.GetChild(2).GetComponent<TextMeshProUGUI>();
            _playerName = null;
        }

        public void ChangeTo(string playerName, ulong id, int value)
        {
            _root.gameObject.SetActive(true);
            _playerName = playerName;
            _nameText.text = playerName;
            ModifyScoreHolder(value, _root.transform.GetSiblingIndex()+1);

            _root.color = id == _myID ? _myColor : _otherColor; 
                
            Id = id;
        }

        public void ChangeTo(ScoreHolders other)
        {
            ChangeTo(other._playerName, other.Id, other.Value);
        }


        public void ModifyScoreHolder(int newValue, int rank)
        {
            Value = newValue;
            _rankText.text = rank + ".";
            _scoreText.text = newValue.ToString();
        }


        public void Disable()
        {
            _root.gameObject.SetActive(false);
        }
    }
    #endregion
    
}
