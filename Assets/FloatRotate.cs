using UnityEngine;

public class FloatRotate : MonoBehaviour
{
    private float floatSpeed = 4.0f;  // 浮动速度
    private float floatHeight = 0.5f;  // 浮动高度

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 计算新的Y轴位置，根据正弦函数实现上下浮动
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // 更新物体的位置
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
