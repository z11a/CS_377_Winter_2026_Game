using UnityEngine;

public interface IItem
{
    public enum ItemState
    {
        NotCollected,
        Collected
    }

    ItemState _ItemState { get; set; }

    Vector3 spawnPosition { get; set; }
}
