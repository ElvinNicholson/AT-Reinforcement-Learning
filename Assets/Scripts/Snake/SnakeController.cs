using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SnakeController : Agent
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed;

    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private int bodyGap;
    [SerializeField] private Transform bodyParent;
    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> posHistory = new List<Vector3>();
    private int maxListSize;

    [Header("ML-AGENTS")]
    [SerializeField] private Food food;
    private int turnDir;

    private Vector3 lastPos;

    [SerializeField] private float deathTimer;
    private float deathCountdown;

    [SerializeField] private float passiveRewardRate;
    [SerializeField] private float pickedUpFoodTimer;
    private float pickedUpFoodCountdown;

    private void Update()
    {
        transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, turnDir * turnSpeed * Time.deltaTime);

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (posHistory.Count == 0)
            {
                return;
            }

            Vector3 movePos = posHistory[Mathf.Min(i * bodyGap, posHistory.Count - 1)];
            Vector3 moveDir = movePos - bodyParts[i].transform.position;
            bodyParts[i].transform.localPosition += moveDir * moveSpeed * Time.deltaTime;
            bodyParts[i].transform.LookAt(movePos);
        }

        DeathCountdown();

        pickedUpFoodCountdown -= Time.deltaTime;
    }

    private void DeathCountdown()
    {
        if (deathCountdown > 0)
        {
            deathCountdown -= Time.deltaTime;

            if (deathCountdown <= 0)
            {
                EndEpisode();
            }
        }
    }

    private void FixedUpdate()
    {
        posHistory.Insert(0, transform.position);
        posHistory = posHistory.GetRange(0, Mathf.Min(maxListSize, posHistory.Count));

        if (pickedUpFoodCountdown > 0)
        {
            AddReward(passiveRewardRate);
        }
        else
        {
            AddReward(-passiveRewardRate);
        }
    }

    private void ResetGame()
    {
        foreach (GameObject body in bodyParts)
        {
            Destroy(body);
        }

        bodyParts.Clear();
        posHistory.Clear();
        turnDir = 0;
        deathCountdown = 0;

        // Randomize spawn positions
        float randomX = Random.Range(-5, 5);
        float randomZ = Random.Range(-5, 5);
        transform.localPosition = new Vector3(randomX, transform.localPosition.y, randomZ);
        food.RandomizePos();

        AddBodySegment();
    }

    private void AddBodySegment()
    {
        GameObject body = Instantiate(bodyPrefab);

        if (bodyParts.Count > 0)
        {
            body.transform.position = bodyParts[bodyParts.Count - 1].transform.position;
        }
        else
        {
            // First segment
            body.transform.position = transform.position;
            body.transform.tag = "Untagged";
        }

        body.transform.parent = bodyParent;

        bodyParts.Add(body);
        maxListSize = bodyParts.Count * bodyGap;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddBodySegment();
            food.RandomizePos();
            
            if (deathCountdown == 0)
            {
                SetReward(1f);
                pickedUpFoodCountdown = pickedUpFoodTimer;
            }
        }

        else if (other.gameObject.CompareTag("Avoid") || other.gameObject.CompareTag("Wall"))
        {
            SetReward(-5f);
            
            if (deathCountdown == 0)
            {
                deathCountdown = deathTimer;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Avoid") || other.gameObject.CompareTag("Wall"))
        {
            SetReward(-1f);
        }
    }

    #region ML-AGENTS

    public override void OnEpisodeBegin()
    {
        ResetGame();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Current Vec3 Pos
        sensor.AddObservation(transform.localPosition);

        // Food Vec3 Pos
        sensor.AddObservation(food.transform.localPosition);

        // X Velocity
        float vel_X = (transform.position.x - lastPos.x) / Time.deltaTime;
        sensor.AddObservation(vel_X);

        // Z Velocity
        float vel_Z = (transform.position.z - lastPos.z) / Time.deltaTime;
        sensor.AddObservation(vel_Z);

        lastPos = transform.position;

        // Distance to Food
        float distToFood = (Vector3.Distance(transform.position, food.transform.position));
        sensor.AddObservation(distToFood);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        switch (actions.DiscreteActions[0])
        {
            case 0:
                turnDir = 0;
                break;

            case 1:
                turnDir = 1;
                break;

            case 2:
                turnDir = -1;
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float input = Input.GetAxis("Horizontal");
        int action = 0;

        if (input < 0)
        {
            action = 2;
        }
        else if (input > 0)
        {
            action = 1;
        }

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = action;
    }

#endregion
}
