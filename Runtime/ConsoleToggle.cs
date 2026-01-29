using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Ametrin.Console
{
    [RequireComponent(typeof(ConsoleManager))]
    public sealed class ConsoleToggle : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [SerializeField] private InputAction Show;
        [SerializeField] private InputAction Hide;

        private void ShowConsole(InputAction.CallbackContext context)
        {
            Show.Disable();
            ConsoleManager.Show();
        }

        private void HideConsole(InputAction.CallbackContext context)
        {
            Hide.Disable();
            ConsoleManager.Hide();
        }

        private void OnEnable()
        {
            Show.canceled += ShowConsole;
            Hide.canceled += HideConsole;
            ConsoleManager.OnShow += Hide.Enable;
            ConsoleManager.OnHide += Show.Enable;
        }

        private void OnDisable()
        {
            Show.canceled -= ShowConsole;
            Hide.canceled -= HideConsole;
            ConsoleManager.OnShow -= Hide.Enable;
            ConsoleManager.OnHide -= Show.Enable;
            Hide.Disable();
            Show.Disable();
        }
#else
        [SerializeField] private KeyCode Show;
        [SerializeField] private KeyCode Hide;

        private void Update()
        {
            if(!ConsoleManager.IsVisible && Input.GetKeyUp(Show))
            {
                ConsoleManager.Show();
            }
            else if(ConsoleManager.IsVisible && Input.GetKeyUp(Hide))
            {
                ConsoleManager.Hide();
            }
        }
#endif
    }
}