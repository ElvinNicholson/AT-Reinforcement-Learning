using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private Transform spawnMax;
    [SerializeField] private Transform spawnMin;

    private void Start()
    {
        RandomizePos();
    }

    public void RandomizePos()
    {
        Vector3 pos = GetRandomPos();
        while (Vector3.Distance(pos, transform.localPosition) < 1f)
        {
            pos = GetRandomPos();
        }

        transform.localPosition = pos;
    }

    private Vector3 GetRandomPos()
    {
        Vector3 newPos = transform.localPosition;
        newPos.x = Random.Range(spawnMin.localPosition.x, spawnMax.localPosition.x);
        newPos.z = Random.Range(spawnMin.localPosition.z, spawnMax.localPosition.z);
        return newPos;
    }
}
