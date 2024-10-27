using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Componentes
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;
    [SerializeField] private TrailRenderer tr;

    // Movimento
    private float dirX = 0f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float airMoveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float gravityScale = 3.0f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    // Dash
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 16f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.1f;
    [SerializeField] private float iFrameDuration = 0.3f; // Duração da invulnerabilidade após o dash
    private bool hasAirDashed = false;

    // Agachamento
    private bool isCrouching = false;
    private Vector2 crouchingColliderSize = new Vector2(1f, 0.5f);
    private Vector2 originalColliderSize;
    [SerializeField] private float crouchOffset = 0.1f;
    private float crouchSpeedReduction = 2f;

    // Sprint
    private bool isSprinting = false;
    private float doubleTapTime = 0.2f;
    private float lastTapTimeD = 0f;
    private float lastTapTimeA = 0f;

    // Estados de movimento
    private enum MovementState { idle = 0, running = 1, jumping = 2, falling = 3, crouching = 4, sprinting = 5, wallSliding = 6 }

    // Áudio
    [SerializeField] private AudioSource jumpSoundEffect;

    // Verificações de terreno e parede
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // Wall Sliding e Wall Jumping
    private bool isJumping = false;
    private bool isFacingRight = true;
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(15f, 20f);

    // Jump Buffering e Variable Jump Height
    private bool isJumpPressed = false;
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    // Contagem de pulos para pulo duplo
    private int jumpCount = 0;
    [SerializeField] private int maxJumpCount = 2; // Máximo de pulos (2 para pulo duplo)

    // Layer original do jogador
    private int originalLayer;

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
            jumpCount = 0; // Reseta o contador de pulos ao tocar o chão
            hasAirDashed = false; // Reseta a disponibilidade do dash no ar
            canDash = true; // Permite dash novamente
        }

        // Detecção de parede
        if (IsWalled())
        {
            hasAirDashed = false; // Reseta o dash no ar ao tocar a parede
            canDash = true; // Permite dash novamente
        }

        // Atualiza o contador do jump buffer
        jumpBufferCounter -= Time.deltaTime;

        // Jump buffering: Armazena o input de pulo
        if (Input.GetButtonDown("Jump"))
        {
            isJumpPressed = true;
            jumpBufferCounter = jumpBufferTime;
        }

        HandleMovement();

        WallSlide();
        WallJump();

        UpdateAnimationState();
    }

    private void HandleMovement()
    {
        // Controle do agachamento
        if (Input.GetKey(KeyCode.S) && IsGrounded())
        {
            isCrouching = true;
            coll.size = crouchingColliderSize;
            coll.offset = new Vector2(0f, crouchOffset); // Ajuste o offset para evitar afundar no chão
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

        // Movimento no chão
        if (IsGrounded() && !isWallJumping)
        {
            if (!isCrouching && !isJumping)
            {
                // Sprint
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
                }
                else
                {
                    isSprinting = false;
                    rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
                }
            }

            // Dash
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash());
            }
        }
        else // Movimento no ar
        {
            // Permitir controle aéreo
            rb.velocity = new Vector2(dirX * airMoveSpeed, rb.velocity.y);

            // Dash no ar
            if (Input.GetKeyDown(KeyCode.LeftShift) && !hasAirDashed && !IsGrounded() && canDash)
            {
                StartCoroutine(Dash());
                hasAirDashed = true;
            }
        }

        // Implementa melhor sensação de pulo
        if (rb.velocity.y < 0)
        {
            // Caindo
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Pulo baixo
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Atualiza a direção do sprite
        Flip();

        // Pulo
        if (jumpBufferCounter > 0 && jumpCount < maxJumpCount && !isWallSliding)
        {
            Jump();
            jumpBufferCounter = 0;  // Reseta o jump buffer após pular
        }
    }

    private void Jump()
    {
        isJumping = true;
        jumpSoundEffect.Play();

        // Ajusta a força do pulo com base na velocidade horizontal do jogador
        float adjustedJumpForce = jumpForce + Mathf.Abs(rb.velocity.x) * 0.1f; // Ajuste o multiplicador conforme necessário

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * adjustedJumpForce, ForceMode2D.Impulse);
        jumpCount++; // Incrementa o contador de pulos
    }

    private void UpdateAnimationState()
    {
        if (isDashing)
        {
            return;
        }

        MovementState state;

        if (isWallSliding)
        {
            state = MovementState.wallSliding;
        }
        else if (isCrouching)
        {
            state = MovementState.crouching;
        }
        else if (isJumping && rb.velocity.y > 0.1f) // Pulando para cima
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -0.1f && !IsGrounded()) // Caindo
        {
            state = MovementState.falling;
        }
        else if (isSprinting && IsGrounded()) // Correndo no chão
        {
            state = MovementState.sprinting;
        }
        else if (dirX != 0f && IsGrounded()) // Andando no chão
        {
            state = MovementState.running;
        }
        else if (!isJumping && !isSprinting && IsGrounded()) // Parado no chão
        {
            state = MovementState.idle;
        }
        else
        {
            state = MovementState.falling;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, groundLayer);
        if (hit.collider != null)
        {
            isJumping = false;
            return true;
        }
        return false;
    }

    // Verifica se está na parede
    private bool IsWalled()
    {
        bool isWalledLeft = Physics2D.OverlapCircle(wallCheckLeft.position, 0.2f, wallLayer);
        bool isWalledRight = Physics2D.OverlapCircle(wallCheckRight.position, 0.2f, wallLayer);
        return isWalledLeft || isWalledRight;
    }

    // Implementação do wall sliding
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && dirX != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void Flip()
    {
        if (dirX > 0)
        {
            isFacingRight = true;
            sprite.flipX = false;
        }
        else if (dirX < 0)
        {
            isFacingRight = false;
            sprite.flipX = true;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = GetWallJumpDirection();
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            // Reseta o contador de pulos ao fazer wall jump
            jumpCount = 1; // Permite um pulo adicional após o wall jump

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private float GetWallJumpDirection()
    {
        bool isWalledLeft = Physics2D.OverlapCircle(wallCheckLeft.position, 0.2f, wallLayer);
        bool isWalledRight = Physics2D.OverlapCircle(wallCheckRight.position, 0.2f, wallLayer);

        if (isWalledLeft)
            return 1; // Pula para a direita
        else if (isWalledRight)
            return -1; // Pula para a esquerda
        else
            return isFacingRight ? 1 : -1; // Direção padrão
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
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
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;
        }

        rb.velocity = dashDirection * dashingPower;

        tr.emitting = true;

        // Ativa I-frames (invulnerabilidade) mudando temporariamente a layer
        gameObject.layer = LayerMask.NameToLayer("Invulnerable");

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;

        // Aguarda a duração do I-frame
        yield return new WaitForSeconds(iFrameDuration);

        // Retorna o jogador para a layer original
        gameObject.layer = originalLayer;

        // Remova o comentário abaixo se desejar aplicar um cooldown adicional antes de permitir o dash novamente
        // yield return new WaitForSeconds(dashingCooldown);
    }

    private float GetSlopeAngle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.0f, groundLayer);
        if (hit.collider != null)
        {
            return Vector2.Angle(hit.normal, Vector2.up);
        }
        return 0f;
    }

    private void OnDrawGizmos()
    {
        if (wallCheckLeft != null && wallCheckRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheckLeft.position, 0.2f);
            Gizmos.DrawWireSphere(wallCheckRight.position, 0.2f);
        }

        // Opcional: Desenhar o collider do jogador
        if (coll != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(coll.bounds.center, coll.bounds.size);
        }
    }
}
