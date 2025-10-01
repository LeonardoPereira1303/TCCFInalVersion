using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter 
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public static event EventHandler OnAnyContainerUsed;

    public override void Interact(Player player){
        if(!player.HasKitchenObject())
        {
            //Player is not carrying anything
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

            OnAnyContainerUsed?.Invoke(this, EventArgs.Empty);
        }
    }
}
