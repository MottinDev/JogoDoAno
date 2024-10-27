using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;
    [SerializeField] private TrailRenderer tr;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 16f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    [SerializeField] private float iFrameDuration = 0.3f; // Dura��o do I-frame ap�s o dash

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private float slopeAngleThreshold = 30f;

    private float dirX = 0f;
    private float lastDirectionX = 1f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float gravityScale = 1.0f;
    [SerializeField] private float crouchSpeedReduction = 2f;

    private bool isCrouching = false;
    private Vector2 crouchingColliderSize = new Vector2(1f, 0.5f); // Ajuste esse tamanho para corresponder � sua sprite agachada
    private Vector2 originalColliderSize;

    private bool isSprinting = false;
    private float doubleTapTime = 0.2f;
    private float lastTapTimeD = 0f;
    private float lastTapTimeA = 0f;

    private enum MovementState { idle = 0, running = 1, jumping = 2, falling = 3, crouching = 4, sprinting = 5 }

    [SerializeField] private AudioSource jumpSoundEffect;

    private bool isOnSlope = false;
    private bool isJumping = false;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // Variável para wall sliding e wall jump
    private bool isFacingRight = true;
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(2f, 4f);

    // Novas vari�veis para Jump Buffering e Coyote Time
    private bool isJumpPressed = false;
    [SerializeField] private float jumpBufferTime = 0.1f;  // Tempo para jump buffering
    [SerializeField] private float coyoteTime = 0.2f;      // Tempo para coyote time
    [SerializeField] private float variableJumpMultiplier = 0.5f;  // Multiplicador para altura vari�vel de pulo
    [SerializeField] private float crouchOffset = 0.1f;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private int originalLayer; // Armazena a camada original do jogador

    // Vari�veis para o Pulo Duplo
    private int jumpCount = 0;
    [SerializeField] private int maxJumpCount = 2; // Define o m�ximo de pulos (2 para pulo duplo)

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = gravityScale;
        originalColliderSize = coll.size;

        // Armazena a layer original do jogador
        originalLayer = gameObject.layer;
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        dirX = Input.GetAxisRaw("Horizontal");

        if (IsGrounded())
        {
            float slopeAngle = GetSlopeAngle();

            if (slopeAngle > slopeAngleThreshold)
            {
                isOnSlope = true;
            }
            else if (slopeAngle < -slopeAngleThreshold)
            {
                isOnSlope = true;
            }
            else
            {
                isOnSlope = false;
            }
        }

        // Atualiza o jump buffer e o coyote time counters
        jumpBufferCounter -= Time.deltaTime;
        coyoteTimeCounter -= Time.deltaTime;

        // Jump buffering: Se o jogador apertou o bot�o de pulo recentemente, armazenamos isso
        if (Input.GetButtonDown("Jump"))
        {
            isJumpPressed = true;
            jumpBufferCounter = jumpBufferTime;
        }

        // Coyote time: Permite que o jogador pule mesmo um pouco ap�s ter deixado o ch�o
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            jumpCount = 0; // Reseta o contador de pulos ao tocar o ch�o
        }

        // Permite pular se o jump buffer estiver ativo e o jogador n�o tiver excedido o n�mero m�ximo de pulos
        if (jumpBufferCounter > 0 && (coyoteTimeCounter > 0 || jumpCount < maxJumpCount))
        {
            Jump();
            jumpBufferCounter = 0;  // Reseta o jump buffer ap�s pular
        }

        // Controle de altura vari�vel do pulo
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpMultiplier);
        }

        if (Input.GetKey(KeyCode.S) && IsGrounded())
        {
            isCrouching = true;
            coll.size = crouchingColliderSize;
            coll.offset = new Vector2(0f, crouchOffset); // Ajuste o valor do offset para evitar que entre no ch�o
            if (Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                rb.AddForce(new Vector2(-rb.velocity.x * crouchSpeedReduction, 0f), ForceMode2D.Force);
            }
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            isCrouching = false;
            coll.size = originalColliderSize;
            coll.offset = Vector2.zero; // Restaura o offset original
        }

        // Atualiza a dire��o do sprite (flipX) baseado no input horizontal, mas s� se houver movimento horizontal
        if (dirX != 0f)
        {
            lastDirectionX = dirX; // Armazena a �ltima dire��o horizontal
            sprite.flipX = dirX < 0f;
        }

        if (!isCrouching && !isJumping)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (Time.time - lastTapTimeD < doubleTapTime)
                {
                    isSprinting = true;
                }
                lastTapTimeD = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (Time.time - lastTapTimeA < doubleTapTime)
                {
                    isSprinting = true;
                }
                lastTapTimeA = Time.time;
            }

            if (isSprinting && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)))
            {
                rb.velocity = new Vector2(dirX * sprintSpeed, rb.velocity.y);
                sprite.flipX = dirX < 0f;
            }
            else
            {
                isSprinting = false;
                rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash());
            }
        }

        // Aplicando gravidade extra na descida
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (gravityScale - 1) * Time.deltaTime;
        }

        WallSlide();
        WallJump();

        if (!isWallJumping){
            Flip();
        }

        UpdateAnimationState();
    }

    private void Jump()
    {
        if (jumpCount < maxJumpCount)
        {
            isJumping = true;
            jumpSoundEffect.Play();
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim.SetInteger("state", (int)MovementState.jumping);
            jumpCount++; // Incrementa o contador de pulos
        }
    }

    private void UpdateAnimationState()
    {
        if (isDashing)
        {
            return;
        }

        MovementState state;

        if (isCrouching)
        {
            state = MovementState.crouching;
        }
        else if (isJumping && rb.velocity.y > 0.1f) // Se estiver pulando para cima
        {
            state = MovementState.jumping;
            sprite.flipX = lastDirectionX < 0f; // Usa a �ltima dire��o armazenada
        }
        else if (rb.velocity.y < -0.1f && !IsGrounded()) // Se estiver caindo
        {
            state = MovementState.falling;
            sprite.flipX = lastDirectionX < 0f; // Mant�m a dire��o no ar
        }
        else if (isSprinting && IsGrounded()) // Se estiver correndo e no ch�o
        {
            state = MovementState.sprinting;
        }
        else if (dirX != 0f && IsGrounded()) // Se estiver se movendo e no ch�o
        {
            state = MovementState.running;
            sprite.flipX = dirX < 0f; // Atualiza a dire��o com base no input horizontal
        }
        else if (!isJumping && !isSprinting && IsGrounded()) // Se n�o estiver pulando, nem correndo e estiver no ch�o
        {
            state = MovementState.idle;
            sprite.flipX = lastDirectionX < 0f;
        }
        else
        {
            state = MovementState.falling;
            sprite.flipX = lastDirectionX < 0f;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
        if (hit.collider != null)
        {
            isJumping = false;
            coyoteTimeCounter = coyoteTime;
            // O reset do jumpCount foi movido para o Update() para evitar conflitos
            return true;
        }
        return false;
    }

    // Verificação se está na parede
    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    // Implementação do wall sliding
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && dirX != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            
            // se a velocidade do y é menor que a velocidade do wall sliding manter negativo
            if (rb.velocity.y > -wallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void Flip(){
        if (isFacingRight && dirX < 0f || !isFacingRight && dirX > 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            wallJumpingCounter = wallJumpingTime; // Resetar o contador do wall jump quando estiver wall sliding
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime; 
            // Diminuir o contador do wall jump se não estive sliding
        }

        // Checar botão jump
        if (Input.GetButtonDown("Jump"))
        {
            if (wallJumpingCounter > 0f)
            {
                // Checar qual botão é pressionando enquanto pulando
                if (Input.GetKeyDown(KeyCode.D)) 
                {
                    wallJumpingDirection = 1f; 
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    wallJumpingDirection = -1f; 
                }
                else
                {
                    Jump();
                    return; 
                }

                // Executar wall jump
                rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
                wallJumpingCounter = 0f; 
            }
        }
    }


    private void StopWallJumping(){
        isWallJumping = false;
    }

    private float GetSlopeAngle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.0f, jumpableGround);
        if (hit.collider != null)
        {
            return Vector2.Angle(hit.normal, Vector2.up);
        }
        return 0f;
    }

    private IEnumerator Dash()
    {
        if (!canDash) yield break;

        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = sprite.flipX ? Vector2.left : Vector2.right;
        }

        rb.velocity = dashDirection * dashingPower;

        tr.emitting = true;

        // Ativa I-frames (invulnerabilidade) mudando a layer temporariamente
        gameObject.layer = LayerMask.NameToLayer("Invulnerable");

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;

        // Aguarda a dura��o do I-frame
        yield return new WaitForSeconds(iFrameDuration);

        // Retorna o jogador para a layer original
        gameObject.layer = originalLayer;

        yield return new WaitForSeconds(dashingCooldown - iFrameDuration);
        canDash = true;
    }

    private void OnDrawGizmos()
    {
        if (coll != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(coll.bounds.center, coll.bounds.size);
        }
    }
}
