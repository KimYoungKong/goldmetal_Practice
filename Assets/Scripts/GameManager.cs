using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP 사용을 위해 필수
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;

    public Image[] UIhealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage; 
    public GameObject UIRestartBtn;

    void Update()
    {
        // 1. .text를 사용하여 점수 갱신
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        if(stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            // 2. .text를 사용하여 스테이지 표시
            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else 
        {
            Time.timeScale = 0;
            Debug.Log("게임 클리어!!!");
            
            // 3. 버튼 텍스트 컴포넌트도 TMP 타입으로 가져와야 함
            TextMeshProUGUI btnText = UIRestartBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "Clear!";
            UIRestartBtn.SetActive(true);
        } 

        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if(health > 1) {
            health--;
            // UnityEngine.Color라고 명시하거나 상단 using System.Drawing을 지우면 됩니다.
            UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }
        else
        {
            //All Health UI off
            UIhealth[0].color = new Color(1, 0, 0, 0.4f);

            player.OnDie();
            Debug.Log("죽었습니다.");
            UIRestartBtn.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player") {
            if(health > 1){
                PlayerReposition();
            }
            HealthDown();
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 1, -1);
        player.VelocityZero();
    }

    public void Restart()
    {
        // 재시작 시 멈췄던 시간을 다시 흐르게 해야 합니다.
        Time.timeScale = 1; 
        // 훨씬 안전한 코드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}