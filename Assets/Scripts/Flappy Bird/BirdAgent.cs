using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class BirdAgent : Agent
{
    [SerializeField] private FlappyBird gameController;
    [SerializeField] private TextMeshProUGUI scoreboard;

    [Header("Bird")]
    [SerializeField] private float birdSpawnHeight;
    [SerializeField] private float gravity;
    [SerializeField] private float jump;

    [SerializeField] private Transform max;
    [SerializeField] private Transform min;

    [Header("ML Agent")]
    [SerializeField] private float passiveRewardRate;
    [SerializeField] private float deathTimer;

    private float verticalSpeed;
    private int score;

    private int action;

    private float deathCountdown;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(GetNextPipe() + gameController.transform.position, 0.5f);
    }

    private void Update()
    {
        MoveBird();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            action = 1;
        }

        DeathCountdown();

        KillBox();
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

    private void KillBox()
    {
        if (transform.position.y > 20 || transform.position.y < -10)
        {
            EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        if (deathCountdown == 0)
        {
            AddReward(passiveRewardRate);
        }
    }

    private Vector3 GetNextPipe()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default")))
        {
            if (hit.collider.gameObject.CompareTag("Pipe"))
            {
                return hit.collider.transform.GetChild(0).position - gameController.transform.position;
            }
        }

        return Vector3.zero;
    }

    private void MoveBird()
    {
        verticalSpeed -= gravity * Time.deltaTime;

        transform.position += Vector3.up * verticalSpeed * Time.deltaTime;

        float lerp1 = Mathf.InverseLerp(-10, 10, verticalSpeed);
        float lerp2 = Mathf.Lerp(-30, 30, lerp1);
        transform.GetChild(0).transform.rotation = Quaternion.Euler(Vector3.forward * lerp2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            other.transform.tag = "Untagged";

            score++;

            if (scoreboard != null)
            {
                if (int.Parse(scoreboard.text) < score)
                {
                    scoreboard.text = score.ToString();
                }
            }

            if (deathCountdown == 0)
            {
                SetReward(1f);
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
        gameController.Reset();

        deathCountdown = 0;

        verticalSpeed = 0;
        transform.localPosition = Vector3.up * birdSpawnHeight;

        score = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Current Vec3 Pos
        sensor.AddObservation(transform.localPosition);

        // Next Pipe Vec3 Pos
        sensor.AddObservation(GetNextPipe());

        // Height Ratio
        float heightRatio = (transform.localPosition.y - min.localPosition.y) / (max.localPosition.y - min.localPosition.y);
        sensor.AddObservation(heightRatio);

        // Vertical Speed
        sensor.AddObservation(verticalSpeed);

        // Distance to Next Pipe
        float distToTarget = Vector3.Distance(transform.localPosition, GetNextPipe());
        sensor.AddObservation(distToTarget);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        switch (actions.DiscreteActions[0])
        {
            case 0:
                break;

            case 1:
                action = 0;
                verticalSpeed = 0;
                verticalSpeed += jump;
                break;

            case 2:
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = action;
    }

    #endregion
}
