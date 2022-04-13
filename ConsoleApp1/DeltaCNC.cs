using CNCNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp1
{
    internal class DeltaCNC
    {
        private CNCInfoClass delta;
        private bool ConnectionStatus = false;
        public DeltaCNC(string machineIP = "192.168.10.201")
        {
            var localIP = GetLocalIPAddress();

            delta = new CNCInfoClass();
            delta.SetConnectInfo(localIP, machineIP, 5000);
            ConnectionStatus = (delta.Connect() == 0) ? true : false;
            if (ConnectionStatus)
                Console.WriteLine($"[ INFO ] {machineIP} Delta CNC connected ... [ OK ]");
            else
                Console.WriteLine($"[ FAIL ] {machineIP} Delta CNC connection unreachable ... [ ERROR ]");
        }

        public void Close()
        {
            Console.WriteLine("[ EXIT ] delta cnc exit ... done");
            delta.Disconnect();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public JObject Get()
        {
            var returnJson = new JObject();
            if (!ConnectionStatus)
                return returnJson;

            GetWorkTime(ref returnJson);
            GetFeedSpindle(ref returnJson);
            GetSpindleCurrent(ref returnJson);
            GetCNCFlag(ref returnJson);
            GetNCode(ref returnJson);
            GetNCPointer(ref returnJson);
            GetPosition(ref returnJson);
            GetServoLoad(ref returnJson);

            return returnJson;
        }

        private void GetFeedSpindle(ref JObject data)
        {
            delta.READ_feed_spindle(0, 0, out double OvFeed, out uint OvSpindle, out double ActFeed, out uint ActSpindle);
            data.Add("feedSpindleOvFeed", OvFeed.ToString());
            data.Add("feedSpindleOvSpindle", OvSpindle.ToString());
            data.Add("feedSpindleActFeed", ActFeed.ToString());
            data.Add("feedSpindleActSpindle", ActSpindle.ToString());
        }

        private void GetServoLoad(ref JObject data)
        {
            delta.READ_servo_load(0, out ushort AxisCount, out ushort[] AxisNr, out bool[] Result, out int[] AxisValue);
            data.Add("servoLoadAxisCount", (int)AxisCount);
            data.Add("servoLoadAxisNr", String.Join("|", AxisNr));
            data.Add("servoLoadAxisValue", String.Join("|", AxisValue));
        }

        private void GetSpindleCurrent(ref JObject data)
        {
            delta.READ_servo_load(0, out ushort AxisCount, out ushort[] AxisNr, out bool[] Result, out int[] AxisValue);
            data.Add("spindleCurrentAxisCount", (int)AxisCount);
            data.Add("spindleCurrentAxisNr", String.Join("|", AxisNr));
            data.Add("spindleCurrentAxisValue", String.Join("|", AxisValue));
        }

        private void GetCNCFlag(ref JObject data)
        {
            delta.READ_CNCFlag(out bool WorkingFlag, out bool AlarmFlag);
            data.Add("workingFlag", WorkingFlag.ToString());
            data.Add("alarmFlag", AlarmFlag.ToString());
        }

        private void GetNCode(ref JObject data)
        {
            delta.READ_current_code(1, out uint[] LineNo, out string[] Content);
            data.Add("nCodeLineNo", (LineNo == null) ? -1 : (int)LineNo[0]);
            data.Add("nCodeContent", (Content == null) ? "empty" : Content[0]);
        }

        private void GetNCPointer(ref JObject data)
        {
            delta.READ_nc_pointer(out int LineNum, out int MDILineNum);
            data.Add("nCPointerLineNum", LineNum);
            data.Add("nCPointerMDILineNum", MDILineNum);
        }

        private void GetPosition(ref JObject data)
        {
            delta.READ_POSITION(0, 0, out string[] AxisName, out double[] CoorMach, out double[] CoorAbs, out double[] CoorRel, out double[] CoorRes, out double[] CoorOffset);
            data.Add("positionAxisName", String.Join("|", AxisName));
            data.Add("positionCoorMach", String.Join("|", CoorMach));
            data.Add("positionCoorAbs", String.Join("|", CoorAbs));
            data.Add("positionCoorRel", String.Join("|", CoorRel));
            data.Add("positionCoorRes", String.Join("|", CoorRes));
            data.Add("positionCoorOffset", String.Join("|", CoorOffset));
        }

        private void GetWorkTime(ref JObject data)
        {
            delta.READ_processtime(out uint TotalWorkTime, out uint SingleWorkTime);
            data.Add("totalWorkTime", (int)TotalWorkTime);
            data.Add("singleWorkTime", (int)SingleWorkTime);
        }
    }
}
