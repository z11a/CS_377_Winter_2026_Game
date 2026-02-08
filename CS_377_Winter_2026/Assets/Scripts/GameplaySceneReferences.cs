using System.Collections.Generic;
using UnityEngine;

public class GameplaySceneReferences : MonoBehaviour
{
    public static GameplaySceneReferences Instance;

    public Transform player1Spawn;
    public Transform player2Spawn;
    public List<Transform> itemSpawnLocations;

    private void Awake()
    {
        Instance = this;
    }
}
