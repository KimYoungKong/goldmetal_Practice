
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //Jump
        if(Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")){
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping",true);
        }
        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x*0.5f,rigid.linearVelocity.y);
        }    
        //Direction Sprite
        /*if(Input.GetButtonDown("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
            */
        float h = Input.GetAxisRaw("Horizontal");

        // 만약 입력이 있다면 (0이 아니면), 그 방향을 기준으로 시선을 바꿉니다.
        // D키를 떼지 않고 A키를 눌러도, h값이 -1로 변하므로 즉시 시선이 왼쪽으로 바뀝니다.
        if(h != 0) 
        spriteRenderer.flipX = h == -1;
        //Animation
        if(Mathf.Abs(rigid.linearVelocity.x) < 0.3)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Move By key Control
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        //Max Speed
        if(rigid.linearVelocity.x > maxSpeed) // 오른쪽
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        else if(rigid.linearVelocity.x < maxSpeed*(-1)) // 왼쪽
            rigid.linearVelocity = new Vector2(maxSpeed*(-1), rigid.linearVelocity.y);

        //Landing Platform
        if(rigid.linearVelocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down,new Color(0,1,0));

            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position,Vector3.down,1,LayerMask.GetMask("Platform"));

            if(rayHit.collider != null)
            {
                if(rayHit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
            }
        }
        
    }
}
