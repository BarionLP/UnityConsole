#if ENABLE_INPUT_SYSTEM

using Ametrin.Console;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ConsoleToggle : MonoBehaviour{
    [SerializeField] private InputAction Show;
    [SerializeField] private InputAction Hide;

    private void ShowConsole(InputAction.CallbackContext context){
        Show.Disable();
        ConsoleManager.Show();
    }
    
    private void HideConsole(InputAction.CallbackContext context){
        Hide.Disable();
        ConsoleManager.Hide();
    }

    private void OnEnable(){
        Show.canceled += ShowConsole;
        Hide.canceled += HideConsole;
        ConsoleManager.OnShow += Hide.Enable;
        ConsoleManager.OnHide += Show.Enable;
        if(ConsoleManager.IsVisible){
            Hide.Enable();
        }else{
            Show.Enable();
        }   
    }

    private void OnDisable(){
        Hide.Disable();
        Show.Disable();
        Show.canceled -= ShowConsole;
        Hide.canceled -= HideConsole;
        ConsoleManager.OnShow -= Hide.Enable;
        ConsoleManager.OnHide -= Show.Enable;
    }
}
#endif