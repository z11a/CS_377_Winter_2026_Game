using UnityEngine;

public interface IWeapon : IItem
{
    Coroutine attackCoroutine { get; set; }

    void Attack();
}