using System.IO;
using System.Text;
using UnityEngine;

namespace SeerGoGame.Utilities
{
    //  public class SeerGoLogger : MonoBehaviour
    /// <summary>
    /// 封装Debug日志打印，实现日志开关控制、彩色日志打印、日志文件存储等功能
    /// </summary>
    public class SeerGoLogger
    {
        // 普通日志开关
        private static bool debugLogEnable = true;
        // 警告日志开关
        private static bool warningLogEnable = true;
        // 错误日志开关
        private static bool errorLogEnable = true;

        // 使用StringBuilder来优化字符串的重复构造
        private static StringBuilder _logStr = new StringBuilder();
        // 日志储存位置
        public static string _logFilePath;

        /// <summary>
        /// 初始化，在App启动的入口脚本Awake中调用
        /// </summary>
        public static void Init()
        {
            var t = System.DateTime.Now.ToString("yyyyMMddhhmmss");
            //_logFilePath = string.Format("{0}/output_{1}.log", Application.persistentDataPath, t);
            //Application.logMessageReceived += OnLogCallBack;
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="condition"> 日志文本 </param>
        /// <param name="stackTrace"> 调用堆栈 </param>
        /// <param name="type"> 日志类型 </param>
        private static void OnLogCallBack(string condition, string stackTrace, LogType type)
        {
            _logStr.Append(condition);
            _logStr.Append("\n");
            _logStr.Append(stackTrace);
            _logStr.Append("\n");

            if (_logStr.Length <= 0)
                return;

            if (!File.Exists(_logFilePath))
            {
                var fs = File.Create(_logFilePath);
                fs.Close();
            }

            using (var sw = File.AppendText(_logFilePath))
            {
                sw.WriteLine(_logStr.ToString());
            }

            _logStr.Remove(0, _logStr.Length);
        }

        /// <summary>
        /// 上传日志到服务端
        /// </summary>
        /// <param name="desc"></param>
        public static void UpLoadLog(string desc)
        {

        }

        /// <summary>
        /// 格式化打印日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        /// <summary>
        /// 普通日志打印
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void Log(object message, Object context = null)
        {
            if (!debugLogEnable) return;

            Debug.Log(message, context);
        }

        /// <summary>
        /// 警告日志打印
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void LogWarning(object message, Object context = null)
        {
            if (!warningLogEnable) return;

            Debug.LogWarning(message, context);
        }

        /// <summary>
        /// 错误日志打印
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void LogError(object message, Object context = null)
        {
            if (!errorLogEnable) return;

            Debug.LogError(message, context);
        }

        /// <summary>
        /// 带颜色的普通日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <param name="context"></param>
        public static void LogWithColor(string message, string color, Object context = null)
        {
            Debug.Log(FmtColor(color, message), context);
        }

        /// <summary>
        /// 格式化带颜色的日志
        /// </summary>
        /// <param name="color"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static object FmtColor(string color, string msg)
        {
#if !UNITY_EDITOR
            return msg;
#else
            int p = msg.IndexOf('\n');
            if (p >= 0) p = msg.IndexOf('\n', p + 1);// 可以同时显示两行
            if (p < 0 || p >= msg.Length - 1) return string.Format("<color={0}>{1}</color>", color, msg);
            if (p > 2 && msg[p - 1] == '\r') p--;
            return string.Format("<color={0}>{1}</color>{2}", color, msg.Substring(0, p), msg.Substring(p));
#endif
        }

        #region 解决日志双击溯源问题
#if UNITY_EDITOR
        [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
        static bool OnOpenAsset(int instanceID, int line)
        {
            string stackTrace = GetStackTrace();
            if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains("XxLogger:Log"))
            {
                //使用正则表达式匹配at的哪一个脚本
                var matches = System.Text.RegularExpressions.Regex.Match(stackTrace, @"\(at (.+)\)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                string pathLine = "";
                while (matches.Success)
                {
                    pathLine = matches.Groups[1].Value;

                    if (!pathLine.Contains("XxLogger.cs"))
                    {
                        int splitIndex = pathLine.LastIndexOf(":");
                        //脚本路径
                        string path = pathLine.Substring(0, splitIndex);
                        //行号
                        line = System.Convert.ToInt32(pathLine.Substring(splitIndex + 1));
                        string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                        fullPath = fullPath + path;
                        //跳转到目标代码特定行
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'),
                            line);
                        break;
                    }
                    matches = matches.NextMatch();
                }
                return true;
            }

            return false;
        }

        static string GetStackTrace()
        {
            // 通过反射获取ConsoleWindow类
            var ConsoleWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            //获取窗口实例
            var fieldInfo = ConsoleWindowType.GetField("ms_ConsoleWindow",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic);
            var consoleInstance = fieldInfo.GetValue(null);
            if (consoleInstance != null)
            {
                if ((object)UnityEditor.EditorWindow.focusedWindow == consoleInstance)
                {
                    //获取成员
                    fieldInfo = ConsoleWindowType.GetField("m_ActiveText",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic);
                    //获取值
                    string activeText = fieldInfo.GetValue(consoleInstance).ToString();
                    return activeText;
                }
            }

            return null;
        }
#endif
        #endregion
    }


}
