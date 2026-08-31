using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BD.Standard.DD.BillChangePlugIns
{
    public class Logs
    {
        public static void Log(String message, string logpath, string type, String json)
        {
            StreamWriter streamWriter = new StreamWriter(logpath + type + DateTime.Now.ToString("yyyMMdd") + "log.txt", true);
            streamWriter.Write("消息：" + message + "\r\n" +
                "操作时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss\r\n" +
                "详情信息：" + json + "\r\n" +
                "--------------------------分割线--------------------------\r\n"));
            streamWriter.Close();
        }
    }
}
