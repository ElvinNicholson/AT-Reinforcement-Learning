using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed;

    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private int bodyGap;
    [SerializeField] private Transform bodyParent;
    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> posHistory = new List<Vector3>();
    private int maxListSize;

    private void Start()
    {
        AddBodySegment();
    }

    private void Update()
    {
        transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;

        float turnDir = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, turnDir * turnSpeed * Time.deltaTime);

        for (int i = 0; i < bodyParts.Count; i++)
        {
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
            other.GetComponent<Food>().RandomizePos();
        }

        else if (other.gameObject.CompareTag("Snake Body"))
        {
            Debug.Log("Game Over");
        }
    }
}
