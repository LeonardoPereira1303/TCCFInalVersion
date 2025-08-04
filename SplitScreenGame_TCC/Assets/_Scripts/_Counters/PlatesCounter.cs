using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlatesCounter : BaseCounter
{
    public event EventHandler onPlateSpawned;
    public event EventHandler onPlateRemoved;
    [SerializeField] private KitchenObjectSO plateKicthenObjectSO;
   private float spawnPlateTimer;
   private float spawnPlateTimerMax = 4f;
   private int platesSpawnedAmount;
   private int platesSpawnedAmountMax = 4;

   private void Update()
   {
        spawnPlateTimer += Time.deltaTime;
        if(spawnPlateTimer > spawnPlateTimerMax){
            spawnPlateTimer = 0f;

            if(platesSpawnedAmount < platesSpawnedAmountMax){
                platesSpawnedAmount++;

                onPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
   }

   public override void Interact(Player player){
        if(!player.HasKitchenObject()){
            //Player is empty handed
            if(platesSpawnedAmount > 0){
                //There's at least one plate here
                platesSpawnedAmount--;

                KitchenObject.SpawnKitchenObject(plateKicthenObjectSO, player);

                onPlateRemoved?.Invoke(this, EventArgs.Empty);
            }
        }
   }
}
