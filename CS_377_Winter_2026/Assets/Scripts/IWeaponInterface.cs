using System.Collections;
using UnityEngine;

public interface IWeapon : IItem
{
    IEnumerator attackCoroutine { get; set; }

    void Attack();
}