using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuView : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    public InputActionReference buttonAction;

    private void OnEnable()
    {
        buttonAction.action.Enable();
        buttonAction.action.performed += OnButtonBPressed;
    }

    private void OnDisable()
    {
        buttonAction.action.performed -= OnButtonBPressed;
        buttonAction.action.Disable();
    }
    
    private void OnButtonBPressed(InputAction.CallbackContext context)
    {
        if (menuPanel != null) {
            bool isActive = menuPanel.activeSelf;
            if (isActive) {
                HideMenu();
            }
            else {
                ShowMenu();
            }
        }
    }


    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            
            Vector3 headPosition = Camera.main.transform.position;
            Vector3 forward = Camera.main.transform.forward;

            // Смещение панели: вперед и немного ниже головы
            float distance = 0.7f;   // расстояние от головы
            float verticalOffset = -0.15f; // смещение вниз (меньше 0.2 для комфортного взгляда)

            Vector3 targetPosition = headPosition + forward.normalized * distance;
            targetPosition.y += verticalOffset; // опускаем панель ниже головы

                // Устанавливаем позицию панели
            menuPanel.transform.position = targetPosition;

            // Смотрим на камеру, сохраняя горизонтальный уровень
            Vector3 lookDirection = menuPanel.transform.position - headPosition;
            lookDirection.y = 0; // убираем наклон вверх/вниз
            menuPanel.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void Update() {
        if (menuPanel != null && menuPanel.activeSelf) {
            if (Vector3.Distance(menuPanel.transform.position, Camera.main.transform.position) > 3f){
                HideMenu();
            }
        }
    }

    public void HideMenu() {
        if (menuPanel != null) {
            menuPanel.SetActive(false);
        }
    }

    public void Load360Scene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("360VideoScene");
    }
    
    public void LoadDesertScene() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DesertScene");
    }
    
    public void Mute(bool mute) {
        AudioListener.volume = mute ? 1f : 0f;
    }
}
