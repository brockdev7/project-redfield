using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolatorWIP : MonoBehaviour
{
    [SerializeField] private float posTimeElapsed = 0f;
    [SerializeField] private float rotTimeElapsed = 0f;
    [SerializeField] private float posTimeToReachTarget = 0.05f;
    [SerializeField] private float rotTimeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
    private readonly List<QuaternionUpdate> futureQuaternionUpdates = new List<QuaternionUpdate>();

    private float squareMovementThreshold;

    private TransformUpdate toPosition;
    private TransformUpdate fromPosition;
    private TransformUpdate previousPosition;

    private QuaternionUpdate toRotation;
    private QuaternionUpdate fromRotation;
    private QuaternionUpdate previousRotation;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;

        toPosition = new TransformUpdate(NetworkManager.Singleton.ServerTick,  transform.position);
        fromPosition = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position);
        previousPosition = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position);

        toRotation = new QuaternionUpdate(NetworkManager.Singleton.ServerTick, transform.rotation);
        fromRotation = new QuaternionUpdate(NetworkManager.Singleton.InterpolationTick, transform.rotation);
        previousRotation = new QuaternionUpdate(NetworkManager.Singleton.InterpolationTick, transform.rotation);
    }


    private void Update()
    {
        //Handle Position Updates
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(NetworkManager.Singleton.ServerTick >= futureTransformUpdates[i].Tick)
            {
                previousPosition = toPosition;
                toPosition = futureTransformUpdates[i];
                fromPosition = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position);
             
                futureTransformUpdates.RemoveAt(i);
                i--;
                posTimeElapsed = 0f;

                if ((toPosition.Tick - fromPosition.Tick * Time.fixedDeltaTime) == 0)
                    return;

                posTimeToReachTarget = (toPosition.Tick - fromPosition.Tick) * Time.fixedDeltaTime;
            }
        }

        //Handle Rotation Updates
        for (int i = 0; i < futureQuaternionUpdates.Count; i++)
        {
            if (NetworkManager.Singleton.ServerTick >= futureQuaternionUpdates[i].Tick)
            {
                previousRotation = toRotation;
                toRotation = futureQuaternionUpdates[i];
                fromRotation = new QuaternionUpdate(NetworkManager.Singleton.InterpolationTick, transform.rotation);

                futureQuaternionUpdates.RemoveAt(i);
                i--;
                rotTimeElapsed = 0f;
                rotTimeToReachTarget = (toRotation.Tick - fromRotation.Tick) * Time.fixedDeltaTime;
            }
        }

        posTimeElapsed += Time.deltaTime;
        rotTimeElapsed += Time.deltaTime;

        InterpolatePosition(posTimeElapsed / posTimeToReachTarget);
        InterpolateRotation(rotTimeElapsed / rotTimeToReachTarget);
    }


    public void NewPositionUpdate(ushort tick, Vector3 position)
    {
        //Insert new position update if tick < any of our update ticks
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, position));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, position));
    }

    private void InterpolatePosition(float lerpAmount)
    {
        if ((toPosition.Position - previousPosition.Position).sqrMagnitude < squareMovementThreshold)
        {
            if (toPosition.Position != fromPosition.Position)
                transform.position = Vector3.Lerp(fromPosition.Position, toPosition.Position, lerpAmount);
            return;
        }

        transform.position = Vector3.LerpUnclamped(fromPosition.Position, toPosition.Position, lerpAmount);    
    }

    public void NewRotationUpdate(ushort tick, Quaternion rotation)
    {
        //Insert new rotation update if tick < any of our update ticks
        for (int i = 0; i < futureQuaternionUpdates.Count; i++)
        {
            if (tick < futureQuaternionUpdates[i].Tick)
            {
                futureQuaternionUpdates.Insert(i, new QuaternionUpdate(tick, rotation));
                return;
            }
        }

        futureQuaternionUpdates.Add(new QuaternionUpdate(tick, rotation));
    }

    private void InterpolateRotation(float lerpAmount)
    {
            transform.rotation = Quaternion.Lerp(fromRotation.Rotation, toRotation.Rotation, lerpAmount);
    }





}
