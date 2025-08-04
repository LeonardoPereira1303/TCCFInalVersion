using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectPlacedHere;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] SelectedCounterVisual counterVisual;

    private KitchenObject kitchenObject;

    private void Awake()
    {
        if(counterVisual == null)
            counterVisual = transform.GetComponentInChildren<SelectedCounterVisual>();
    }

    public SelectedCounterVisual CounterVisual { get { return counterVisual; } }

   public virtual void Interact(Player player) {
        Debug.LogError("BaseCounter.Interact()");
   }

   public virtual void InteractAlternate(Player player) {
        //Debug.LogError("BaseCounter.InteractAlternate()");ss
   }

    public Transform GetKitchenObjectFollowTransform() {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void ClearKitchenObject(){
        kitchenObject = null;
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }
}
