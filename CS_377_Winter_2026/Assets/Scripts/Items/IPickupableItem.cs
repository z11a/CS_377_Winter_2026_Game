using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;

public interface IPickupableItem : IItem
{
    GameObject owner { get; set; }
    void Use();
    IEnumerator useCoroutine { get; set; }
    void PickupItem(GameObject _owner);
    void DropItem();
    BoxCollider unequippedCollider { get; set; }
    Rigidbody rb { get; set; }
}
