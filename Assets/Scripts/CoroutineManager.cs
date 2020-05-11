using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager
{
    public Coroutine coroutine { get; private set; }
    public object result = null;
    public bool isDone = false; 
    private IEnumerator target;

    public CoroutineManager(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while(target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        isDone = true;
    }


}
