/*
    This file is part of HomeGenie Project source code.

    HomeGenie is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HomeGenie is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with HomeGenie.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
 * 2014-02-24
 *      Weecoboard-4M module
 *      Author: Luciano Neri <l.neri@nerinformatica.it>
*/

#define TEST


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MIG.Interfaces.HomeAutomation.Commons;

namespace MIG.Interfaces.EmbeddedSystems
{
    public class Weeco4mGPIO : MIGInterface
    {

        #region Implemented MIG Commands

        // typesafe enum
        public sealed class Command : GatewayCommand
        {

            public static Dictionary<int, string> CommandsList = new Dictionary<int, string>()
            {
                {203, "Parameter.Status"},
				{204, "Parameter.Counter"},
				{205, "Parameter.Periode"},
				{231, "Parameter.Reset"},

                {701, "Control.On"},
                {702, "Control.Off"},
                {731, "Control.Reset"},
            };

            // <context>.<command> enum   -   eg. Control.On where <context> :== "Control" and <command> :== "On"
            public static readonly Command PARAMETER_STATUS = new Command(203);
            public static readonly Command PARAMETER_COUNTER = new Command(204);
            public static readonly Command PARAMETER_PERIODE = new Command(205);
            public static readonly Command PARAMETER_RESET = new Command(231);


            public static readonly Command CONTROL_ON = new Command(701);
            public static readonly Command CONTROL_OFF = new Command(702);
            public static readonly Command CONTROL_RESET = new Command(731);

            private readonly String name;
            private readonly int value;

            private Command(int value)
            {
                this.name = CommandsList[value];
                this.value = value;
            }

            public Dictionary<int, string> ListCommands()
            {
                return Command.CommandsList;
            }

            public int Value
            {
                get { return this.value; }
            }

            public override String ToString()
            {
                return name;
            }

            public static implicit operator String(Command a)
            {
                return a.ToString();
            }

            public static explicit operator Command(int idx)
            {
                return new Command(idx);
            }

            public static explicit operator Command(string str)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command " + str);

                if (CommandsList.ContainsValue(str))
                {
                    var cmd = from c in CommandsList where c.Value == str select c.Key;
                    return new Command(cmd.First());
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            public static bool operator ==(Command a, Command b)
            {
                return a.value == b.value;
            }
            public static bool operator !=(Command a, Command b)
            {
                return a.value != b.value;
            }
        }

        #endregion

        private const string GPIO_LED_PATH = "/sys/class/leds/";
        private const string GPIO_INOUT_PATH = "/sys/kernel/lgw4m-8di/";


#if TEST
        private Dictionary<string, string> CrossRefPins = new Dictionary<string, string>()		
        {
        	{ "0", GPIO_INOUT_PATH + "in0_value" },
        	{ "1", GPIO_INOUT_PATH + "in1_value" },
        	{ "2", GPIO_INOUT_PATH + "in2_value" },
        	{ "3", GPIO_INOUT_PATH + "in3_value" },
        	{ "4", GPIO_INOUT_PATH + "in4_value" },
        	{ "5", GPIO_INOUT_PATH + "in5_value" },
        	{ "6", GPIO_INOUT_PATH + "in6_value" },
        	{ "7", GPIO_INOUT_PATH + "in7_value" },
			
        	{ "8", GPIO_LED_PATH + "led_orange/brightness" },
        	{ "9", GPIO_LED_PATH + "led_green/brightness" },
        	{ "10", GPIO_LED_PATH + "output1/brightness" },
        	{ "11", GPIO_LED_PATH + "output2/brightness" },
			
        	{ "16", GPIO_INOUT_PATH + "in0_counter" },
        	{ "17", GPIO_INOUT_PATH + "in1_counter" },			
        	{ "18", GPIO_INOUT_PATH + "in2_counter" },			
        	{ "19", GPIO_INOUT_PATH + "in3_counter" },			
        	{ "20", GPIO_INOUT_PATH + "in4_counter" },
        	{ "21", GPIO_INOUT_PATH + "in5_counter" },
        	{ "22", GPIO_INOUT_PATH + "in6_counter" },			
        	{ "23", GPIO_INOUT_PATH + "in7_counter" },
			
        	{ "32", GPIO_INOUT_PATH + "in0_periode" },			
        	{ "33", GPIO_INOUT_PATH + "in1_periode" },	
        	{ "34", GPIO_INOUT_PATH + "in2_periode" },			
        	{ "35", GPIO_INOUT_PATH + "in3_periode" },				
        	{ "36", GPIO_INOUT_PATH + "in4_periode" },			
        	{ "37", GPIO_INOUT_PATH + "in5_periode" },				
        	{ "38", GPIO_INOUT_PATH + "in6_periode" },			
        	{ "39", GPIO_INOUT_PATH + "in7_periode" }			
			
		};



        public Dictionary<string, bool> GPIOPins = new Dictionary<string, bool>()
		{
            { "0", false },
            { "1", false },
            { "2", false },
            { "3", false },
            { "4", false },
            { "5", false },
            { "6", false },
            { "7", false },
			
            { "8", false },
            { "9", false },
			
            { "10", false },
            { "11", false }
		};

        public Dictionary<string, string> REGPins = new Dictionary<string, string>()
		{			
            { "16", "0" },
            { "17", "0" },
            { "18", "0" },
            { "19", "0" },
            { "20", "0" },
            { "21", "0" },
            { "22", "0" },
            { "23", "0" },
		
            { "32", "0" },
            { "33", "0" },
            { "34", "0" },
            { "35", "0" },
            { "36", "0" },
            { "37", "0" },
            { "38", "0" },
            { "39", "0" }			
        };


#else
		private Dictionary<string, string> CrossRefPins = new Dictionary<string, string>()		{
        	{ "input0", GPIO_INOUT_PATH + "in0_value" },
        	{ "input1", GPIO_INOUT_PATH + "in1_value" },
        	{ "input2", GPIO_INOUT_PATH + "in2_value" },
        	{ "input3", GPIO_INOUT_PATH + "in3_value" },
        	{ "input4", GPIO_INOUT_PATH + "in4_value" },
        	{ "input5", GPIO_INOUT_PATH + "in5_value" },
        	{ "input6", GPIO_INOUT_PATH + "in6_value" },
        	{ "input7", GPIO_INOUT_PATH + "in7_value" },
			
        	{ "led0", GPIO_LED_PATH + "led_orange/brightness" },
        	{ "led1", GPIO_LED_PATH + "led_green/brightness" },
        	{ "output0", GPIO_LED_PATH + "output1/brightness" },
        	{ "output1", GPIO_LED_PATH + "output2/brightness" },
			
        	{ "counter0", GPIO_INOUT_PATH + "in0_counter" },
        	{ "counter1", GPIO_INOUT_PATH + "in1_counter" },			
        	{ "counter2", GPIO_INOUT_PATH + "in2_counter" },			
        	{ "counter3", GPIO_INOUT_PATH + "in3_counter" },			
        	{ "counter4", GPIO_INOUT_PATH + "in4_counter" },
        	{ "counter5", GPIO_INOUT_PATH + "in5_counter" },
        	{ "counter6", GPIO_INOUT_PATH + "in6_counter" },			
        	{ "counter7", GPIO_INOUT_PATH + "in7_counter" },
			
        	{ "periode0", GPIO_INOUT_PATH + "in0_periode" },			
        	{ "periode1", GPIO_INOUT_PATH + "in1_periode" },	
        	{ "periode2", GPIO_INOUT_PATH + "in2_periode" },			
        	{ "periode3", GPIO_INOUT_PATH + "in3_periode" },				
        	{ "periode4", GPIO_INOUT_PATH + "in4_periode" },			
        	{ "periode5", GPIO_INOUT_PATH + "in5_periode" },				
        	{ "periode6", GPIO_INOUT_PATH + "in6_periode" },			
        	{ "periode7", GPIO_INOUT_PATH + "in7_periode" }			
			
		};
		
		
		
    	public Dictionary<string, bool> GPIOPins = new Dictionary<string, bool>()
		{
            { "input0", false },
            { "input1", false },
            { "input2", false },
            { "input3", false },
            { "input4", false },
            { "input5", false },
            { "input6", false },
            { "input7", false },
			
            { "led0", false },
            { "led1", false },
			
            { "output0", false },
            { "output1", false }
		};
		
    	public Dictionary<string, string> REGPins = new Dictionary<string, string>()
		{			
            { "counter0", "0" },
            { "counter1", "0" },
            { "counter2", "0" },
            { "counter3", "0" },
            { "counter4", "0" },
            { "counter5", "0" },
            { "counter6", "0" },
            { "counter7", "0" },
		
            { "periode0", "0" },
            { "periode1", "0" },
            { "periode2", "0" },
            { "periode3", "0" },
            { "periode4", "0" },
            { "periode5", "0" },
            { "periode6", "0" },
            { "periode7", "0" }			
        };
#endif
        bool isconnected = false;
        bool isDesktopEmulation = false;

        public Weeco4mGPIO()
        {
            isconnected = Directory.Exists(GPIO_INOUT_PATH) && Directory.Exists(GPIO_LED_PATH);
            //if (!isconnected)
            //{
            //	isconnected = true;
            //	isDesktopEmulation = true;	
            //}
        }

        #region MIG Interface members

        public event Action<InterfacePropertyChangedAction> InterfacePropertyChangedAction;

        public string Domain
        {
            get
            {
                string ifacedomain = this.GetType().Namespace.ToString();
                ifacedomain = ifacedomain.Substring(ifacedomain.LastIndexOf(".") + 1) + "." + this.GetType().Name.ToString();
                return ifacedomain;
            }
        }

        public bool Connect()
        {
            return isconnected;
        }
        public void Disconnect()
        {
            CleanUpAllPins();
        }
        public bool IsDevicePresent()
        {
            return isconnected;
        }
        public bool IsConnected
        {
            get { return isconnected; }
        }

        public void WaitOnPending()
        {
        }

        public object InterfaceControl(MIGInterfaceCommand request)
        {
            if (DEBUG) Console.WriteLine("Weeco4mGPIO Command : " + request);

            Command command = (Command)request.command;
            string returnvalue = "";
            //
            if (command == Command.CONTROL_ON)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command.CONTROL_ON ");

                this.OutputPin(request.nodeid, "1");
                if (IsRegister(request.nodeid))
                    REGPins[request.nodeid] = "1";
                else
                    GPIOPins[request.nodeid] = true;
                if (InterfacePropertyChangedAction != null)
                {
                    try
                    {
                        InterfacePropertyChangedAction(new InterfacePropertyChangedAction() { Domain = this.Domain, SourceId = request.nodeid, SourceType = "Weeco-4M GPIO", Path = ModuleParameters.MODPAR_STATUS_LEVEL, Value = 1 });
                    }
                    catch { }
                }
            }
            //
            if (command == Command.CONTROL_OFF)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command.CONTROL_OFF ");

                this.OutputPin(request.nodeid, "0");
                if (IsRegister(request.nodeid))
                    REGPins[request.nodeid] = "0";
                else
                    GPIOPins[request.nodeid] = false;

                if (InterfacePropertyChangedAction != null)
                {
                    try
                    {
                        InterfacePropertyChangedAction(new InterfacePropertyChangedAction() { Domain = this.Domain, SourceId = request.nodeid, SourceType = "Weeco-4M GPIO", Path = ModuleParameters.MODPAR_STATUS_LEVEL, Value = 0 });
                    }
                    catch { }
                }
            }
            //
            if (command == Command.PARAMETER_STATUS)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command.PARAMETER_STATUS ");
                if (!IsRegister(request.nodeid))
                    GPIOPins[request.nodeid] = this.InputPin(request.nodeid).CompareTo("0") == 0 ? false : true;
                else
                    REGPins[request.nodeid] = this.InputPin(request.nodeid);
            }
            //
            if (command == Command.CONTROL_RESET)
            {
                this.CleanUpAllPins();
                foreach (KeyValuePair<string, bool> kv in GPIOPins)
                {
                    GPIOPins[kv.Key] = false;
                }
                foreach (KeyValuePair<string, string> kv in REGPins)
                {
                    REGPins[kv.Key] = "0";
                }
            }
            //			
            if (command == Command.PARAMETER_COUNTER)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command.PARAMETER_COUNTER ");

            }
            //			
            if (command == Command.PARAMETER_PERIODE)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command.PARAMETER_PERIOD ");


            }
            //			
            if (command == Command.PARAMETER_RESET)
            {
                if (DEBUG) Console.WriteLine("Weeco4mGPIO Command.PARAMETER_RESET ");

            }


            return returnvalue;
        }

        #endregion


        public enum enumPIN
        {
            gpio0 = 0, gpio1 = 1, gpio2 = 2, gpio3 = 3, gpio4 = 4, gpio5 = 5, gpio6 = 6, gpio7 = 7,	// input
            gpio16 = 16, gpio17 = 17, gpio18 = 18, gpio19 = 19, gpio20 = 20, gpio21 = 21, gpio22 = 22, gpio23 = 23,	// counters
            gpio24 = 32, gpio25 = 33, gpio26 = 34, gpio27 = 35, gpio28 = 36, gpio29 = 37, gpio30 = 38, gpio31 = 39,	// periode
            gpio8 = 8, gpio9 = 9, gpio10 = 10, gpio11 = 11
        };	// output (leds and out)

        public enum enumDirection { IN, OUT };



        //set to true to write whats happening to the screen
        private const bool DEBUG = true;

        private bool IsRegister(string nodeid)
        {
            return nodeid.StartsWith("counter") || nodeid.StartsWith("periode");
        }

        //no need to setup pin this is done for you
        public void OutputPin(string pin, string value)
        {
            if (!isDesktopEmulation)
                File.WriteAllText(CrossRefPins[pin], value);

            if (DEBUG) Console.WriteLine("Weeco4mGPIO  output to pin " + CrossRefPins[pin] + ", value was " + value);
        }

        //no need to setup pin this is done for you
        public string InputPin(string pin)
        {
            string returnValue = "0";

            string filename = CrossRefPins[pin];

            if (!isDesktopEmulation)
            {
                if (File.Exists(filename))
                {
                    returnValue = File.ReadAllText(filename);
                }
                else
                    throw new Exception(string.Format("Cannot read from {0}. File does not exist", pin));
            }
            else
                returnValue = "-1";

            if (DEBUG) Console.WriteLine("Weeco4mGPIO  input from pin " + CrossRefPins[pin] + ", value was " + returnValue);

            return returnValue;
        }

        public void CleanUpAllPins()
        {

        }
    }
}
