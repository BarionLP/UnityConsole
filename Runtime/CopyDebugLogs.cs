using UnityEngine;

namespace Ametrin.Console{
    [RequireComponent(typeof(ConsoleManager))]
    public sealed class CopyDebugLogs : MonoBehaviour{
        [SerializeField] private LogType LogLevel = LogType.Warning;

        private void OnEnable(){
            Application.logMessageReceived += OnUnityMessageLogged;
        }

        private void OnUnityMessageLogged(string message, string stack, LogType logType){
            if (!ShouldLog(logType)) return;

            switch (logType){
                case LogType.Warning:
                    ConsoleManager.AddWarningMessage(message);
                    break;
                case LogType.Assert:
                case LogType.Error:
                    ConsoleManager.AddErrorMessage(message);
                    break;
                case LogType.Exception:
                    ConsoleManager.AddExceptionMessage(message);
                    break;

                default:
                    ConsoleManager.AddMessage(message);
                    break;
            }
        }

        bool ShouldLog(LogType type){
            return type switch{
                LogType.Exception => true,
                LogType.Error => LogLevel is not LogType.Exception,
                LogType.Assert => LogLevel is not LogType.Exception and not LogType.Error,
                LogType.Warning => LogLevel is LogType.Warning or LogType.Log,
                LogType.Log => LogLevel is LogType.Log,
                _ => false,
            };
        }

        private void OnDisable(){
            Application.logMessageReceived -= OnUnityMessageLogged;
        }
    }
}
