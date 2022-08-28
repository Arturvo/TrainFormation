using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarriageController : MonoBehaviour
{
    private GameObject carriageToFollow;
    private  TrainController trainController;

    private class CarriageState
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private CarriageState lastStateEnqued;

    private Queue<CarriageState> statesToFollow;

    public void InitCarriage(GameObject carriageToFollow, TrainController trainController)
    {
        statesToFollow = new Queue<CarriageState>();
        this.carriageToFollow = carriageToFollow;
        this.trainController = trainController;
    }

    // Update is called once per frame
    void Update()
    {
        if (trainController.trainIsMoving)
        {
            if (lastStateEnqued == null || Vector3.Distance(lastStateEnqued.position, carriageToFollow.transform.position) > 0.01f)
            {
                lastStateEnqued = new CarriageState()
                {
                    position = carriageToFollow.transform.position,
                    rotation = carriageToFollow.transform.rotation,
                };
                statesToFollow.Enqueue(lastStateEnqued);
            }

            while (Vector3.Distance(transform.position, carriageToFollow.transform.position) > trainController.carriageDistance)
            {
                CarriageState firstState = statesToFollow.Dequeue();
                transform.position = firstState.position;
                transform.rotation = firstState.rotation;
            }
        }
    }
}
