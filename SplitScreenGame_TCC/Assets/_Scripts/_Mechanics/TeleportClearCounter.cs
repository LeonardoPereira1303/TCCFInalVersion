using System.Collections;
using UnityEngine;

public class TeleportClearCounter : ClearCounter
{
    [Tooltip("Tempo em segundos para o objeto ser teletransportado")]
    [SerializeField] private float teleportDelay = 3f;
    
    [Tooltip("Bancada de destino para onde o objeto ser� teleportado")]
    [SerializeField] private BaseCounter targetCounter;

    private Coroutine teleportCoroutine;

    public override void Interact(Player player)
    {
        // Chama a intera��o padr�o do ClearCounter
        base.Interact(player);

        // Se a bancada possui um objeto e ainda n�o iniciou o teletransporte, inicia a coroutine
        if (HasKitchenObject() && teleportCoroutine == null)
        {
            teleportCoroutine = StartCoroutine(TeleportAfterDelay());
        }
    }

    private IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(teleportDelay);

        // Verifica se o objeto ainda est� presente, se a bancada de destino est� definida
        // e se ela n�o est� ocupada (ou seja, se n�o possui nenhum objeto)
        if (HasKitchenObject() && targetCounter != null && !targetCounter.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectParent(targetCounter);
        }
        
        teleportCoroutine = null;
    }
}
