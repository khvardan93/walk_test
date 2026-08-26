using Fusion;
using UnityEngine;

public class GameManager : MonoBehaviour, INetworkInput
{
    [SerializeField] private GameObject _playerPrefab;  // SapphiArtchan prefab
    [SerializeField] private Transform _spawnPoint;     // Where to spawn player
    
    private NetworkRunner _runner;
    private bool _playerSpawned = false;
    
    private async void Start()
    {
        await StartNetworkGame();
    }
    
    private async System.Threading.Tasks.Task StartNetworkGame()
    {
        // Create a new NetworkRunner
        var runnerGO = new GameObject("NetworkRunner");
        _runner = runnerGO.AddComponent<NetworkRunner>();
        
        // Add InputHandler to NetworkRunner
        runnerGO.AddComponent<InputHandler>();
        
        // Configure the runner
        var args = new StartGameArgs();
        args.GameMode = GameMode.Shared;
        args.SessionName = "CorpseParty";
        args.PlayerCount = 2;
        
        // Start the game
        await _runner.StartGame(args);
        
        Debug.Log("Photon Fusion Started!");
    }
    
    private void Update()
    {
        // Spawn player once when game starts
        if (_runner != null && !_playerSpawned && _runner.IsRunning)
        {
            SpawnPlayer();
            _playerSpawned = true;
        }
    }
    
    private void SpawnPlayer()
    {
        Vector3 pos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
        Quaternion rot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;
        
        // Spawn the player prefab
        var playerInstance = Instantiate(_playerPrefab, pos, rot);
        
        // Make sure it has the NetworkPlayer script
        if (playerInstance.GetComponent<NetworkPlayer>() == null)
        {
            playerInstance.AddComponent<NetworkPlayer>();
        }
        
        Debug.Log("Player spawned at " + pos);
    }
}