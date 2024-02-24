using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float speed;
    [SerializeField] private Transform targetMin;
    [SerializeField] private Transform targetMax;

    [SerializeField] private MeshRenderer ground;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material looseMaterial;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, 0.5f, 0f);

        Vector3 newPos = targetTransform.localPosition;
        newPos.x = Random.Range(targetMin.localPosition.x, targetMax.localPosition.x);
        newPos.z = Random.Range(targetMin.localPosition.z, targetMax.localPosition.z);
        targetTransform.localPosition = newPos;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            ground.material = winMaterial;

            SetReward(+1f);
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            ground.material = looseMaterial;

            SetReward(-1f);
            EndEpisode();
        }
    }
}
