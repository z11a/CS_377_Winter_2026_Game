using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public interface IWeapon : IItem
{
    GameObject owner { get; set; }

    IEnumerator attackCoroutine { get; set; }
    void Attack();

    void PickupWeapon(GameObject _owner);
    void DropWeapon();
}