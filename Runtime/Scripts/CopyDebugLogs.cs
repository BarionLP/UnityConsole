using UnityEngine;

namespace Ametrin.Console{
    [RequireComponent(typeof(ConsoleManager))]
    public class CopyDebugLogs : MonoBehaviour{
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
            return LogLevel switch{
                LogType.Exception => type is LogType.Exception,
                LogType.Error => type is LogType.Error or LogType.Exception,
                LogType.Warning => type is LogType.Warning or LogType.Error or LogType.Exception,
                LogType.Log => type is not LogType.Assert,
                LogType.Assert => true,
                _ => false,
            };
        }

        private void OnDisable(){
            Application.logMessageReceived -= OnUnityMessageLogged;
        }
    }
}
