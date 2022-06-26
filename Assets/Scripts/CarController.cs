using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CarController : MonoBehaviour
{
    public float speed;
    public float rotation;
    public float castDistance;
    public LayerMask checkpointlayerMask;
    public LayerMask tracklayerMask;
    public int position;
    public bool collided;

    private float[] input = new float[10];//input to the neural network
    public NeuralNetwork network;
    public GameObject[] checkPoints;

    void FixedUpdate()
    {
        if (!collided)//if the car is still on the track, use run inputs trough network
        {
             float castAngle = transform.rotation.z - 90;

            for (int i = 0; i < 5; i++)//draws five sensor rays from the car.
            {
                Vector2 endPoint = new Vector2((castDistance * (float)Math.Cos(castAngle)) + transform.position.x, (castDistance * (float)Math.Sin(castAngle)) + transform.position.y);
                RaycastHit2D hitCheckpoint = Physics2D.Linecast(transform.position, endPoint, checkpointlayerMask);
                RaycastHit2D hitTrackEdge = Physics2D.Linecast(transform.position, endPoint, tracklayerMask);

                if (hitCheckpoint.collider != null) //Asign input as distance to colliding checkpoint (from 0 to 1)
                {
                    input[i] = (10 - hitCheckpoint.distance) / 10;
                }
                else
                {
                    input[i] = 0;
                }

                if (hitTrackEdge.collider != null) //Asign input as distance to colliding track edge (from 0 to 1)
                {
                    input[i + 5] = (10 - hitTrackEdge.distance) / 10;
                }
                else
                {
                    input[i + 5] = 0;
                }

                castAngle += 45;
            }

            float[] output = network.FeedForward(input); //Gets outputs from network and uses them to controll the car
            Vector2 newPosition = this.transform.up * output[1] * speed;
            transform.Translate(newPosition, Space.World);
            transform.Rotate(0, 0, output[0] * rotation);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 9) //If the car passes a checkpoint
        {
            if (collider.gameObject == checkPoints[position]) //If collideed checkpoint is the next checkpont, increase position 
            {
                position++;
            }

        }
        else if (collider.gameObject.layer == 8) //if car has collided with the edge of the track
        {
            collided = true;
        }
    }

    public void UpdateFitness()
    {
        network.fitness = position;//updates fitness of network for sorting
    }
}

