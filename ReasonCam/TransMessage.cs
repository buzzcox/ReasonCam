using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;

namespace ReasonCam
{
    public enum CommandMessage
    {
        StartSnap = 0,
        StopReturn = 1,
        GoHome = 2
    }

    [KnownType(typeof(ReasonCam.TransMessage))]
    [DataContractAttribute]
    public class TransMessage
    {
        public TransMessage() { }

        public TransMessage(CommandMessage command, Object data)
        {
            this.Command = command;
            this.Data = data;
        }

        [DataMember]
        public CommandMessage Command { get; set; }

        [DataMember]
        public Object Data { get; set; }

        public override string ToString()
        {
            return String.Format("TransMessage: Command={0}, Data={1}", CommandString(Command), (this.Data==null)?"null":this.Data.ToString());
        }

        public string CommandString(CommandMessage cmd)
        {
            switch (cmd)
            {
                case CommandMessage.StartSnap:
                    return "StartSnap";

                case CommandMessage.GoHome:
                    return "GoHome";

                case CommandMessage.StopReturn:
                    return "StopReturn";

                default:
                    return "null";

            }
        }
    }
}
