using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForecourtSimulator_1
{
    class ForecourtCommunication
    {

        //Client
        static NamedPipeClientStream client;
        static StreamWriter clientWriter;
        static StreamReader clientReader;

        private static ManualResetEvent ConnectedEvent = new ManualResetEvent(false);

        Form1 form; 

        public ForecourtCommunication(Form1 form)
        {
            this.form = form; 
        }

 
        public void CreateClient()
        {
            //Want to wait until Forecourt server has been created until Sim client tries to connect
            bool signalReceived = ConnectedEvent.WaitOne(600000);
            if (signalReceived)
                Console.WriteLine("Forecourt manager connected");
            else
                Console.WriteLine("Forecourt manager did not connect");


            client = new NamedPipeClientStream("PumpSimToForecourt");
            client.Connect(6000);

            clientReader = new StreamReader(client);
            clientWriter = new StreamWriter(client);


            NumberOfPumps();
            SetFuelPrices();

        }

        public static string SendMessage(SimToForecourtDTO dto)
        {

            if (!client.IsConnected)
                return "";

            string data = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            clientWriter.WriteLine(data);
            clientWriter.Flush();
            return clientReader.ReadLine();
        }

        public void StartPipeServer()
        {
            try
            {
                Task serverTask = Task.Factory.StartNew(() =>
                {

                    PipeSecurity ps = new PipeSecurity();
                    System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.BuiltinUsersSid, null);
                    PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                    ps.AddAccessRule(par);
                    var server = new NamedPipeServerStream("ForecourtToPumpSim", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps);
                    Console.WriteLine("Waiting for client connection...");
                    server.WaitForConnection();


                    StreamReader serverReader = new StreamReader(server);
                    StreamWriter serverWriter = new StreamWriter(server);
                    ConnectedEvent.Set();


                    while (true)
                    {
                        string line = serverReader.ReadLine();
                        string response = "";
                        HandleForecourtMessages(line, ref response); 

                        serverWriter.WriteLine(response);
                        serverWriter.Flush();
                    }
                });
            }
            catch (Exception e)
            {

            }
        }

        private void HandleForecourtMessages(string line, ref string response)
        {
            ForecourtToSimDTO message = Newtonsoft.Json.JsonConvert.DeserializeObject<ForecourtToSimDTO>(line);

            switch (message.MsgType)
            {
                case ForecourtToSimMessageType.PumpToIdle:
                    form.StatusAuthorizedToIdle(int.Parse(message.MsgData));
                    break;
                case ForecourtToSimMessageType.EmergencyStop:
                    form.SetEmergencyStop(true);
                    break;
                case ForecourtToSimMessageType.RecallEmergencyStop:
                    form.StatusEmergencyStopToAuthorized(int.Parse(message.MsgData));
                    break;
                case ForecourtToSimMessageType.ClosePump:
                    form.ClosePump(int.Parse(message.MsgData));
                    break;
                case ForecourtToSimMessageType.OpenPump:
                    form.OpenPump(int.Parse(message.MsgData));
                    break;
                case ForecourtToSimMessageType.DisableAddRemoveButtons:
                    form.DisableAddCloseButtons();
                    break;
            }
        }

        public void NumberOfPumps()
        {
            string numberOfPumps = form.pumpFieldCount.ToString();
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.NumberOfPumps,
                MsgData = numberOfPumps
            };

            SendMessage(dto);
        }

        public void SetFuelPrices()
        {
            string prices = form.GetFuelPrices();
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.FuelPrices,
                MsgData = prices
            };

            SendMessage(dto);
        }

        public void PumpToStarting(int pumpID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.PumpToStarting,
                MsgData = pumpID.ToString()
            };

            SendMessage(dto);
        }

        public void PumpToFuelling(int pumpID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.PumpToFuelling,
                MsgData = pumpID.ToString()
            };

            SendMessage(dto);
        }

        public void PumpToPaused(int pumpID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.PumpToPaused,
                MsgData = pumpID.ToString()
            };

            SendMessage(dto);
        }

        public void PumpToAuthorized(int pumpID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.PumpToAuthorized,
                MsgData = pumpID.ToString()
            };

            SendMessage(dto);
        }

        public void PumpToCalling(int pumpID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.PumpToCalling,
                MsgData = pumpID.ToString()
            };

            SendMessage(dto);
        }

        public void SetPumpGrade(int pumpID, int gradeID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.SetPumpGrade,
                MsgData = pumpID.ToString() + "," + gradeID.ToString()
            };

            SendMessage(dto);
        }

        public void FuellingStep(int pumpID, double volume, double amount)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.FuellingStep,
                MsgData = pumpID.ToString() + "," + volume.ToString() + "," + amount.ToString()
            };

            SendMessage(dto);
        }

        public void StopFuelling(int pumpID)
        {
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.StopFuelling,
                MsgData = pumpID.ToString()
            };

            SendMessage(dto);
        }

        public class SimToForecourtDTO
        {
            public SimToForecourtMessageType MsgType;
            public string MsgData;
        }

        public enum SimToForecourtMessageType
        {
            //Need to update this, this is just so that the project will not yield errors
            NumberOfPumps, 
            PumpToStarting,
            PumpToFuelling,
            PumpToPaused,
            PumpToAuthorized,
            PumpToCalling,
            FuelPrices,
            SetPumpGrade,
            FuellingStep,
            StopFuelling
        }

        public class ForecourtToSimDTO
        {
            public ForecourtToSimMessageType MsgType;
            public string MsgData;
        }

        public enum ForecourtToSimMessageType
        {
            PumpToIdle,
            EmergencyStop,
            RecallEmergencyStop,
            ClosePump,
            OpenPump,
            DisableAddRemoveButtons
        }
    }
}
