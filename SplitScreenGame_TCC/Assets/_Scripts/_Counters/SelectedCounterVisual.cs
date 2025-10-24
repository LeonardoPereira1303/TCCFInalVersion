using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    [SerializeField] private GameObject iconDefault;
    // ícone exibido quando existe um item no counter
    [SerializeField] private GameObject iconWithObject;

    private bool isVisible = false;
    private bool wasOccupied = false;

    private void Start() {
        // inicializa estados
        SetAllIconsActive(false);
        wasOccupied = baseCounter != null && baseCounter.HasKitchenObject();
        //Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }

    private void Update() {
        if (!isVisible || baseCounter == null) return;

        bool isOccupied = baseCounter.HasKitchenObject();
        if (isOccupied != wasOccupied) {
            UpdateIcons(isOccupied);
            wasOccupied = isOccupied;
        }
    }

    public void ShowCounterVisual()
    {
        isVisible = true;
        UpdateIcons(baseCounter != null && baseCounter.HasKitchenObject());
        Show();
    }

    public void HideCounterVisual()
    {
        isVisible = false;
        SetAllIconsActive(false);
        Hide();
    }

    private void UpdateIcons(bool occupied){
        if (occupied){
            if (iconDefault != null) iconDefault.SetActive(false);
            if (iconWithObject != null) iconWithObject.SetActive(true);
        } else {
            if (iconDefault != null) iconDefault.SetActive(true);
            if (iconWithObject != null) iconWithObject.SetActive(false);
        }
    }

    private void SetAllIconsActive(bool active){
        if (iconDefault != null) iconDefault.SetActive(active);
        if (iconWithObject != null) iconWithObject.SetActive(active);
    }

    //private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e){
    //    if (e.selectedCounter == baseCounter){
    //        Show();
    //    }
    //    else{
    //        Hide();
    //    }
    //}

    private void Show(){
        foreach (GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(true);
        }
    }

    private void Hide(){
        foreach (GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(false);
        }
    }
}
