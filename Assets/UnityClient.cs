// UnityClient.cs

using System;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using XCharts.Runtime;
using System.Collections;

public class UnityClient : MonoBehaviour
{
    // 接收来自Python的消息
    TcpClient client;
    NetworkStream stream;
    Thread thread;//另开一个线程
    
    string[] parts = new string[2]; // 创建一个包含5个元素的字符串数组
    
    static public float leftValue;
    static public float rightValue;

    
    // Reference to the LineChart component
    public LineChart lineChart;
    // Time interval between data updates in seconds
    public float updateInterval = 1.0f;


    void Start()
    {
        // 连接到Python服务器
        ConnectToServer();
        
        // Get the LineChart component
        if (lineChart == null)
        {
            Debug.LogError("LineChart component is not assigned.");
            return;
        }
        // Set X Axis type to Time
        var xAxis = lineChart.GetChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Time;
        // Clear existing data
        lineChart.ClearData();
        // Add series and initialize data
        lineChart.AddSerie<Line>("Real-Time Series");
        // Start updating data
        StartCoroutine(UpdateChartData());
    }

    void Update()
    {
        leftValue = float.Parse(parts[0]);
        rightValue = float.Parse(parts[1]);
        
        // Debug.Log(leftValue + "和" + rightValue);

    }
    
    // 连接到服务器
    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("localhost", 12345);
            stream = client.GetStream();
            Debug.Log("连接到Python服务器！");

            // 开启一个线程用于接收消息
            thread = new Thread(new ThreadStart(ReceiveData));
            thread.Start();
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
    }
    
    // 向Python服务器发送消息
    void ReceiveData()
    {
        byte[] data = new byte[1024];
        while (true)
        {
            int bytesRead = stream.Read(data, 0, data.Length);
            string message = Encoding.ASCII.GetString(data, 0, bytesRead);
            // Debug.Log("接收到Python的消息: " + message);
            
            // 去掉括号和空格，只留下数字和逗号
            message = message.Replace("(", "").Replace(")", "").Replace(" ", "");
            // Debug.Log("message:" + message);

            // 按逗号分割字符串
            parts = message.Split(',');
            // Debug.Log("x 值:" + parts[0]);
            // Debug.Log("y 值:" + parts[1]);
            // // 解析成浮点数
            // if (parts.Length == 2)
            // {
            //     if (float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
            //     {
            //         // x 和 y 分别是解析后的浮点数值
            //         Debug.Log("x 值: " + x);
            //         Debug.Log( y);
            //     }
            //     else
            //     {
            //         Debug.LogError("无法解析坐标值。");
            //     }
            // }
            // else
            // {
            //     Debug.LogError("坐标格式不正确。");
            // }
        }
    }
    
    //画出实时折线图
    private IEnumerator UpdateChartData()
    {
        while (true)
        {
            // Update the chart with new data from DataProvider
            // float randomValue = dataProvider.randomValue; // Get the random value from DataProvider
            // float Value = UnityClient.leftValue; // Get the random value from DataProvider
            DateTime currentTime = DateTime.Now;
            
            Debug.Log("画图的值" + leftValue);

            // Add data point to the chart
            lineChart.AddData(0, leftValue, currentTime.ToString("HH:mm:ss"));

            // Wait for the next update
            yield return new WaitForSeconds(updateInterval);
        }
    }

    // 关闭连接
    void OnDestroy()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (thread != null && thread.IsAlive)
            thread.Abort();
    }
}