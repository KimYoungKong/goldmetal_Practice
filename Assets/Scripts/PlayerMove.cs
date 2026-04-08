using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{ 
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;
    CapsuleCollider2D capsuleCollider;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    AudioSource audioSource;

    void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
            audioSource.clip = audioJump;
            break;
            case "ATTACK":
            audioSource.clip = audioAttack;
            break;
            case "DAMAGED":
            audioSource.clip = audioDamaged;
            break;
            case "ITEM":
            audioSource.clip = audioItem;
            break;
            case "DIE":
            audioSource.clip = audioDie;
            break;
            case "FINISH":
            audioSource.clip = audioFinish;
            break;
        }
        audioSource.Play();
    }

    void Update() {
        // 1. 점프
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")) {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // 2. 멈출 때 속도 조절
        if (Input.GetButtonUp("Horizontal")) {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        // 3. 방향 전환
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // 4. 애니메이션 상태 업데이트
        if (Mathf.Abs(rigid.linearVelocity.x) < 0.3f)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    void FixedUpdate() {
        // 5. 이동 물리 처리
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // 6. 최대 속도 제한
        if (rigid.linearVelocity.x > maxSpeed)
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        else if (rigid.linearVelocity.x < maxSpeed * (-1))
            rigid.linearVelocity = new Vector2(maxSpeed * (-1), rigid.linearVelocity.y);

        // 7. 착지 체크 (Raycast)
        if (rigid.linearVelocity.y < 0) {
            Debug.DrawRay(rigid.position, Vector3.down, Color.red);
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null && rayHit.distance < 0.7f)
                anim.SetBool("isJumping", false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            // 핵심: 몬스터 밟기 (내려오는 중 + 내 위치가 몬스터보다 위)
            if (rigid.linearVelocity.y < 0 && transform.position.y > collision.transform.position.y) {
                OnAttack(collision.transform);
            }
            else {
                // 옆에서 부딪히면 피격
                OnDamaged(collision.transform.position);
            }
        }
    }
    
    // PlayerMove 클래스 내부 어딘가에 추가
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            
            // 1. 점수 계산
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze) gameManager.stagePoint += 50;
            else if (isSilver) gameManager.stagePoint += 100;
            else if (isGold) gameManager.stagePoint += 300;
        

            // 2. 아이템 습득 효과 (지금 바로 확인 가능!)
            collision.gameObject.SetActive(false);

            // 3. 사운드
            PlaySound("ITEM");
            
            Debug.Log("아이템 획득!"); 
        }
        else if(collision.gameObject.tag == "Finish")
        {
            gameManager.NextStage();
            PlaySound("FINISH");
        }
    }
    void OnAttack(Transform enemy) {

        //포인트
        gameManager.stagePoint += 100;
        // 점프 반발력
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        PlaySound("ATTACK");
        // 적 사망 처리 함수 호출
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    public void OnDamaged(Vector2 targetPos) {
        // 피 감소
        gameManager.HealthDown();
        // 피격 시 무적 레이어 변경 및 연출
        gameObject.layer = 11; 
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // 튕겨나가는 물리 효과
        int reactionDir = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(reactionDir, 1) * 7, ForceMode2D.Impulse);
        PlaySound("DAMAGED");
        anim.SetTrigger("doDamaged");
        Invoke("OffDamaged", 3f);
    }

    void OffDamaged() {
        gameObject.layer = 10; 
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }
    public void OnDie()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        capsuleCollider.enabled = false;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        PlaySound("DIE");
    }
    // VelocityZero 함수 근처에 추가하거나 기존 함수를 수정하세요
public void VelocityZero() {
    rigid.linearVelocity = Vector2.zero;
    // 죽을 때 꺼졌던 콜라이더를 다시 켭니다.
    capsuleCollider.enabled = true; 
    // 죽어서 뒤집혔던 스프라이트를 원래대로 돌립니다.
    spriteRenderer.flipY = false;
}
}