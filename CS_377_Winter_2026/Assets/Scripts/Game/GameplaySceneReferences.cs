using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameplaySceneReferences : MonoBehaviour
{
    public static GameplaySceneReferences instance;

    public Transform player1Spawn;
    public Transform player2Spawn;
    public List<Transform> playerSpawnLocations;
    public List<GameObject> itemSpawners;

    private void Awake()
    {
        instance = this;
    }
}
