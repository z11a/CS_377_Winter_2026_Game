using UnityEngine;

public interface IItem
{
    public enum ItemState
    {
        NotCollected,
        Collected
    }

    ItemState _ItemState { get; set; }  // item state should only be "NotCollected" if it just spawned and hasn't been interacted with. An item will not switch back to being "Uncollected".

    Vector3 initialSpawnPosition { get; set; }
}
