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
                LogType.Error => LogLevel is LogType.Error or LogType.Exception,
                LogType.Warning => LogLevel is not LogType.Log or LogType.Assert,
                LogType.Log => LogLevel is not LogType.Assert,
                LogType.Assert => LogLevel is LogType.Assert,
                _ => false,
            };
        }

        private void OnDisable(){
            Application.logMessageReceived -= OnUnityMessageLogged;
        }
    }
}
