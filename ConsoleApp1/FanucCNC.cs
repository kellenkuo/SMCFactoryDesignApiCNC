using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class FanucCNC
    {
        private bool ConnectionStatus = false;
        public ushort FlibHndl;
        public FanucCNC(string machineIP = "192.168.10.12")
        {
            var ret = Focas1.cnc_allclibhndl3(machineIP, 8193, 1, out FlibHndl);
            ConnectionStatus = (ret == Focas1.EW_OK) ? true : false;
            if (ConnectionStatus)
                Console.WriteLine($"[ INFO ] {machineIP} Fanuc CNC connected ... [ OK ]");
            else
                Console.WriteLine($"[ FAIL ] {machineIP} Fanuc CNC connection unreachable ... [ ERROR ]");
        }

        public JObject Get()
        {
            var returnJson = new JObject();
            if (!ConnectionStatus)
                return returnJson;

            GetFeedRate(ref returnJson);
            GetSpindleSpeed(ref returnJson);
            GetNCode(ref returnJson);
            GetAbsoluteCoords(ref returnJson);
            GetMachineCoords(ref returnJson);
            GetRelativeCoords(ref returnJson);
            GetDistance(ref returnJson);
            GetRDZOffset(ref returnJson);

            return returnJson;
        }

        private void GetFeedRate(ref JObject data)
        {
            Focas1.ODBACT cache = new Focas1.ODBACT();
            var ret = Focas1.cnc_actf(FlibHndl, cache);
            if (ret == Focas1.EW_OK)
                data.Add("feedRate", cache.data);
            else
                data.Add("feedRate", "None");
        }

        private void GetSpindleSpeed(ref JObject data)
        {
            Focas1.ODBACT cache = new Focas1.ODBACT();
            var ret = Focas1.cnc_acts(FlibHndl, cache);
            if (ret == Focas1.EW_OK)
                data.Add("spindleSpeed", cache.data);
            else
                data.Add("spindleSpeed", "None");
        }

        private void GetNCode(ref JObject data)
        {
            ushort length = 450;
            char[] lineData = new char[450];
            var ret = Focas1.cnc_rdexecprog(FlibHndl, ref length, out short blkNum, lineData);
            if (ret == Focas1.EW_OK)
            {
                string NCode = "";
                for (int i = 0; i < length; i++)
                {
                    var ncOutput = string.Concat(lineData[i]);
                    NCode += ncOutput;
                }
                data.Add("NCcodeBlock", NCode);
            }
            else
                data.Add("NCcodeBlock", "None");
        }

        private void GetAbsoluteCoords(ref JObject data)
        {
            Focas1.ODBAXIS cache = new Focas1.ODBAXIS();
            var ret = Focas1.cnc_absolute(FlibHndl, -1, 4 + 4 * Focas1.MAX_AXIS, cache);
            data.Add("absoluteCoordX", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[0]) / 1000, 3) : -1.0);
            data.Add("absoluteCoordY", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[1]) / 1000, 3) : -1.0);
            data.Add("absoluteCoordZ", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[2]) / 1000, 3) : -1.0);
        }

        private void GetMachineCoords(ref JObject data)
        {
            Focas1.ODBAXIS cache = new Focas1.ODBAXIS();
            var ret = Focas1.cnc_absolute(FlibHndl, -1, 4 + 4 * Focas1.MAX_AXIS, cache);
            data.Add("machineCoordX", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[0]) / 1000, 3) : -1.0);
            data.Add("machineCoordY", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[1]) / 1000, 3) : -1.0);
            data.Add("machineCoordZ", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[2]) / 1000, 3) : -1.0);
        }

        private void GetRelativeCoords(ref JObject data)
        {
            Focas1.ODBAXIS cache = new Focas1.ODBAXIS();
            var ret = Focas1.cnc_relative(FlibHndl, -1, 4 + 4 * Focas1.MAX_AXIS, cache);
            data.Add("relativeCoordX", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[0]) / 1000, 3) : -1.0);
            data.Add("relativeCoordY", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[1]) / 1000, 3) : -1.0);
            data.Add("relativeCoordZ", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[2]) / 1000, 3) : -1.0);
        }

        private void GetDistance(ref JObject data)
        {
            Focas1.ODBAXIS cache = new Focas1.ODBAXIS();
            var ret = Focas1.cnc_distance(FlibHndl, -1, 4 + 4 * Focas1.MAX_AXIS, cache);
            data.Add("distanceX", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[0]) / 1000, 3) : -1.0);
            data.Add("distanceY", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[1]) / 1000, 3) : -1.0);
            data.Add("distanceZ", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[2]) / 1000, 3) : -1.0);
        }

        private void GetRDZOffset(ref JObject data)
        {
            Focas1.IODBZOFS cache = new Focas1.IODBZOFS();
            var ret = Focas1.cnc_rdzofs(FlibHndl, -1, 4, 4 + 4 * Focas1.MAX_AXIS, cache);
            data.Add("rdzOffsetX", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[0]) / 1000, 3) : -1.0);
            data.Add("rdzOffsetY", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[1]) / 1000, 3) : -1.0);
            data.Add("rdzOffsetZ", (ret == Focas1.EW_OK) ? Math.Round(Convert.ToDouble(cache.data[2]) / 1000, 3) : -1.0);
        }
    }
}
