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

    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private float slopeAngleThreshold = 30f;

    private float dirX = 0f;
    private float lastDirectionX = 1f; // Armazena a última direção horizontal (1 = direita, -1 = esquerda)
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float gravityScale = 1.0f;
    [SerializeField] private float crouchSpeedReduction = 2f;

    private bool isCrouching = false;
    private Vector2 crouchingColliderSize = new Vector2(1f, 0.5f); // Ajuste esse tamanho para corresponder à sua sprite agachada
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

    // Novas variáveis para Jump Buffering e Coyote Time
    private bool isJumpPressed = false;
    [SerializeField] private float jumpBufferTime = 0.1f;  // Tempo para jump buffering
    [SerializeField] private float coyoteTime = 0.2f;      // Tempo para coyote time
    [SerializeField] private float variableJumpMultiplier = 0.5f;  // Multiplicador para altura variável de pulo
    [SerializeField] private float crouchOffset = 0.1f;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = gravityScale;
        originalColliderSize = coll.size;
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

        // Jump buffering: Se o jogador apertou o botão de pulo recentemente, armazenamos isso
        if (Input.GetButtonDown("Jump"))
        {
            isJumpPressed = true;
            jumpBufferCounter = jumpBufferTime;
        }

        // Coyote time: Permite que o jogador pule mesmo um pouco após ter deixado o chão
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0;  // Reset jump buffer após pular
        }

        // Controle de altura variável do pulo
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpMultiplier);
        }

        if (Input.GetKey(KeyCode.S) && IsGrounded())
        {
            isCrouching = true;
            coll.size = crouchingColliderSize;
            coll.offset = new Vector2(0f, crouchOffset); // Ajuste o valor do offset para evitar que entre no chão
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

        // Atualiza a direção do sprite (flipX) baseado no input horizontal, mas só se houver movimento horizontal
        if (dirX != 0f)
        {
            lastDirectionX = dirX; // Armazena a última direção horizontal
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

        UpdateAnimationState();
    }

    private void Jump()
    {
        isJumping = true;
        jumpSoundEffect.Play();
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        anim.SetInteger("state", (int)MovementState.jumping);
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
            sprite.flipX = lastDirectionX < 0f; // Usa a última direção armazenada
        }
        else if (rb.velocity.y < -0.1f && !IsGrounded()) // Se estiver caindo
        {
            state = MovementState.falling;
            sprite.flipX = lastDirectionX < 0f; // Mantém a direção no ar
        }
        else if (isSprinting && IsGrounded()) // Se estiver correndo e no chão
        {
            state = MovementState.sprinting;
        }
        else if (dirX != 0f && IsGrounded()) // Se estiver se movendo e no chão
        {
            state = MovementState.running;
            sprite.flipX = dirX < 0f; // Atualiza a direção com base no input horizontal
        }
        else if (!isJumping && !isSprinting && IsGrounded()) // Se não estiver pulando, nem correndo e estiver no chão
        {
            state = MovementState.idle;
            // Mantém a última direção escolhida no ar ou em terra
            sprite.flipX = lastDirectionX < 0f;
        }
        else
        {
            state = MovementState.falling;
            sprite.flipX = lastDirectionX < 0f; // Mantém a direção mesmo se estiver caindo
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
        if (hit.collider != null)
        {
            isJumping = false;
            return true;
        }
        return false;
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

        rb.AddForce(dashDirection * dashingPower, ForceMode2D.Impulse);

        tr.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);
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
