using DG.Tweening;
using System.Collections;
using UnityEngine;

public enum MoveDirection
{
    Right,
    Up,
    Down,
    Left,
}
public class PlayerController : MonoBehaviour
{
    private BoardManager boardManager;
    private Rigidbody2D rb;
    public ParticleSystem dust;
    public ParticleSystem star;
    public ParticleSystem lightFx;
    public ParticleSystem land;

    private int width;
    private int height;

    //Jumping
    private Vector3 lastPosition;
    private float previousTime;
    public float fallTime = 0.4f;
    private bool canFall = true;
    private int heigthJumped = 0;
    public int jumpHeight = 3;
    public bool canJump = true;
    private int maxHorizontalMoveInAir = 3;
    private int horizontalMovedInAir = 0;

    //Moving
    private Vector3 targetPosition;
    private bool isMoving = false;
    float movementDuration = 0.05f;
    public float moveSpeed = 1f;
    public MoveDirection moveDirection;

    //Input buffer
    private Vector3 queuedInput = Vector3.zero;

    private SpriteRenderer spriteRenderer;
    private Color32 originalMaterialColor;

    Transform spriteTransform;

    public int heightClimbed = 0;

    private bool animPlayed = false;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private PlayerAnimationController animController;
    public bool hasLanded;

    private int facingDir = 1;
    private bool facingRight = true;

    private bool isWon = false;
    private bool isCheckingCollision = false;

    private AudioController audioController;
    private void Awake()
    {
        spriteTransform = transform.Find("Animator");
    }
    void Start()
    {
        
        boardManager = BoardManager.instance;
        spriteRenderer = spriteTransform.gameObject.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioController = AudioController.instance;

        width = boardManager.grid.GetLength(0);
        height = boardManager.grid.GetLength(1);

        // Set the player's initial position on the grid
        boardManager.grid[(int)RoundedPos(transform.position).x, (int)RoundedPos(transform.position).y] = this.transform;
        originalMaterialColor = spriteRenderer.material.color;

        animController.animator.Play("capyIdle", 0, 0);
    }
    private void Update()
    {
        if (!isWon)
        {
            addToGrid(lastPosition);

        }
        int posYWinThreshold = 17;
        //prevent spawning block when the player can jump to victory, ensuring the player has touch the ground
        if (transform.position.y >= posYWinThreshold && canJump && !isWon)
        {
            boardManager.canSpawn = false;
            if (Input.GetKeyDown(KeyCode.Space) && !boardManager.gameLost)
            {
                WIN();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            WIN();
        }
        HorizontalMovement();
        JumpAndFallMovement();
        HandleInputBuffering();

        // Update the grid every frame, even when not moving

        if (IsGrounded())
        {
            animPlayed = false;

        }
        else
        {
            hasLanded = false;
        }
        if (IsGrounded() && !hasLanded)
        {
            animController.animator.Play("capyLanding", 0, 0);
            audioController.PlaySound(audioController.landing);
            land.Play();
            hasLanded = true;
        }

    }

    private void WIN()
    {
        Debug.Log("WONNN");
        boardManager.IsWonAnimation();
        transform.DOMoveY(transform.position.y + 30, 2).SetEase(Ease.InOutBack);
        audioController.PlaySound(audioController.passLevel);
        rb.isKinematic = true;
        isWon = true;
    }
    public void GetToNextLevel()
    {
        int originalY = 0;
        transform.position = new Vector2(transform.position.x, -4);
        transform.DOMoveY(originalY, 2).SetEase(Ease.InOutBack).OnComplete(() =>
        {

            
            rb.isKinematic = false;
            GetComponent<BoxCollider2D>().enabled = true;

            if (boardManager.levelController.CurrentLevel() < 2)
            {
                boardManager.canSpawn = true;
                FindObjectOfType<TetrisRandomizer>().SpawnNewTetromino();
                isWon = false;
            }
            else
            {
                StartCoroutine(DashLoopSequence());
            }

        });

    }
    private IEnumerator DashLoopSequence()
    {
        FlipController(-1);
        for (int i = 0; i < 30; i++)
        {
            // Play dash animation
            animController.animator.Play("capyDash", 0 ,0);
            audioController.PlaySound(audioController.move);

            // Move the object
            transform.DOMove(transform.position + Vector3.right * 1, 0.1f).SetEase(Ease.Linear);

            // Wait for the dash duration
            yield return new WaitForSeconds(0.1f);

            // Wait for the delay
            yield return new WaitForSeconds(0.05f);
        }
        // Reset position after the loop
    }

    #region FLIP
    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
        {
            Flip();
        }
        if (_x < 0 && facingRight)
        {
            Flip();
        }
    }
    #endregion
    #region MOVEMENT
    private void JumpAndFallMovement()
    {
        if (transform.position.y == 0)
        {
            canFall = false;
            canJump = true;
            horizontalMovedInAir = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isMoving)
        {
            canFall = false;
            StartCoroutine(JumpUp());
        }
        if (Time.time - previousTime > fallTime && canFall && !isMoving)
        {
            if (validMove(new Vector3(0, -1, 0)))
            {
                canJump = false;
                targetPosition = transform.position + new Vector3(0, -1, 0) * moveSpeed;
                StartCoroutine(MoveForward(RoundedPos(targetPosition)));
                if (!animPlayed)
                {
                    animController.animator.Play("capyFall", 0, 0);
                    animPlayed = true;
                }
            }
            else
            {
                canJump = true;
                horizontalMovedInAir = 0;
            }
            previousTime = Time.time;
        }
    }



    private void HorizontalMovement()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving)
        {
            FlipController(1);
            if (!maxHorizontalMoveInAirAchieved())
            {
                targetPosition = transform.position + new Vector3(-1, 0, 0) * moveSpeed;
                //Debug.Log(validMove(new Vector3(-1, 0, 0)));
                if (validMove(new Vector3(-1, 0, 0)))
                {
                    animController.animator.Play("capyDash", 0, 0);
                    audioController.PlaySound(audioController.move);
                    
                    StartCoroutine(MoveForward(targetPosition));
                }
                if (!canJump)
                {
                    horizontalMovedInAir++;
                }
            }
            
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving )
        {
            FlipController(-1);
            if (!maxHorizontalMoveInAirAchieved())
            {
                targetPosition = transform.position + new Vector3(1, 0, 0) * moveSpeed;
                if (validMove(new Vector3(1, 0, 0)))
                {
                    animController.animator.Play("capyDash", 0, 0);
                    audioController.PlaySound(audioController.move);
                    StartCoroutine(MoveForward(targetPosition));
                }
                if (!canJump)
                {
                    horizontalMovedInAir++;
                }

            }
                
        }
    }
    public bool maxHorizontalMoveInAirAchieved() 
    {
        animController.animator.Play("capyHurt", 0, 0);
        return horizontalMovedInAir > maxHorizontalMoveInAir;
    }
    private void HandleInputBuffering()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving && !maxHorizontalMoveInAirAchieved())
        {
            queuedInput = new Vector3(-1, 0, 0);
            animController.animator.Play("capyDash", 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving && !maxHorizontalMoveInAirAchieved())
        {
            animController.animator.Play("capyDash", 0, 0);
            queuedInput = new Vector3(1, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isMoving)
        {
            queuedInput = new Vector3(0, 1, 0);
            canFall = false;
            StartCoroutine(JumpUp());
        }

        // Execute queued input after movement is finished
        if (!isMoving && queuedInput != Vector3.zero)
        {
            if (validMove(queuedInput))
            {
                Vector3 targetPosition = transform.position + queuedInput * moveSpeed;
                StartCoroutine(MoveForward(RoundedPos(targetPosition)));
            }
            queuedInput = Vector3.zero; // Clear the queued input
        }
    }
    #endregion

    #region GRID LOGIC
    private void addToGrid(Vector3 lastPosition)
    {

        int roundedlastPosX = Mathf.RoundToInt(lastPosition.x);
        int roundedLastPosY = Mathf.RoundToInt(lastPosition.y);

        int roundedX = Mathf.RoundToInt(transform.position.x);
        int roundedY = Mathf.RoundToInt(transform.position.y);

        // If the player is not moving, update the grid
        if (lastPosition == transform.position)
        {
            if (boardManager.grid[roundedX, roundedY] == null)
            {
                // Update the grid with the player's position
                boardManager.grid[roundedX, roundedY] = transform;
                return;
            }
        }
        // If the player is moving, update the grid with the new position and clear the old position
        if (boardManager.grid[roundedX, roundedY] == null)
        {
            boardManager.grid[roundedX, roundedY] = transform;
            boardManager.grid[roundedlastPosX, roundedLastPosY] = null;
            //Debug.Log(new Vector2(roundedlastPosX, roundedLastPosY));
        }
    }
    private bool validMove(Vector2 direction)
    {
        int roundedX = Mathf.RoundToInt(transform.position.x + direction.x);
        int roundedY = Mathf.RoundToInt(transform.position.y + direction.y);

        if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height)
            return false;

        if (boardManager.grid[roundedX, roundedY] != null && !boardManager.grid[roundedX, roundedY].gameObject.CompareTag("Obstacle"))
            return false;
        return true;
    }
    #endregion

    #region ACTUAL MOVEMENT LOGIC
    IEnumerator MoveForward(Vector3 targetPosition)
    {
        isMoving = true;
        if (IsGrounded())
        {
            dust.Play();
        }
        // Calculate the target position
        // Move the Rigidbody to the target position over one second
        float elapsedTime = 0f;
        lastPosition = transform.position;
        while (elapsedTime < movementDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / movementDuration;
            Vector2 roundedPos = RoundedPos(transform.position);
            Vector2 roundedTarget = RoundedPos(targetPosition);
            rb.MovePosition(Vector3.Lerp(roundedPos, roundedTarget, progress));
            // Update the grid during the movement
            addToGrid(lastPosition);
            yield return null;
        }

        // Ensure the player reaches the exact target position
        previousTime = Time.time;
        if (!isCheckingCollision)
        {
            moveDirection = DetermineMoveDirection(RoundedPos(lastPosition), RoundedPos(targetPosition));
        }
        rb.MovePosition(targetPosition);
        isMoving = false;
    }
    private Vector2 RoundedPos(Vector2 pos)
    {
        int roundedX = Mathf.RoundToInt(pos.x);
        int roundedY = Mathf.RoundToInt(pos.y);
        return new Vector2(roundedX, roundedY);
    }
    IEnumerator JumpUp()
    {
        animController.animator.Play("capyJump", 0, 0);
        audioController.PlaySound(audioController.jump);
        while (heigthJumped < jumpHeight && validMove(new Vector3(0, 1, 0)))
        {
            canJump = false;
            targetPosition = transform.position + new Vector3(0, 1, 0) * moveSpeed;
            yield return StartCoroutine(MoveForward((RoundedPos(targetPosition))));
            heigthJumped++;
            if (targetPosition.y > heightClimbed)
            {
                heightClimbed++;
                UIController.instance.ChangeHeightClimbedText(heightClimbed);
            } 
        }
        heigthJumped = 0;
        canFall = true;
        //animController.SwitchAnimation("Fall");
    }
    #endregion

    public void ChangePlayerMaterialColor()
    {
        StartCoroutine(ChangeMaterialColorCoroutine());
    }
    private IEnumerator ChangeMaterialColorCoroutine()
    {
        spriteRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.material.color = originalMaterialColor;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Medal"))
        {
            Destroy(collision.gameObject);
            boardManager.IncreaseMedalScore();
            star.Play();
            lightFx.Play();
            audioController.PlaySound(audioController.coin);
            
        }
        if (collision.CompareTag("Obstacle"))
        {
            isCheckingCollision = true;
            Vector2 roundedCollisionPos = RoundedPos(collision.transform.position);
            boardManager.grid[(int)roundedCollisionPos.x, (int)roundedCollisionPos.y] = null; // Set obstacle to null before knockback
            animController.animator.Play("capyHurt", 0, 0);
            audioController.PlaySound(audioController.hurt);
            Vector2 knockbackDirection = Vector2.zero;
            if (lastPosition.x - roundedCollisionPos.x < 0 && lastPosition.y == roundedCollisionPos.y)
            {
                //push right
                Debug.Log(lastPosition.x + "Left");
                knockbackDirection = new Vector2(roundedCollisionPos.x - 1, roundedCollisionPos.y);
                previousTime = Time.time;
            }
            if (lastPosition.x - roundedCollisionPos.x > 0 && lastPosition.y == roundedCollisionPos.y)
            {
                Debug.Log(lastPosition.x + "Right");
                //push left
                knockbackDirection = new Vector2(roundedCollisionPos.x + 1, roundedCollisionPos.y);
                previousTime = Time.time;

            }
            if (lastPosition.x == roundedCollisionPos.x && lastPosition.y > roundedCollisionPos.y)
            {
                // Push up
                Debug.Log(lastPosition.y + "UP");
                knockbackDirection = new Vector2(roundedCollisionPos.x, roundedCollisionPos.y + 1);
                previousTime = Time.time + 0.2f;
            }
            if (lastPosition.x == roundedCollisionPos.x && lastPosition.y < roundedCollisionPos.y)
            {
                //push down
                Debug.Log(lastPosition.y + "DOWN");
                knockbackDirection = new Vector2(roundedCollisionPos.x, roundedCollisionPos.y - 1);  // Push left
                previousTime = Time.time;
            }
            // Get the desired knockback position
            Vector2 knockbackPos = DesiredHitPos(knockbackDirection);
                Debug.Log(knockbackPos);

                Vector2 previousPlayerPos = RoundedPos(transform.position);

                // Add a small delay before applying the knockback

                StartCoroutine(ApplyKnockback(knockbackPos, collision.transform, roundedCollisionPos, previousPlayerPos));

            }
        
    }

    private IEnumerator ApplyKnockback(Vector2 knockbackPos, Transform obstacle, Vector2 roundedCollisionPos, Vector2 previousPlayerPos)
    {
        yield return new WaitForSeconds(0.1f); // Adjust the delay as needed

        
        StartCoroutine(MoveForward(knockbackPos));
        previousTime = Time.time; // Reset the fall timer
        
        boardManager.grid[(int)roundedCollisionPos.x, (int)roundedCollisionPos.y] = obstacle;

        // Set the player's previous position to null
        boardManager.grid[(int)previousPlayerPos.x, (int)previousPlayerPos.y] = null;



    }

    private Vector2 DesiredHitPos(Vector2 targetPos)
    {

        // Ensure the knockback position is within the grid bounds
        targetPos.x = Mathf.Clamp(targetPos.x, 0, width - 1);
        targetPos.y = Mathf.Clamp(targetPos.y, 0, height - 1);

        return targetPos;
    }
    private bool IsGrounded() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    private void OnDrawGizmos()
    {
        // Draw a debug ray to visualize the ground check distance
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
    }

    private MoveDirection DetermineMoveDirection(Vector2 lastPos, Vector2 afterMovePos)
    {
        if (afterMovePos.x - lastPos.x > 0 && afterMovePos.y == lastPos.y)
        {
            return MoveDirection.Right;
        }
        if (afterMovePos.x - lastPos.x < 0 && afterMovePos.y == lastPos.y)
        {
            return MoveDirection.Left;
        }
        if (afterMovePos.y - lastPos.y < 0 && afterMovePos.x == lastPos.x)
        {
            return MoveDirection.Down;
        }
        if (afterMovePos.y - lastPos.y > 0 && afterMovePos.x == lastPos.x)
        {
            return MoveDirection.Up;
        }
        return MoveDirection.Left;
    }
}