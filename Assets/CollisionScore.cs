using UnityEngine;
using UnityEngine.UI;

public class CollisionScore : MonoBehaviour
{
    public ParticleSystem pickupParticle;  // 引用道具触发的粒子系统
    private int score = 0;
    public Text scoreText;  // 引用UI文本对象
    public GameObject gameOverPanel;  // 这里假设你已经在场景中创建了一个用于显示游戏结束信息的面板
   
    void Start()
    {
        // 初始化UI文本
        scoreText.text = "Score: " + score.ToString();
        gameOverPanel.SetActive(false);
    }
    
    void Update()
    {
        // 达到100分后停止游戏
        if (score >= 100)
        {
            Time.timeScale = 0;
            Debug.Log("Game Over! Your final score is: " + score);
            // 显示游戏结束面板
            gameOverPanel.SetActive(true);
        }
    }
    
    // 当赛车碰到道具时触发
    private void OnTriggerEnter(Collider other)
    {
        // 检查触发的物体是否是道具物体
        if (other.CompareTag("Score10"))
        {
            // 增加分数
            UpdateScore10();
            Debug.Log("Scored! Current Score: " + score);
            // 播放粒子效果
            PlayPickupParticle(other.transform.position);

            // 可以在此处播放音效或者其他效果

            // 销毁道具物体
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Score20"))
        {
            // 增加分数
            UpdateScore20();
            Debug.Log("Scored! Current Score: " + score);
            // 播放粒子效果
            PlayPickupParticle(other.transform.position);

            // 可以在此处播放音效或者其他效果

            // 销毁道具物体
            Destroy(other.gameObject);
        }
    }
    
    // 更新分数显示
    public void UpdateScore10()
    {
        score += 10;
        scoreText.text = "Score: " + score.ToString();
    }
    public void UpdateScore20()
    {
        score += 20;
        scoreText.text = "Score: " + score.ToString();
    }
    
    // 播放道具触发的粒子效果
    private void PlayPickupParticle(Vector3 position)
    {
        // 检查是否已经设置了粒子系统
        if (pickupParticle != null)
        {
            // 实例化粒子效果，并设置位置
            ParticleSystem particleInstance = Instantiate(pickupParticle, position, Quaternion.identity);
            
            // 在粒子播放完成后销毁
            Destroy(particleInstance.gameObject, particleInstance.main.duration);
        }
    }
    
}
