using EZSOCKETNCLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class MitsubishiCNC
    {
        private EZNcCommunication301Class mitsubishi;
        private bool ConnectionStatus = false;
        // cnc data reference manual
        private int lAxisNo = 1;
        private int lType = 2;
        private int lIndex = 2;
        private int lSpindle = 1;
        public MitsubishiCNC(string machineIP = "192.168.10.13")
        {
            mitsubishi = new EZNcCommunication301Class();
            var ret = mitsubishi.SetTCPIPProtocol(machineIP, 683);
            if (ret == 0)
            {
                ret = mitsubishi.Open2(9, 1, 1000);
                if (ret == 0)
                    ConnectionStatus = true;
            }
            if (ConnectionStatus)
                Console.WriteLine($"[ INFO ] {machineIP} Mitsubishi CNC connected ... [ OK ]");
            else
                Console.WriteLine($"[ FAIL ] {machineIP} Mitsubishi CNC connection unreachable ... [ ERROR ]");
        }

        public JObject Get()
        {
            var returnJson = new JObject();
            if (!ConnectionStatus)
                return returnJson;

            GetFeedRate(ref returnJson);
            GetSpindleSpeed(ref returnJson);
            GetCurrentBlock(ref returnJson);
            GetToolCommand(ref returnJson);
            GetStartTime(ref returnJson);
            GetEstimateTime(ref returnJson);
            GetCurrentCoords(ref returnJson);
            GetMachineCoords(ref returnJson);
            GetWorkingCoords(ref returnJson);
            GetProgramCoords(ref returnJson);
            GetDistance(ref returnJson);
            GetServo(ref returnJson);

            return returnJson;
        }

        private void GetFeedRate(ref JObject data)
        {
            var ret = mitsubishi.GetFeedSpeed(3, out double pdSpeed);
            if (ret == 0)
                data.Add("feedRate", pdSpeed.ToString("f3"));
            else
                data.Add("feedRate", "None");
        }

        private void GetSpindleSpeed(ref JObject data)
        {
            var ret = mitsubishi.GetSpindleMonitor(lIndex, lSpindle, out int plData, out string lppwszBuffer);
            if (ret == 0)
                data.Add("spindleSpeed", plData);
            else
                data.Add("spindleSpeed", "None");
        }

        private void GetCurrentBlock(ref JObject data)
        {
            var ret = mitsubishi.CurrentBlockRead(2, out string lppwszProgramData, out int plCurrentBlockNo);
            if (ret == 0)
                data.Add("currentBlock", lppwszProgramData);
            else
                data.Add("currentBlock", "None");
        }

        private void GetToolCommand(ref JObject data)
        {
            var ret = mitsubishi.GetToolCommand(lAxisNo, lType, out int plValue);
            if (ret == 0)
                data.Add("toolCommand", plValue);
            else
                data.Add("toolCommand", "None");
        }

        private void GetStartTime(ref JObject data)
        {
            var ret = mitsubishi.GetStartTime(out int plTime);
            if (ret == 0)
            {
                int sec = plTime % 100;
                int min = (plTime / 100) % 100;
                int hour = (plTime / 10000) % 1000;
                data.Add("startTime", $"{hour}:{min}:{sec}");
            }
            else
                data.Add("startTime", "None");
        }

        private void GetEstimateTime(ref JObject data)
        {
            var ret = mitsubishi.GetEstimateTime(0, out int plTime);
            if (ret == 0)
            {
                int sec = plTime % 100;
                int min = (plTime / 100) % 100;
                int hour = (plTime / 10000) % 1000;
                data.Add("estimateTime", $"{hour}:{min}:{sec}");
            }
            else
                data.Add("estimateTime", "None");
        }

        private void GetCurrentCoords(ref JObject data)
        {
            double coordValue;
            data.Add("currentCoordX", (mitsubishi.GetCurrentPosition(1, out coordValue) == 0) ? coordValue : -1.0);
            data.Add("currentCoordY", (mitsubishi.GetCurrentPosition(2, out coordValue) == 0) ? coordValue : -1.0);
            data.Add("currentCoordZ", (mitsubishi.GetCurrentPosition(3, out coordValue) == 0) ? coordValue : -1.0);
        }

        private void GetMachineCoords(ref JObject data)
        {
            double coordValue;
            data.Add("machineCoordX", (mitsubishi.GetMachinePosition(1, out coordValue, 0) == 0) ? coordValue : -1.0);
            data.Add("machineCoordY", (mitsubishi.GetMachinePosition(2, out coordValue, 0) == 0) ? coordValue : -1.0);
            data.Add("machineCoordZ", (mitsubishi.GetMachinePosition(3, out coordValue, 0) == 0) ? coordValue : -1.0);
        }

        private void GetWorkingCoords(ref JObject data)
        {
            double coordValue;
            data.Add("workingCoordX", (mitsubishi.GetWorkPosition(1, out coordValue, 0) == 0) ? coordValue : -1.0);
            data.Add("workingCoordY", (mitsubishi.GetWorkPosition(2, out coordValue, 0) == 0) ? coordValue : -1.0);
            data.Add("workingCoordZ", (mitsubishi.GetWorkPosition(3, out coordValue, 0) == 0) ? coordValue : -1.0);
        }

        private void GetProgramCoords(ref JObject data)
        {
            double coordValue;
            data.Add("programCoordX", (mitsubishi.GetProgramPosition(1, out coordValue) == 0) ? coordValue : -1.0);
            data.Add("programCoordY", (mitsubishi.GetProgramPosition(2, out coordValue) == 0) ? coordValue : -1.0);
            data.Add("programCoordZ", (mitsubishi.GetProgramPosition(3, out coordValue) == 0) ? coordValue : -1.0);
        }

        private void GetDistance(ref JObject data)
        {
            double coordValue;
            data.Add("distanceX", (mitsubishi.GetDistance(1, out coordValue, 0) == 0) ? coordValue : -1.0);
            data.Add("distanceY", (mitsubishi.GetDistance(2, out coordValue, 0) == 0) ? coordValue : -1.0);
            data.Add("distanceZ", (mitsubishi.GetDistance(3, out coordValue, 0) == 0) ? coordValue : -1.0);
        }

        private void GetServo(ref JObject data)
        {
            var ret = mitsubishi.GetServoMonitor(lAxisNo, lIndex, out int plData, out string lppwszBuffer);
            data.Add("servoBuffer", (ret == 0) ? plData : -1);
            data.Add("servoData", (ret == 0) ? lppwszBuffer : "None");
        }
    }
}
