using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#122-bikeinput")]
    public class BikeInput : MonoBehaviour
    {
        // 接收来自Python的消息
        TcpClient client;
        NetworkStream stream;
        Thread thread;//另开一个线程
    
        string[] parts = new string[2]; // 创建一个包含5个元素的字符串数组
        public float XValue;
        public float YValue;
        
        public Slider slider; // 拖拽UI滑块到此处
        public Slider sliderxAxis; // 拖拽UI滑块到此处
        public Text xAxisText; // 拖拽UI Text到此处
        
        
        
#if ENABLE_INPUT_SYSTEM
        private InputDevice inputDevice;
        public KeyboardSettings keyboardSettings;
        public JoystickSettings joystickSettings;
#else
        [Tooltip("User input sensitivity.")]
        [Range(0.01f, 0.1f)]
        public float sensitivity = 0.01f;
        [Tooltip("Determines the speed at which the return to zero occurs.")]
        [Range(0, 0.1f)]
        public float toZero = 0.5f;//用来模拟阻尼效果，使得输入在停止操作后逐渐减弱，而不是突然变为零。越大回归到0的速度越快
#endif
        /// <summary>
        /// Output of this script.
        /// </summary>
        [Tooltip("Output of this script.")]
        public float xAxis;
        /// <summary>
        /// Output of this script.
        /// </summary>
        [Tooltip("Output of this script.")]
        public float yAxis;

        private void Start()
        {
            // 连接到Python服务器
            ConnectToServer();
            
            //滑块控制值
            if (slider != null)
            {
                // 设置初始值
                slider.value = toZero;
                // 添加监听器，当滑块值改变时调用
                slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
            
#if ENABLE_INPUT_SYSTEM
            if (inputDevice == null)
                inputDevice = InputSystem.devices[0];
#endif
        }
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputDevice is Keyboard)
                keyboard((Keyboard)inputDevice);
            if (inputDevice is Mouse)
                mouse((Mouse)inputDevice);
            if (inputDevice is Joystick)
                joystick((Joystick)inputDevice);
            if (inputDevice is Gamepad)
                gamepad((Gamepad)inputDevice);
#else
            oldInput();
            xAxis *= (1 - toZero);
#endif
            xAxis = Mathf.Clamp(xAxis, -1, 1);
            yAxis = Mathf.Clamp(yAxis, -1, 1);
            
            
            // 更新滑块和文本
            if (sliderxAxis != null)
            {
                sliderxAxis.value = xAxis;
            }
            
            if (xAxisText != null)
            {
                xAxisText.text = $"xAxis: {xAxis:F2}"; // 格式化为小数点后两位
            }
        }
        private void FixedUpdate()
        {
        }
#if ENABLE_INPUT_SYSTEM
        private void keyboard(Keyboard kb)
        {
            xAxis *= (1 - keyboardSettings.toZero);

            if ((kb.aKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad4Key.isPressed && keyboardSettings.numpad) ||
                (kb.leftArrowKey.isPressed && keyboardSettings.arrows))
                xAxis -= keyboardSettings.sensitivityX;

            if ((kb.dKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad6Key.isPressed && keyboardSettings.numpad) ||
                (kb.rightArrowKey.isPressed && keyboardSettings.arrows))
                xAxis += keyboardSettings.sensitivityX;

            if ((kb.wKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad8Key.isPressed && keyboardSettings.numpad) ||
                (kb.upArrowKey.isPressed && keyboardSettings.arrows))
            {
                yAxis = Mathf.Max(yAxis, 0);
                yAxis += keyboardSettings.sensitivityY;
            }

            if ((kb.sKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad2Key.isPressed && keyboardSettings.numpad) ||
                (kb.downArrowKey.isPressed && keyboardSettings.arrows))
                if (yAxis > 0)
                    yAxis -= keyboardSettings.sensitivityY;

            if (kb.spaceKey.isPressed)
            {
                yAxis = Mathf.Min(yAxis, 0);
                yAxis -= keyboardSettings.sensitivityY;
            }
        }
        private void mouse(Mouse mouse)
        {
            if (mouse.leftButton.isPressed)
                xAxis -= keyboardSettings.sensitivityX;
            if (mouse.rightButton.isPressed)
                xAxis += keyboardSettings.sensitivityX;

            Vector2 scroll = mouse.scroll.ReadValue();
            if (scroll.y > 0)
                yAxis += keyboardSettings.sensitivityY * 10;
            if (scroll.y < 0)
                yAxis -= keyboardSettings.sensitivityY * 10;

            xAxis *= (1 - keyboardSettings.toZero);
        }
        private void joystick(Joystick joystick)
        {
            InputSystem.settings.defaultDeadzoneMin = joystickSettings.deadZoneMin;
            Vector2 stick = joystick.stick.ReadValue();
            xAxis = stick.x * joystickSettings.sensitivityX;
            yAxis = stick.y * joystickSettings.sensitivityY;
        }
        private void gamepad(Gamepad gamepad)
        {
            xAxis = gamepad.leftStick.x.ReadValue();
            yAxis = gamepad.rightStick.y.ReadValue();
        }
        public void setInputDevice(InputDevice inputDevice)
        {
            this.inputDevice = inputDevice;
        }
#else
        private void oldInput()
        {
            XValue = float.Parse(parts[0]);
            YValue = float.Parse(parts[1]);
            
            Debug.Log("倾斜值" + XValue);
            Debug.Log("前倾值" + YValue);

            if (XValue >= 0.8) XValue = 0.6f;
            else if (XValue <= -0.8) XValue = -0.6f;
            
            else if (YValue >= 0.5 & YValue < 0.8) XValue = 0.3f;
            else if (YValue > -0.8 & YValue <= -0.5) XValue = -0.3f;
            
            else if (YValue >= 0.3 & YValue < 0.5) XValue = 0.1f;
            else if (YValue > -0.5 & YValue <= -0.3) XValue = -0.1f;
            
            else if (YValue > -0.3 & YValue < 0.3) YValue = 0f / 100;
            

            if (YValue > 5) YValue = 0.5f;
            else if (YValue < -5) YValue = -0.5f;
            else YValue = YValue;

            // Debug.Log(XValue + "和" + YValue);
            
            xAxis = XValue;
            // xAxis += XValue * sensitivity;
            // yAxis += YValue * sensitivity;
            yAxis = YValue;
            
            Debug.Log("水平值" + xAxis);
            Debug.Log("垂直值" + yAxis);
            
            // xAxis += Input.GetAxis("Horizontal") * sensitivity;
            // yAxis += Input.GetAxis("Vertical") * sensitivity;
            // Debug.Log("水平值" + Input.GetAxis("Horizontal"));
            // Debug.Log("垂直值" + Input.GetAxis("Vertical"));
        }
#endif
        [System.Serializable]
        public class KeyboardSettings
        {
            public bool AWSD = true;
            public bool arrows = true;
            public bool numpad = true;
            [Space]
            [Range(0.01f, 1.0f)]
            public float sensitivityX = 0.01f;
            [Range(0.01f, 1.0f)]
            public float sensitivityY = 0.01f;
            [Range(0, 0.5f)]
            public float toZero = 0.01f;//用来模拟阻尼效果，使得输入在停止操作后逐渐减弱，而不是突然变为零。越大回归到0的速度越快
        }
        [System.Serializable]
        public class JoystickSettings
        {
            [Range(0, 1)]
            public float sensitivityX = 1;
            [Range(0, 1)]
            public float sensitivityY = 1;
            [Range(0, 0.9f)]
            public float deadZoneMin = 0;
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
                Debug.Log("x 值:" + parts[0]);
                Debug.Log("y 值:" + parts[1]);
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
        
        // 当滑块值改变时调用的方法
        void OnSliderValueChanged(float value)
        {
            toZero = value;
            Debug.Log("Controlled Value: " + toZero);
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
}