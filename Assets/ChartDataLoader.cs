using UnityEngine;
using XCharts.Runtime;
using System.Collections;
using System;

public class ChartDataLoader : MonoBehaviour
{
    // Reference to the LineChart component
    public LineChart lineChart;

    // Reference to the DataProvider component
    // public UnityClient unityclient;

    // Time interval between data updates in seconds
    public float updateInterval = 1.0f;

    void Start()
    {
        if (lineChart == null)
        {
            Debug.LogError("LineChart component is not assigned.");
            return;
        }

        // if (unityclient == null)
        // {
        //     Debug.LogError("DataProvider component is not assigned.");
        //     return;
        // }

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

    private IEnumerator UpdateChartData()
    {
        while (true)
        {
            // Update the chart with new data from DataProvider
            // float randomValue = dataProvider.randomValue; // Get the random value from DataProvider
            float Value = UnityClient.leftValue; // Get the random value from DataProvider
            DateTime currentTime = DateTime.Now;
            
            Debug.Log("ccccc" + Value);

            // Add data point to the chart
            lineChart.AddData(0, Value, currentTime.ToString("HH:mm:ss"));

            // Wait for the next update
            yield return new WaitForSeconds(updateInterval);
        }
    }
}