using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float moveSpeed = 7.0f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform KitchenObjectHoldPoint;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    [SerializeField] private TrailRenderer trailRenderer; // Refer�ncia ao Trail Renderer


    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing = false;

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    private void Start()
    {
        gameInput.InitializeInput();

        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction += GameInput_OnDashAction;
        gameInput.OnPauseAction += GameInput_OnPauseAction;
        // Certifique-se de que o Trail Renderer est� desativado inicialmente
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        gameInput.OnInteractAction -= GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction -= GameInput_OnDashAction;
        gameInput.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void GameInput_OnInteractAction(object sender, GameInput.InputActionEventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        TryInteractWithPortal();


        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, GameInput.InputActionEventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnDashAction(object sender, GameInput.InputActionEventArgs e)
    {
        if (canDash && !isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    private void GameInput_OnPauseAction(object sender, GameInput.InputActionEventArgs e)
    {
        KitchenGameManager.Instance.TogglePauseGame();
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Ativar o Trail Renderer no in�cio do dash
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }

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

            bool hit = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, dashDirection,
                dashStep
            );

            if (!hit)
            {
                transform.position = nextPosition;
            }
            else
            {
                break; // Parar dash ao colidir
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        // Desativar o Trail Renderer ao final do dash
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Update()
    {

        if (!KitchenGameManager.Instance.CanPlayersMove())
        {
            // Anula qualquer entrada enquanto não puder mover
            return;
        }
        
        HandleMovement();
        HandleInteractions();
    }

    public bool isWalkingMethod()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, counterLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

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
            canMove = (Mathf.Abs(moveDir.x) > 0.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (Mathf.Abs(moveDir.z) > 0.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDir * moveDistance;
        }

        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
    }

    private void SetSelectedCounter(BaseCounter newSelectedCounter)
    {
        if (selectedCounter != newSelectedCounter)
        {
            if (selectedCounter != null)
            {
                selectedCounter.CounterVisual.HideCounterVisual();
            }

            selectedCounter = newSelectedCounter;

            if (selectedCounter != null)
            {
                selectedCounter.CounterVisual.ShowCounterVisual();
            }

            OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = newSelectedCounter });
        }
    }

    public Transform GetKitchenObjectFollowTransform() => KitchenObjectHoldPoint;
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObject() => kitchenObject;
    public void ClearKitchenObject() => kitchenObject = null;
    public bool HasKitchenObject() => kitchenObject != null;

    private void TryInteractWithPortal()
    {
        float interactDistance = 2f;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f; // Ajuste da altura do raycast
        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, interactDistance))
        {
            if (hit.transform.TryGetComponent(out Portal portal))
            {
                portal.TryInteractTeleport(this);
            }
        }
    }
}
