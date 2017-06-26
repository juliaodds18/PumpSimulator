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

 
        public static void CreateClient()
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

            //While true, get commands and send to forecourt? 

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

        public static void StartPipeServer()
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
                        //HandleForecourtMessages(line, ref response); 

                        serverWriter.WriteLine(response);
                        serverWriter.Flush();
                    }
                });
            }
            catch (Exception e)
            {

            }
        }

        public static void NumberOfPumps()
        {
            //Need to find a way to fetch data from Form1 without causing a null reference exception 
            string numberOfPumps = "3"; 
            SimToForecourtDTO dto = new SimToForecourtDTO
            {
                MsgType = SimToForecourtMessageType.NumberOfPumps,
                MsgData = numberOfPumps
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
            NumberOfPumps
        }
    }
}
