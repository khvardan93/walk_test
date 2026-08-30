using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    
    private void Start()
    {
        SpawnPlayer();
    }
    
    private void SpawnPlayer()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        var playerInstance = Instantiate(playerPrefab, pos, rot);
        
        CharacterMovement charMovement = playerInstance.GetComponent<CharacterMovement>();
        if (charMovement == null) charMovement = playerInstance.AddComponent<CharacterMovement>();
        
        NetworkPlayer netPlayer = playerInstance.GetComponent<NetworkPlayer>();
        if (netPlayer == null) netPlayer = playerInstance.AddComponent<NetworkPlayer>();
        
        Debug.Log("Player spawned at " + pos);
        GameEvents.CharacterReady(charMovement);
    }
}