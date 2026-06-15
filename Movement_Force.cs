using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Movement_Force : MonoBehaviour
{
    float moveSpeed = 9f;
    [SerializeField]float acceleration = 13f;
    [SerializeField]float deceleration = 16f;
    [SerializeField]float velPower = 0.96f;
    [SerializeField]float jumpForce = 14f;
    [SerializeField]float jumpCutMultiplier = 0.4f;
    [SerializeField]float fallGlavityMultiplier;
    [SerializeField]float peakJumpThreshold = 0.2f;
    [SerializeField]float peakJumpBonusTimeMultiplier = 0.5f;
    [SerializeField]float dashingForce = 14f;
    [SerializeField]float dashingTime = 0.5f;
    [SerializeField]float groundDashCooldown = 1f;
    [SerializeField]float maxFallSpeed;
    [SerializeField]float wallSlideSpeed;
    [SerializeField]float wallJumpingTime = 0.5f;
    float horizontalMovement =0f;
    Vector2 directionForce;
    Vector2 wallJumpDistance = new Vector2(10f,20f);
    public Rigidbody2D rb;
    public TrailRenderer trailRenderer;
    private bool isFacingRight = true;
    float frictionAmount = 0.2f;
    bool isGrounded =false;
    bool isJumping =false;
    bool isJCllmbingRight = false;
    bool jumpCutYet =false;
    bool requestJump = false;
    bool requestDash = false;
    bool requestJumpCut = false;
    bool isClimbing = false;
    bool isDashing = false;
    bool canDash = true;
    bool shortJumpBuffer = false;
    bool isWallJumping = false;
    float lastGroundedTime = 0f;
    private float coyoteTime = 0.2f;
    private float lastJumpTime =0f;
    private float jumpBuffer = 0.2f;
    float gravityScale;
    public Animator animator;

    
    // Start is called before the first frame update
    void Start()
    {
        rb.GetComponent<Rigidbody2D>();
        trailRenderer.GetComponent<TrailRenderer>();
        animator.GetComponent<Animator>();
        gravityScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        processInput();
        animator.SetFloat("velocity",Mathf.Abs(Input.GetAxisRaw("Horizontal")));
        //Debug.Log(animator.GetBool("isJumping"));
    }

    void FixedUpdate(){
        //ใช้ Fixed update เพื่ออัพเดทโดยไม่ใช่ framerate
        dash();
        if(!isDashing){
            move();
            friction();
            flipSprite();
            GroundedTime();
            jump();
            jumpCut();      
            fallGlavity();
        }
        if(isClimbing && !isDashing){
            WallSlide();
        }
        
        
    }
    void processInput(){
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        directionForce = new Vector2(horizontalMovement, Input.GetAxisRaw("Vertical")).normalized;

        /*if(lastGroundedTime < 0f && lastJumpTime >0f && !(Input.GetKey(KeyCode.Space)) && !isClimbing &&!isWallJumping){
            shortJumpBuffer = true;

        }else if(Input.GetKey(KeyCode.Space) && !isClimbing&&!isWallJumping){
            shortJumpBuffer = false;
            
        }*/

        //คำสั่งใช้กระโดด
        if(lastJumpTime > 0f && lastGroundedTime > 0f && !isJumping&&!isWallJumping /*&& !isClimbing*/){
            requestJump = true;
        }/*else if(lastJumpTime > 0f && lastGroundedTime > 0f && !isJumping && isClimbing){
            lastJumpTime = 0f;
            requestJump = true;
        }*/

        //คำสั่ง wall Jump
        if(lastJumpTime > 0f && !isWallJumping && isClimbing&&!isWallJumping){
            requestJump = true;
        }
       
       //คำสั่งสำหรับ cayote time และ jumpbuffer
        if(Input.GetButtonDown("Jump")){
            lastJumpTime = jumpBuffer;
        }else{
            lastJumpTime -= Time.deltaTime;
        }
        if(Input.GetButtonUp("Jump") && rb.velocity.y > 0f){ 
            requestJumpCut = true;  
            lastGroundedTime = 0f;
        }
        
        //คำสั่ง dash
        if(Input.GetButtonDown("Dash") && canDash && !isDashing&&!isWallJumping){
            isDashing = true;
            canDash = false;
            if(directionForce == Vector2.zero){
                directionForce = new Vector2(transform.localScale.x,0);
            }
            requestDash = true;
 
        }
        
    }

    //คำสั่งหันหน้า
    void flipSprite(){
        if((isFacingRight && horizontalMovement < 0f || !isFacingRight && horizontalMovement >0f) && !isClimbing){
            isFacingRight = !isFacingRight;
            //เช็คว่าหันด้านไหนอยู่
            Vector3 ls = transform.localScale;
            //สร้างตัวแปลสำหรับกำหนดขนาดสเกล
            ls.x *=-1f;
            //flip ด้านแกน x
            transform.localScale = ls;
            //นำไปใช้จริง
        }
    }

    //คำสั่งสำหรับใช้ Force เดิน
    void move(){
        float targetSpeed = horizontalMovement * moveSpeed;
        // Input ทิศทางของการเคลื่อนที่และตั่งค่า speed สูงสุด
        float speedDiff = targetSpeed - rb.velocity.x;
        //หาความแตกต้างระหว่าง speed สูงสุด และ สปีดล่าสุด
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        //คำนวณว่าต้องใช้ค่าความเร่งหรือความเฉื่อยในการกำหนด movement
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelerationRate,velPower)* Mathf.Sign(speedDiff);
        //กำหนดว่า movement จะไปในทิศทางไหนและคำนวณการเร่งและชะลอของ movement
        rb.AddForce(movement * Vector2.right);
        //ทำให้เกิด movement ในทิศทางดังกล่าว
    }
    void friction(){
        if(lastGroundedTime ==0f && Mathf.Abs(horizontalMovement) < 0.01f){
            //ตรวจสอบว่าอยู่บนพื้นและพยายามจะหยุดหรือไม่
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x),Mathf.Abs(frictionAmount));
            //หาความแรงของ Friction ที่จะใช้ ระหว่างใช้ความเร็วจริงกับใช้ friction ที่กำหนดไว้
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount,ForceMode2D.Impulse);
        }
    }
    private void OnCollisionEnter2D(Collision2D other){    
            foreach (ContactPoint2D contact in other.contacts)
    {
                //Debug.Log(contact.normal);
                
                //คำสั่งเช็คพื้น
                if (contact.normal.y > 0.5f && (other.gameObject.tag == "Ground"||other.gameObject.tag == "Wall"))
                    {

                        lastGroundedTime =0f;
                        isGrounded =true;
                        isJumping = false;
                        jumpCutYet = false;
                        canDash = true;
                        animator.SetBool("isJumping",false);
                        //Debug.Log("Ground");
                    }
                
                //คำสั่งเช็คกำแพง
                if (Mathf.Abs(contact.normal.x) > 0.5f && (other.gameObject.tag == "Wall"))
                    {
                        rb.velocity = new Vector2(0f,0f);
                        animator.SetBool("isClimbing",true);
                        isClimbing = true;
                        isJumping = false;
                        if(contact.normal.x> 0.5f){
                            isJCllmbingRight = true;
                        }else if(contact.normal.x < -0.5f){
                            isJCllmbingRight = false;
                        }
                    }
    }

    }
    
    //คำสั่งสไลด์กำแพง
    private void WallSlide(){
        rb.velocity = new Vector2(rb.velocity.x,Mathf.Clamp(rb.velocity.y,-wallSlideSpeed,float.MaxValue));
    }

    //คำสั่งเช็คออกจากพื้น
    private void OnCollisionExit2D(Collision2D other){
        if (other.gameObject.tag == "Ground"){
        isGrounded =false;
        }
        if (other.gameObject.tag == "Wall")
        {
            isClimbing = false;
            animator.SetBool("isClimbing",false);
        }
    }
    
    //คำสั่งนับเวลาบนพื้น
    void GroundedTime(){
        if(!isGrounded){
            lastGroundedTime -= Time.deltaTime;
        }
        else{
            lastGroundedTime = coyoteTime;
        }

    }
    
    //คำสั่งกระโดด
    void jump(){
        //กระโดดธรรมดา
        if(requestJump && !shortJumpBuffer && !isJumping &&!isClimbing){
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up* jumpForce,ForceMode2D.Impulse);
        isJumping = true;
        requestJump = false;
        animator.SetBool("isJumping",true);
        animator.SetTrigger("Jumping");
        }/*else if(requestJump && shortJumpBuffer && !isJumping && !isClimbing){
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up* jumpForce*0.5f,ForceMode2D.Impulse);
            isJumping = true;
            requestJump = false;
            animator.SetBool("isJumping",true);
            animator.SetTrigger("Jumping");

            //คำสั่ง walljump
        }*/else if(requestJump && isClimbing && !isJumping&& !isWallJumping){
            isWallJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            if(isJCllmbingRight){
                animator.SetBool("isJumping",true);
                animator.SetTrigger("Jumping");
                Vector2 wallJumpDirection = new Vector2(wallJumpDistance.x,wallJumpDistance.y);
                rb.AddForce(wallJumpDirection,ForceMode2D.Impulse);
                
            }else{
                animator.SetBool("isJumping",true);
                animator.SetTrigger("Jumping");
                Vector2 wallJumpDirection = new Vector2(-wallJumpDistance.x,wallJumpDistance.y);
                rb.AddForce(wallJumpDirection,ForceMode2D.Impulse);
                
            }
            isWallJumping = false;
            //StartCoroutine(stopWallJumping());
        }
    }

    //คำสั่ง jumpcut
    public void jumpCut(){
        if(!jumpCutYet && rb.velocity.y > 0 && isJumping && requestJumpCut){
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier),ForceMode2D.Impulse);
            jumpCutYet = true;
            requestJumpCut = false;
        }
    }
    
    //คำสั่งดึงแรงโน้มถ่วง
    void fallGlavity(){
        if(Mathf.Abs(rb.velocity.y) < peakJumpThreshold && isJumping && !isClimbing && !isDashing){
            rb.gravityScale = gravityScale * peakJumpBonusTimeMultiplier;
        }
        else if(rb.velocity.y < 0 && !isClimbing && !isDashing){
            rb.gravityScale = gravityScale * fallGlavityMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else if(rb.velocity.y >= 0 && !isClimbing && !isDashing){
            rb.gravityScale = gravityScale;
        }
        
    }
    //คำสั่งแดช
    void dash(){
        if(isDashing && requestDash){
            //Debug.Log("dash");
            requestDash = false;
            animator.SetBool("isDashing",true);
            trailRenderer.emitting = true;
            rb.velocity = new Vector2(0f,0f);
            rb.gravityScale = 0f;
            if(isClimbing){
                rb.velocity = new Vector2(0f,directionForce.y) * dashingForce;
            }else{
                rb.velocity = directionForce * dashingForce;
            }
            StartCoroutine(stopDashing());
        }
    }
    //นับเวลาหยุดแดช
    private IEnumerator stopDashing(){
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = gravityScale;
        rb.AddForce(-rb.velocity,ForceMode2D.Impulse);
        isDashing = false;
        animator.SetBool("isDashing",false);
        trailRenderer.emitting = false;
        StartCoroutine(stopGroundDash());
    }
    //นับเวลาหยุดแดชบนดิน
    private IEnumerator stopGroundDash(){
        yield return new WaitForSeconds(groundDashCooldown);
        if(isGrounded){ 
            canDash = true;
        } 
    }

    //นับเวลา walljump
    private IEnumerator stopWallJumping(){
        yield return new WaitForSeconds(wallJumpingTime);
        
    }
    
    //นับเวลาความเร็วจากไอเทม
    private IEnumerator stopSpeed(){
        yield return new WaitForSeconds(1f);
        moveSpeed = 9f;
    }
    
    //คำสั่งเร่งความเร็ว
    void OnTriggerEnter2D(Collider2D other){
    if (other.gameObject.tag == "FruitMagic")
        {
            moveSpeed = 18f;
            StartCoroutine(stopSpeed());
        }
    }
}

