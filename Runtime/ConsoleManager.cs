using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ametrin.Console
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class ConsoleManager : MonoBehaviour
    {
        public static event Action OnShow;
        public static event Action OnHide;
        public static bool IsVisible => _consoleElement.style.visibility.value == Visibility.Visible;
        private static ConsoleManager _Instance;
        public static ConsoleManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    if (FindAnyObjectByType<ConsoleManager>(FindObjectsInactive.Exclude) is not ConsoleManager instance) throw new Exception("No active Console found!");
                    _Instance = instance;
                }

                return _Instance;
            }
        }

        private static UIDocument _document;
        private static VisualElement _consoleElement;
        private static TextField _inputElement;
        private static Label _syntaxHintLabel;
        private static Label _messageDisplayElement;
        private static IConsoleHandler _defaultHandler = new ConsoleMessageHandler(AddMessage, false);
        private readonly static Dictionary<char, IConsoleHandler> _handlers = new();
        private readonly static List<string> _messages = new();

        private void Awake()
        {
            if (_Instance != null && _Instance != this)
            {
                Debug.LogError("Created duplicate console manager");
                DestroyImmediate(gameObject);
                return;
            }
            _document = GetComponent<UIDocument>();
            _consoleElement = _document.rootVisualElement;

            _inputElement = _consoleElement.Query<TextField>();
            _inputElement.RegisterValueChangedCallback(OnInputChanged);
            _inputElement.RegisterCallback<KeyUpEvent>(HandleKeys);

            _messageDisplayElement = _consoleElement.Query<Label>("output");
            _syntaxHintLabel = _consoleElement.Query<Label>("syntax");
            _syntaxHintLabel.style.display = DisplayStyle.None;
        }

        private void Start()
        {
            Hide();
            Clear();
        }

        private void OnInputChanged(ChangeEvent<string> changeEvent)
        {
            var input = ReadInput();
            if (input.IsEmpty)
            {
                _syntaxHintLabel.style.display = DisplayStyle.None;
                return;
            }
            var handler = GetHandler(input);
            if (!handler.PassPrefix && handler != _defaultHandler)
            {
                input = input[1..];
            }
            UpdateHint(input, handler);
        }
        private static void HandleKeys(KeyUpEvent upEvent)
        {
            var input = ReadInput();
            if (input.IsEmpty) return;
            var handler = GetHandler(input);
            char prefix;
            if (handler.PassPrefix || handler == _defaultHandler)
            {
                prefix = '\0';
            }
            else
            {
                prefix = input[0];
                input = input[1..];
            }

            if (upEvent.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                HandleInput(input, handler);
                upEvent.StopPropagation();
            }
            if (upEvent.keyCode is KeyCode.Tab)
            {
                AutoComplete(input, handler, prefix);
                upEvent.StopPropagation();
            }
        }

        private static void UpdateHint(ReadOnlySpan<char> input, IConsoleHandler handler)
        {
            var hint = handler.GetHint(input);

            _syntaxHintLabel.style.display = string.IsNullOrWhiteSpace(hint) ? DisplayStyle.None : DisplayStyle.Flex;
            _syntaxHintLabel.text = hint;
        }

        private static void HandleInput(ReadOnlySpan<char> input, IConsoleHandler handler)
        {
            _inputElement.value = string.Empty;
            handler.Handle(input);
            ScheduleFocusInput(0);
        }

        private static void AutoComplete(ReadOnlySpan<char> input, IConsoleHandler handler, char prefix)
        {
            var completion = handler.GetAutoCompleted(input);
            if (string.IsNullOrWhiteSpace(completion)) return;
            _inputElement.value = prefix == '\0' || handler.PassPrefix ? completion : prefix + completion;
            ScheduleFocusInput();
        }

        private static IConsoleHandler GetHandler(ReadOnlySpan<char> input)
        {
            if (!input.IsEmpty && _handlers.TryGetValue(input[0], out var handler)) return handler;

            return _defaultHandler;
        }

        public static void Hide()
        {
            _consoleElement.style.display = DisplayStyle.None;
            OnHide?.Invoke();
        }
        public static void Show()
        {
            UpdateView();
            _consoleElement.style.display = DisplayStyle.Flex;
            ScheduleFocusInput();
            OnShow?.Invoke();
        }

        private static ReadOnlySpan<char> ReadInput()
        {
            if (string.IsNullOrWhiteSpace(_inputElement.value)) return ReadOnlySpan<char>.Empty;
            return _inputElement.value.AsSpan();
        }
        private static void UpdateView()
        {
            _messageDisplayElement.text = string.Join("\n", _messages);
        }

        private static void ScheduleFocusInput(int idx = -1)
        {
            Instance.StartCoroutine(Impl());
            if (idx == -1) idx = _inputElement.value.Length;
            _inputElement.SelectRange(idx, idx);

#if UNITY_2023_1_OR_NEWER
            static async Awaitable Impl()
            {
                try
                {
                    await Awaitable.EndOfFrameAsync();
                    _inputElement.Focus();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
#else
            static async IEnumerator Impl(){
                yield return new WaitForEndOfFrame();
                _inputElement.Focus();
            }
#endif
        }

        public static void Clear()
        {
            _messages.Clear();
        }
        public static void AddMessage(string message)
        {
            _messages.Add(message);
            if (IsVisible) UpdateView();
        }
        public static void AddWarningMessage(string message)
        {
            AddMessage($"<color=yellow>{message}</color>");
        }
        public static void AddErrorMessage(string message)
        {
            AddMessage($"<color=red>{message}</color>");
        }
        public static void AddExceptionMessage(string message)
        {
            AddMessage($"<color=#cc0000ff>{message}</color>");
        }

        public static void RegisterHandler<T>(char prefix) where T : IConsoleHandler, new() => RegisterHandler(prefix, new T());
        public static void RegisterHandler(char prefix, IConsoleHandler handler)
        {
            if (_handlers.ContainsKey(prefix))
            {
                throw new ArgumentException($"console handler prefix '{prefix}' already exists", nameof(prefix));
            }
            _handlers.Add(prefix, handler);
        }

        public static void OverrideDefaultHandler(IConsoleHandler handler)
        {
            _defaultHandler = handler;
        }
    }

#nullable enable
    public sealed class ConsoleMessageHandler : IConsoleHandler
    {
        public bool PassPrefix { get; set; }
        private readonly Action<string> OnExecute;
        public ConsoleMessageHandler(Action<string> execute, bool passPrefix = false)
        {
            OnExecute = execute;
            PassPrefix = passPrefix;
        }

        public void Handle(ReadOnlySpan<char> input) => OnExecute(input.ToString());
    }

    public interface IConsoleHandler
    {
        public bool PassPrefix { get; }
        public void Handle(ReadOnlySpan<char> input);
        public string GetHint(ReadOnlySpan<char> input) => string.Empty;
        public string GetAutoCompleted(ReadOnlySpan<char> input) => string.Empty;
    }
}
