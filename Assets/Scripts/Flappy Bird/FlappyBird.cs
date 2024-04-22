using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyBird : MonoBehaviour
{
    public int score;

    [Header("References")]
    [SerializeField] private GameObject pipes;

    [Header("Pipes")]
    [SerializeField] private float pipeSpeed;
    [SerializeField] private float pipeSpawnInterval;
    [SerializeField] private Vector3 pipeSpawnOffset;
    [SerializeField] private float pipeSpawnMinHeight;
    [SerializeField] private float pipeSpawnMaxHeight;

    private float pipeSpawnCountdown;
    private GameObject pipeParent;
    private int pipeCount;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        SpawnPipes();

        pipeParent.transform.localPosition += Vector3.left * pipeSpeed * Time.deltaTime;

        if (pipeParent.transform.childCount >= 5)
        {
            Destroy(pipeParent.transform.GetChild(0).gameObject);
        }
    }



    private void SpawnPipes()
    {
        pipeSpawnCountdown -= Time.deltaTime;

        if (pipeSpawnCountdown <= 0)
        {
            pipeSpawnCountdown = pipeSpawnInterval;
            pipeCount++;

            GameObject newPipe = Instantiate(pipes);
            newPipe.transform.parent = pipeParent.transform;
            newPipe.transform.name = pipeCount.ToString();
            newPipe.transform.position = transform.position + pipeSpawnOffset;
            newPipe.transform.position += Vector3.up * Random.Range(pipeSpawnMinHeight, pipeSpawnMaxHeight);
        }
    }

    public void Reset()
    {
        score = 0;
        pipeCount = 0;
        pipeSpawnCountdown = 0;

        Destroy(pipeParent);
        pipeParent = new GameObject("PipesParent");
        pipeParent.transform.parent = transform;
    }
}
