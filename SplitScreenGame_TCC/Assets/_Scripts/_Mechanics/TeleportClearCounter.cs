using System.Collections;
using UnityEngine;

public class TeleportClearCounter : ClearCounter
{
    [Tooltip("Tempo em segundos para o objeto ser teletransportado")]
    [SerializeField] private float teleportDelay = 3f;
    
    [Tooltip("Bancada de destino para onde o objeto será teleportado")]
    [SerializeField] private BaseCounter targetCounter;

    private Coroutine teleportCoroutine;

    public override void Interact(Player player)
    {
        // Chama a interação padrão do ClearCounter
        base.Interact(player);

        // Se a bancada possui um objeto e ainda não iniciou o teletransporte, inicia a coroutine
        if (HasKitchenObject() && teleportCoroutine == null)
        {
            teleportCoroutine = StartCoroutine(TeleportAfterDelay());
        }
    }

    private IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(teleportDelay);

        // Verifica se o objeto ainda está presente, se a bancada de destino está definida
        // e se ela não está ocupada (ou seja, se não possui nenhum objeto)
        if (HasKitchenObject() && targetCounter != null && !targetCounter.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectParent(targetCounter);
        }
        
        teleportCoroutine = null;
    }
}
