using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : Singleton<Test>
{
    [SerializeField] private GameObject prefab;

    internal void Ok()
    {
        print(prefab.name);
        //Instantiate(prefab);
    }

    private protected override void Init()
    {
        print(prefab.name);
        Instance.prefab = prefab;
    }
}
