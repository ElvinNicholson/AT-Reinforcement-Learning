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
        Vector3 newPos = transform.position;
        newPos.x = Random.Range(spawnMin.position.x, spawnMax.position.x);
        newPos.z = Random.Range(spawnMin.position.z, spawnMax.position.z);

        transform.position = newPos;
    }
}
