using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyBird : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bird;
    [SerializeField] private GameObject pipes;

    [Header("Bird")]
    [SerializeField] private float birdSpawnHeight;
    [SerializeField] private float gravity;
    [SerializeField] private float jump;

    [Header("Pipes")]
    [SerializeField] private float pipeSpeed;
    [SerializeField] private float pipeSpawnInterval;
    [SerializeField] private Vector3 pipeSpawnOffset;
    [SerializeField] private float pipeSpawnMinHeight;
    [SerializeField] private float pipeSpawnMaxHeight;

    private float verticalSpeed;

    private float pipeSpawnCountdown;
    private GameObject pipeParent;
    private int pipeCount;

    private void Start()
    {
        pipeCount = 0;
        verticalSpeed = 0;
        pipeSpawnCountdown = 0;

        Destroy(pipeParent);
        pipeParent = new GameObject("PipesParent");
        pipeParent.transform.parent = transform;

        bird.transform.position = Vector3.up * birdSpawnHeight;
    }

    private void Update()
    {
        MoveBird();

        SpawnPipes();

        pipeParent.transform.position += Vector3.left * pipeSpeed * Time.deltaTime;
    }

    private void MoveBird()
    {
        verticalSpeed -= gravity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            verticalSpeed = 0;
            verticalSpeed += jump;
        }

        bird.transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
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
            newPipe.transform.position = pipeSpawnOffset;
            newPipe.transform.position += Vector3.up * Random.Range(pipeSpawnMinHeight, pipeSpawnMaxHeight);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Start();
    }
}
