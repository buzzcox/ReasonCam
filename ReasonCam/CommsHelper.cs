using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using SAClientWRC;

namespace ReasonCam
{
  //  public delegate void MethodDel(object sender);


    static class CommsHelper
    {
        static public SAClient saclient = null;
        static public int CurrentAppID = 1004;
        static public bool IsConnected = false;
        static public int CurrentStructureId;

        public delegate void commsReadyOnline(object sender);
        static public event commsReadyOnline commsReady;

        public delegate void snapTakeAction(object sender);
        static public event snapTakeAction snapTake;

        public delegate void goHomeAction(object sender);
        static public event goHomeAction goHome;

        public delegate void stopReturnAction(object sender);
        static public event stopReturnAction stopReturn;


        static public void Initialize()
        {
        //    Messenger.Default.Register<string>(this, NotificationId, m => Console.WriteLine("hello world with context: " + m.Context));

            saclient = new SAClientWRC.SAClient(CurrentAppID);
            saclient.InitShareCharmHandler();

            saclient.ConnectionStatusEvent += SA_ConnectionStatusEvent;
            saclient.DataArrivedEvent += SA_DataArrivedEvent;
            saclient.BroadcastDataArrivedEvent += SA_BroadcastDataArrivedEvent;
            saclient.JSONBroadcastDataArrivedEvent += SA_JSONBroadcastDataArrivedEvent;
            saclient.ExceptionEvent += SA_ExceptionEvent;
            saclient.DeviceMapUpdatedEvent += SA_DeviceMapUpdatedEvent;
            saclient.JSONDeviceMapUpdatedEvent += SA_JSONDeviceMapUpdatedEvent;

            saclient.InitialiseClient();
        }

        static public void deconstruct()
        {

            saclient.ConnectionStatusEvent -= SA_ConnectionStatusEvent;
            saclient.DataArrivedEvent -= SA_DataArrivedEvent;
            saclient.BroadcastDataArrivedEvent -= SA_BroadcastDataArrivedEvent;
            saclient.JSONBroadcastDataArrivedEvent -= SA_JSONBroadcastDataArrivedEvent;
            saclient.ExceptionEvent -= SA_ExceptionEvent;
            saclient.DeviceMapUpdatedEvent -= SA_DeviceMapUpdatedEvent;
            saclient.JSONDeviceMapUpdatedEvent -= SA_JSONDeviceMapUpdatedEvent;

            if (CommsHelper.saclient != null)
            {
                CommsHelper.saclient.Dispose();
                CommsHelper.saclient = null;
            }

            IsConnected = false;
            CurrentStructureId = 0;
        }


        #region SA Client Events

        static void SA_JSONDeviceMapUpdatedEvent(object sender, string e)
        {
            //int a = 0;
        //    addStatus("json device map updated event");
            Debug.WriteLine("json device map: " + e);
        }

        static void SA_DeviceMapUpdatedEvent(object sender, DeviceMap e)
        {
            Debug.WriteLine("device map updated event");

            //this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            ////Clean up existing
            //this.txtNW.Text = string.Empty;
            //this.txtNE.Text = string.Empty;
            //this.txtSW.Text = string.Empty;
            //this.txtSE.Text = string.Empty;
            //this.txtE.Text = string.Empty;
            //    this.txtW.Text = string.Empty;

            //    //Update the Device Map Display
            //    for (int i = 0; i < e.NW.Count; i++)
            //    {
            //        this.txtNW.Text += e.NW[i];
            //        if ((e.NW.Count > 1) && (i >= 0)) this.txtNW.Text += " ";
            //    }
            //    for (int i = 0; i < e.NE.Count; i++)
            //    {
            //        this.txtNE.Text += e.NE[i];
            //        if ((e.NE.Count > 1) && (i >= 0)) this.txtNE.Text += " ";
            //    }

            //    for (int i = 0; i < e.E.Count; i++)
            //    {
            //        this.txtE.Text += e.E[i];
            //        if ((e.E.Count > 1) && (i >= 0)) this.txtE.Text += " ";
            //    }
            //    for (int i = 0; i < e.SW.Count; i++)
            //    {
            //        this.txtSW.Text += e.SW[i];
            //        if ((e.SW.Count > 1) && (i >= 0)) this.txtSW.Text += " ";
            //    }
            //    for (int i = 0; i < e.SE.Count; i++)
            //    {
            //        this.txtSE.Text += e.SE[i];
            //        if ((e.SE.Count > 1) && (i >= 0)) this.txtSE.Text += " ";
            //    }

            //    for (int i = 0; i < e.W.Count; i++)
            //    {
            //        this.txtW.Text += e.W[i];
            //        if ((e.W.Count > 1) && (i >= 0)) this.txtW.Text += " ";
            //    }
            //});
        }

        static void SA_ExceptionEvent(object sender, string e)
        {
            Debug.WriteLine("device map updated event");
            //this.AddEventListItem(e, ItemState.ERROR);
        }

        static void SA_ConnectionStatusEvent(object sender, SAClientWRC.Status e)
        {
            Debug.WriteLine("Client status event: " + (e.IsConnected ? "connected" : "not connected"));

            //    this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //    {

            if (saclient == null) return;

            IsConnected = (e.IsConnected == true) ? true : false;

            if (e.IsReadyToConnect == true)
            {
                Debug.WriteLine("Device id: " + e.CurrentDeviceID + " is ready to connect");
                CurrentStructureId = e.CurrentStructureID;
                saclient.Connect();
            }
            else
                Debug.WriteLine("no configuration found");


            if (IsConnected == true)
            {
                commsReady(null);
                Debug.WriteLine("CONNECTED to server!");
                saclient.StartListening();
            }
            else
                Debug.WriteLine("Connection CLOSED!");


            if (e.IsListening == true)
                Debug.WriteLine("Listening for broadcast");


            if (e.MessageSent == true)
                Debug.WriteLine("Message sent!");

            if (e.StatusException != "")
                Debug.WriteLine("Exception occured!: " + e.StatusException);

            //   });
        }

        static void SA_JSONBroadcastDataArrivedEvent(object sender, string e)
        {
            Debug.WriteLine("device map updated event");
            //int a = 0;
        }

        static void SA_DataArrivedEvent(object sender, SocketData e)
        {
            Debug.WriteLine("data arrived for me");

            //try
            //{

            //    Debug.WriteLine("deserialize object: " + recievedData.ToString());
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("error deserializing: " + ex.Message);
            //}


            //if (e.BroadcastPacket == false)
            //    this.AddDirectEventListItem(e);
            //else
            //    this.AddEventListItem("Data Arrived Event", ItemState.NOTIFICATION);
        }

        static void SA_BroadcastDataArrivedEvent(object sender, SocketData e)
        {
            Debug.WriteLine("Broadcast data arrived");
          //  Debug.WriteLine(String.Format("Message received: {0}", e.Message));
         //   Debug.WriteLine(String.Format("Data received: {0}", e.Data));

            try
            {
             //   SocketData sd = SAHelper.DeserializeToSocketData(e.Message);

                Debug.WriteLine(String.Format("Message received: {0}", e.Message));
                Debug.WriteLine(String.Format("Data received: {0}", e.Data));

                TransMessage tm = (TransMessage)SAHelper.ByteArrayObjToObject(e.Data, typeof(TransMessage));

                if (tm != null)
                {
                    if (tm.StructureId == CurrentStructureId)
                    {
                        Debug.WriteLine("RECEIVED MESSAGE - " + tm.ToString());
                        processMessage(tm);
                    }
                    else
                    {
                        Debug.WriteLine("Received message but not for this structure");
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR deserializing: " + ex.Message);
            }

        }

        #endregion

        #region comms helper methods

        static public void sendMessage(CommandMessage command, Object data)
        {
            if (!IsConnected) return;
            TransMessage tm = new TransMessage(command, data, CurrentStructureId);
            Debug.WriteLine("SENDING MESSAGE - " + tm.ToString());
            saclient.SendObject(tm, false);
        }

        static void processMessage(TransMessage tm)
        {
            switch (tm.Command)
            {
                case (CommandMessage.StopReturn):
                    stopReturn(null);
                    break;
                case (CommandMessage.StartSnap):
                    snapTake(null);
                    break;
                case (CommandMessage.GoHome):
                    goHome(null);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
