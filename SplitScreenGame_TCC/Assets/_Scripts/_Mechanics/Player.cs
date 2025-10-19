using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    [Header("Referências")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform KitchenObjectHoldPoint;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Animator animator;

    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 7.0f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;

    private bool isWalking;
    private bool canDash = true;
    private bool isDashing = false;

    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    // Eventos
    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs { public BaseCounter selectedCounter; }

    private void Start()
    {
        gameInput.InitializeInput();

        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction += GameInput_OnDashAction;
        gameInput.OnPauseAction += GameInput_OnPauseAction;

        if (trailRenderer != null)
            trailRenderer.enabled = false;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsDashing", false);
            animator.SetBool("HasItem", false);
        }
    }

    private void OnDestroy()
    {
        gameInput.OnInteractAction -= GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction -= GameInput_OnDashAction;
        gameInput.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void Update()
    {
        if (!KitchenGameManager.Instance.CanPlayersMove()) return;

        HandleMovement();
        HandleInteractions();
        UpdateAnimatorParameters();
    }

    // -----------------------
    // ANIMAÇÕES
    // -----------------------
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("HasItem", HasKitchenObject());
        animator.SetBool("IsDashing", isDashing);
    }

    // -----------------------
    // ENTRADAS
    // -----------------------
    private void GameInput_OnInteractAction(object sender, GameInput.InputActionEventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        TryInteractWithPortal();

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);

            // Aciona animação de pegar item (caso o counter entregue algo)
            if (!HasKitchenObject())
            {
                // A animação será disparada ao receber o objeto em SetKitchenObject
            }
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, GameInput.InputActionEventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        selectedCounter?.InteractAlternate(this);
    }

    private void GameInput_OnDashAction(object sender, GameInput.InputActionEventArgs e)
    {
        if (!KitchenGameManager.Instance.CanPlayersMove()) return;
        if (canDash && !isDashing)
            StartCoroutine(PerformDash());
    }

    private void GameInput_OnPauseAction(object sender, GameInput.InputActionEventArgs e)
    {
        KitchenGameManager.Instance.TogglePauseGame();
    }

    // -----------------------
    // MOVIMENTO E DASH
    // -----------------------
    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        float playerHeight = 2f;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = Mathf.Abs(moveDir.x) > 0.5f && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            if (canMove) moveDir = moveDirX;
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = Mathf.Abs(moveDir.z) > 0.5f && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
                if (canMove) moveDir = moveDirZ;
            }
        }

        if (canMove)
            transform.position += moveDir * moveDistance;

        isWalking = moveDir != Vector3.zero;

        if (moveDir != Vector3.zero)
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * 10f);
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        UpdateAnimatorParameters();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayDashSound(transform.position);

        if (trailRenderer != null)
            trailRenderer.enabled = true;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 dashDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        float dashTime = 0.1f;
        float elapsedTime = 0f;
        float playerRadius = 0.7f;
        float playerHeight = 2f;

        while (elapsedTime < dashTime)
        {
            float dashStep = (dashDistance / dashTime) * Time.deltaTime;
            Vector3 nextPosition = transform.position + dashDirection * dashStep;

            bool hit = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, dashDirection, dashStep);
            if (!hit)
                transform.position = nextPosition;
            else break;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        UpdateAnimatorParameters();

        if (trailRenderer != null)
            trailRenderer.enabled = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // -----------------------
    // INTERAÇÃO
    // -----------------------
    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
            lastInteractDir = moveDir;

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, counterLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
                SetSelectedCounter(baseCounter);
            else
                SetSelectedCounter(null);
        }
        else
            SetSelectedCounter(null);
    }

    private void SetSelectedCounter(BaseCounter newSelectedCounter)
    {
        if (selectedCounter == newSelectedCounter) return;

        if (selectedCounter != null)
            selectedCounter.CounterVisual.HideCounterVisual();

        selectedCounter = newSelectedCounter;

        if (selectedCounter != null)
            selectedCounter.CounterVisual.ShowCounterVisual();

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = newSelectedCounter });
    }

    // -----------------------
    // KITCHEN OBJECT
    // -----------------------
    public Transform GetKitchenObjectFollowTransform() => KitchenObjectHoldPoint;

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);

            if (animator != null)
                animator.SetTrigger("PickItem");
        }
    }

    public KitchenObject GetKitchenObject() => kitchenObject;
    public void ClearKitchenObject() => kitchenObject = null;
    public bool HasKitchenObject() => kitchenObject != null;

    // -----------------------
    // PORTAL
    // -----------------------
    private void TryInteractWithPortal()
    {
        float interactDistance = 2f;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, interactDistance))
        {
            if (hit.transform.TryGetComponent(out Portal portal))
                portal.TryInteractTeleport(this);
        }
    }

    public bool IsWalking() => isWalking;
}
