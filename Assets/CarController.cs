using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform[] resetPoints; // 存储所有的复位点
    public float resetDistanceThreshold = 10f; // 复位点的距离阈值
    
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    public float timeToStore = 3f; // 保存旋转状态的时间（秒）
    private Queue<Quaternion> rotationHistory;
    private float timeSinceLastSave;
    
    

    void Start()
    {
        // 记录初始位置和旋转
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        rotationHistory = new Queue<Quaternion>();
        timeSinceLastSave = 0f;
        
    }

    void Update()
    {
        // 每帧记录当前旋转状态
        rotationHistory.Enqueue(transform.rotation);

        // 如果超过保存时间，则移除最旧的记录
        timeSinceLastSave += Time.deltaTime;
        if (timeSinceLastSave > timeToStore)
        {
            rotationHistory.Dequeue();
            timeSinceLastSave -= timeToStore / rotationHistory.Count; // Adjust the time offset for each rotation
        }

        // 按下 R 键时复位到 3 秒前的朝向
        if (Input.GetKeyDown(KeyCode.F))
        {
            ResetRotationToPast();
        }
        
        // 按下 R 键时重置赛车
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCar();
        }
    }
    
    void ResetRotationToPast()
    {
        if (rotationHistory.Count > 0)
        {
            // 获取 3 秒前的旋转状态
            Quaternion pastRotation = rotationHistory.Peek();
            transform.rotation = pastRotation;
        }
    }

    void ResetCar()
    {
        if (resetPoints.Length > 0)
        {
            // 找到距离赛车最近的复位点
            Transform nearestPoint = FindNearestPoint();
            transform.position = nearestPoint.position;
            transform.rotation = nearestPoint.rotation;
        }
        else
        {
            // 如果没有指定复位点，重置到初始位置和旋转
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    Transform FindNearestPoint()
    {
        Transform nearestPoint = null;
        float minDistance = float.MaxValue;

        foreach (Transform point in resetPoints)
        {
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < minDistance && distance <= resetDistanceThreshold)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }

        return nearestPoint ?? resetPoints[0]; // 如果没有找到适合的复位点，则返回第一个复位点
    }
}