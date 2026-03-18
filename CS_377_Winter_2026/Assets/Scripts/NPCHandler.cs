using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCHandler : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(NPCAnimationCycle());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator NPCAnimationCycle()
    {
        yield return null;

        while (true)
        {
            int randomAnimationInt = Random.Range(0, 4);
            animator.SetInteger("Cheer", randomAnimationInt);

            float randomAnimationLength = Random.Range(0.5f, 5f);
            yield return new WaitForSeconds(randomAnimationLength);
        }
    }
}
