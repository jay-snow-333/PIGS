using UnityEngine;
using UnityEngine.UI;
 
[RequireComponent(typeof(RawImage))]
public class MiniMap : MonoBehaviour
{
    public Camera cameraRef; // 指定摄像头
    public RawImage rawImage;
 
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (cameraRef != null)
        {
            rawImage.texture = cameraRef.targetTexture; // 设置RawImage的纹理
        }
    }
}