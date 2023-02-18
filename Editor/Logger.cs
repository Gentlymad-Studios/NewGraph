using UnityEngine;

namespace NewGraph {
    public class Logger {

        private static string logHeader = null;
        private static string LogHeader {
            get {
                if (logHeader == null) {
                    logHeader = $"<b><color=#{GraphSettings.LoggerColorHex}>{nameof(NewGraph)}</color></b> ";
                }
                return logHeader;
            }
        }

        [System.Diagnostics.Conditional(GraphSettings.debugDefine)]
        public static void Log(string format, params object[] args) {
            Debug.LogFormat(LogHeader + format, args);
        }

        public static void LogAlways(string format, params object[] args) {
            Debug.LogFormat(LogHeader + format, args);
        }
    }
}
