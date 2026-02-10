using UnityEngine;

public interface IItem
{
    public enum ItemState
    {
        NotCollected,
        Collected
    }

    ItemState _ItemState { get; set; }
}
