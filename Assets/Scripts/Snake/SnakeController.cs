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
            Vector3 moveDir = movePos - bodyParts[i].transform.localPosition;
            bodyParts[i].transform.localPosition += moveDir * moveSpeed * Time.deltaTime;
            bodyParts[i].transform.LookAt(movePos);
        }
    }

    private void FixedUpdate()
    {
        posHistory.Insert(0, transform.localPosition);
        posHistory = posHistory.GetRange(0, Mathf.Min(maxListSize, posHistory.Count));

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 25f, LayerMask.GetMask("Detectable")))
        {
            if (hit.transform.CompareTag("Target"))
            {
                SetReward(0.05f);
            }
            else if (hit.transform.CompareTag("Avoid"))
            {
                SetReward(-0.1f);
            }
            else if (hit.transform.CompareTag("Wall"))
            {
                SetReward(-0.001f);
            }
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
            body.transform.localPosition = bodyParts[bodyParts.Count - 1].transform.localPosition;
        }
        else
        {
            // First segment
            body.transform.localPosition = transform.localPosition;
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
            SetReward(+1f);
        }

        else if (other.gameObject.CompareTag("Avoid") || other.gameObject.CompareTag("Wall"))
        {
            SetReward(-5f);
            EndEpisode();
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
        sensor.AddObservation(transform.position);

        // Food Vec3 Pos
        sensor.AddObservation(food.transform.position);

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
