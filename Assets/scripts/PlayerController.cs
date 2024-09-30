using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;
using UnityEngine.XR;
using System;

public class PlayerController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI closestText;
    public Vector2 moveValue;
    public float speed;
    private float closestDistance;
    private float smallestAngle;
    private int numPickups = 10;
    private Vector3 prevPosition;
    Vector3 velocity;
    private int score;
    private LineRenderer lineRenderer;
    private enum State
    {
        Normal,
        Distance,
        Vision
    }

    State MyState = State.Normal;
    void OnMove(InputValue value)
    {
        moveValue = value.Get<Vector2>();
    }

    void Start()
    {
        score = 0;
        prevPosition = transform.position;
        winText.text = "";
        positionText.text = "";
        velocityText.text = "";
        closestText.text = "";
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        setCountText();
    }

    void updateText(Vector3 newPosition)
    {
        if(MyState == State.Distance)
        {
            positionText.text = "Position: " + newPosition.ToString("0.00");
            velocityText.text = "Velocity: " + getScalar(velocity).ToString("0.00");
        }
        else
        {
            positionText.text = "";
            velocityText.text = "";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            changeState();
        }
    }
    void FixedUpdate()
    {
        Vector3 newPosition = transform.position;
        Vector3 movement = new Vector3(moveValue.x, 0.0f, moveValue.y);

        GetComponent<Rigidbody>().AddForce(movement * speed * Time.fixedDeltaTime);

        velocity = (newPosition - prevPosition) / Time.deltaTime;
        updateText(newPosition);
        prevPosition = newPosition;
        updateClosest(GameObject.FindGameObjectsWithTag("PickUp"));
        updateVision(GameObject.FindGameObjectsWithTag("PickUp"));
    }

    void changeState()
    {
        if(MyState == State.Normal)
        {
            MyState = State.Distance;
            lineRenderer.enabled = false;
        }
        else if(MyState == State.Distance) 
        {
            MyState= State.Vision;
        }
        else if (MyState == State.Vision)
        {
            MyState = State.Normal;
        }
    }

    private void updateClosest(GameObject[] pickups)
    {
        int closestID = getClosest(pickups);

        if (MyState != State.Distance)
        {
            closestText.text = "";

            if(closestID == -1)
            {
                return;
            }
            if (pickups[closestID].GetComponent<Renderer>().material.color != Color.green)
                pickups[closestID].GetComponent<Renderer>().material.color = Color.white;
            return;
        }
        if (closestID == -1)
        {
            closestDistance = 0;
            return;
        }
        for (int i = 0; i < pickups.Length; i++) 
        {
            if(i == closestID)
            {
                pickups[i].GetComponent<Renderer>().material.color = Color.blue;
            }
            else
            {
                pickups[i].GetComponent<Renderer>().material.color = Color.white;
            }
        }

        closestText.text = "Closest Pickup: " + closestDistance.ToString("0.00");
        lineRenderer.SetPosition(0,transform.position);
        lineRenderer.SetPosition(1, pickups[closestID].transform.position);
        lineRenderer.enabled = true;
    }

    private int getClosest(GameObject[] pickups)
    {
        int closestID = -1;
        float distance;
        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i].activeSelf)
            {
                if (closestID == -1)
                {
                    closestID = i;
                    closestDistance = Mathf.Abs(getScalar(prevPosition - pickups[i].transform.position));
                }
                else
                {
                    distance = Mathf.Abs(getScalar(prevPosition - pickups[i].transform.position));
                    if (distance < closestDistance)
                    {
                        closestID = i;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestID;
    }

    private void updateVision(GameObject[] pickups) 
    {
        int nextID = getPathClosest(pickups);

        if(nextID == -1)
        {
            return;
        }
        if(MyState != State.Vision) 
        {
            if (pickups[nextID].GetComponent<Renderer>().material.color != Color.blue)
                pickups[nextID].GetComponent<Renderer>().material.color = Color.white;
            return;
        }
        for (int i = 0; i < pickups.Length; i++)
        {
            if (i == nextID)
            {
                pickups[i].GetComponent<Renderer>().material.color = Color.green;
                pickups[i].transform.LookAt(transform.position);
            }
            else
            {
                pickups[i].GetComponent<Renderer>().material.color = Color.white;
            }
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + velocity);
        lineRenderer.enabled = true;
    }

    private int getPathClosest(GameObject[] pickups)
    {
        int nextID = -1;
        float angle;
        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i].activeSelf)
            {
                if (nextID == -1)
                {
                    nextID = i;
                    smallestAngle = Vector3.Angle(velocity, (pickups[i].transform.position - transform.position));
                }
                else
                {
                    angle = Vector3.Angle(velocity, (pickups[i].transform.position - transform.position));
                    if (angle < smallestAngle)
                    {
                        nextID = i;
                        smallestAngle = angle;
                    }
                }
            }
        }

        return nextID;
    }
    private void setCountText()
    {
        scoreText.text = "Score: " + score.ToString();

        if(score >= numPickups)
        {
            lineRenderer.enabled = false;
            winText.text = "You Win!";
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PickUp")
        {
            other.gameObject.SetActive(false);
            score++;
            setCountText() ;
        }
    }

    private float getScalar(Vector3 vector)
    {
        float value = 0;
        value = Mathf.Sqrt((vector.x * vector.x) + (vector.y * vector.y) + (vector.z * vector.z));
        return value;
    }

}