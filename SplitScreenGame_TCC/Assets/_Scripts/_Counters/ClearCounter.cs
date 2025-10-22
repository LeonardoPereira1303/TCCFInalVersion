using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        //  Importante: dispara o evento global antes de qualquer outra lógica
        base.Interact(player);

        if (!HasKitchenObject())
        {
            // Não há objeto na bancada
            if (player.HasKitchenObject())
            {
                // Jogador coloca item na bancada
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else
            {
                // Jogador e bancada estão vazios -> nada a fazer
            }
        }
        else
        {
            // Há um objeto na bancada
            if (player.HasKitchenObject())
            {
                // Jogador tenta combinar itens (ex: prato + ingrediente)
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
                else
                {
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else
            {
                // Jogador pega o item da bancada
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}
