/*
   Copyright (C) 2015 MadeInTheUSB LLC

   The MIT License (MIT)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
 
    Written by FT for MadeInTheUSB.net
    MIT license, all text above must be included in any redistribution
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadeInTheUSB;
using MadeInTheUSB.i2c;
using MadeInTheUSB.GPIO;
using MadeInTheUSB.WinUtil;
using MadeInTheUSB.Display;

namespace LCDConsole
{
    class Demo
    {
        
        static LiquidCrystal _liquidCrystal;
        static MachineInfo _machineInfo;

        static string GetAssemblyProduct()
        {
            Assembly currentAssem = typeof(Demo).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if(attribs.Length > 0)
                return  ((AssemblyProductAttribute) attribs[0]).Product;
            return null;
        }


        static void Cls(Nusbio nusbio)
        {
            Console.Clear();

            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);

            ConsoleEx.TitleBar(4, "D)isplay Speed Test  A)pi Demo  Q)uit", ConsoleColor.White, ConsoleColor.DarkBlue);

            ConsoleEx.TitleBar(ConsoleEx.WindowHeight-2, Nusbio.GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.Bar(0, ConsoleEx.WindowHeight-3, string.Format("Nusbio SerialNumber:{0}, Description:{1}", nusbio.SerialNumber, nusbio.Description), ConsoleColor.Black, ConsoleColor.DarkCyan);
        }

        private static void DisplaySpeedTest(LiquidCrystal lc) {

            lc.Clear();
            long i = -1;
            while(i++ < 1000) {
                
                lc.Clear();
                //lc.SetCursor(0, 0);
                var date = DateTime.Now.ToString("s");
                date = date.Substring(0, date.IndexOf("T"));
                switch (i % 2)
                {
                    case 0 :_liquidCrystal.PrintRightPadded("[{0}] {1}", i.ToString("00"), DateTime.Now.ToString("T").Replace(" ",""));  break;
                    case 1 :_liquidCrystal.Print("[{0}] {1}", i.ToString("00"), date);break;
                }
                lc.SetCursor(0, 1);
                switch (i % 3)
                {
                    case 0 : var cpuInfo = _machineInfo.CpuPercent.NextValue().ToString("0.00");_liquidCrystal.PrintRightPadded("Cpu {0}%", cpuInfo);     break;
                    case 1 :_liquidCrystal.PrintRightPadded("DiskR {0}Mb/s",  (_machineInfo.DiskReadBytePerSec.NextValue()/1024/1024).ToString("0.00"));  break;
                    case 2 :_liquidCrystal.PrintRightPadded("DiskW {0}Mb/s", (_machineInfo.DiskWriteBytePerSec.NextValue()/1024/1024).ToString("0.00"));  break;
                }
                TimePeriod.Sleep(1000);
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    break;
                }
            }
        }

        private static void ApiDemoDisplay(LiquidCrystal lc, int x, int y, string text, bool clear = true, int waitTime = 1000) {

            if(clear)
                lc.Clear();

            if(text.Length > 16) {

                var p = 15;
                while (p > 0 & text[p] != ' ')
                {
                    p--;
                }

                if(p==0)
                    p = 15;

                lc.Print(x, y, text.Substring(0, p));
                lc.Print(x, y+1, text.Substring(p).TrimStart());
            }
            else {
                lc.Print(x, y, text);
            }

            if(waitTime> 0)
                TimePeriod.Sleep(waitTime);
        }

        private static void CustomCharDemo(LiquidCrystal lc) {

            var smiley = new List<string>() {
              "B00000",
              "B10001",
              "B00000",
              "B00000",
              "B10001",
              "B01110",
              "B00000",
              "B00000",
            };

            lc.CreateChar(0, BitUtil.ParseBinary(smiley).ToArray());
            lc.Clear();
            lc.Write(0);
            lc.Write(0xF4);
            lc.Write(0xFF);
            lc.Write(0xFF);
            var k = Console.ReadKey();
        }



        private static void ApiDemo(LiquidCrystal lc) {

            ApiDemoDisplay(lc, 0, 0, " -- Api Demo --");

            // Display text
            ApiDemoDisplay(lc, 0, 0, "Display text on line 0 and 1");
            ApiDemoDisplay(lc, 0, 0, DateTime.Now.ToString("d"), waitTime:0);
            ApiDemoDisplay(lc, 0, 1, DateTime.Now.ToString("T"), clear:false);
            
            // Turn  display on / off
            ApiDemoDisplay(lc, 0, 0, "About to turn the display off", waitTime:2000);
            lc.NoDisplay();
            TimePeriod.Sleep(1000);
            lc.Display();
            ApiDemoDisplay(lc, 0, 0, "Display turned on");

            // Flash Screen
            ApiDemoDisplay(lc, 0, 0, "About to flash the screen ...", waitTime:2000);
            ApiDemoDisplay(lc, 0, 0, "Flashing the screen ...", waitTime:0);
            lc.Flash(10);
            TimePeriod.Sleep(1000);
            
            // Cursor Blink Demo
            ApiDemoDisplay(lc, 0, 0, "Cursor blink mode on");
            lc.Blink();
            TimePeriod.Sleep(1000*4);
            ApiDemoDisplay(lc, 0, 0, "Cursor blink mode off");
            lc.NoBlink();
            
            // Cursor Demo
            ApiDemoDisplay(lc, 0, 0, "Display Cursor");
            lc.Cursor();
            TimePeriod.Sleep(1000);
            for (var i = 0; i < 15; i++) { 
                lc.SetCursor(i, 0); TimePeriod.Sleep(300);
            }
            for (var i = 15; i >= 0; i--) { 
                lc.SetCursor(i, 0); TimePeriod.Sleep(300);
            }
            TimePeriod.Sleep(1000*1);
            lc.NoCursor();
            ApiDemoDisplay(lc, 0, 0, "Cursor off");

            // Autoscroll demo
            ApiDemoDisplay(lc, 0, 0, "Autoscroll Demo", waitTime:2000);
            NusbioRocks(lc);

            // Progress Bar Demo
            ApiDemoDisplay(lc, 0, 0, "Progress Bar Demo");
            ProgressBarDemo(lc);


            ApiDemoDisplay(lc, 0, 0, "-- Demo Done --");
        }

        private static void ProgressBarDemo(LiquidCrystal lc)
        {
            lc.Clear();
            lc.Print(0, 0, "Working hard...");
            for (var p = 0; p <= 100; p += 10)
            {
                lc.ProgressBar(0, 1, 10, p, string.Format("{0}% ", p.ToString("000")));
                TimePeriod.Sleep(150);
                if (Console.KeyAvailable)
                {
                    lc.Clear();
                    return;
                }
            }
            TimePeriod.Sleep(1000);
            lc.Clear();
        }

        private static void NusbioRocks(LiquidCrystal lc)
        {
            var autoScrollDemoText1 = "Nusbio for .NET          rocks or what?";
            lc.Clear();
            lc.Autoscroll();
            lc.SetCursor(15, 0);
            try { 
                foreach (var c in autoScrollDemoText1)
                {
                    lc.Print(c.ToString());
                    TimePeriod.Sleep(180);
                    if (Console.KeyAvailable)
                        return;
                }
                TimePeriod.Sleep(1000);
            }
            finally{
                lc.NoAutoscroll();
                lc.Clear();
            }
        }
        
        private static void DisplayTime(LiquidCrystal lc)
        {
            lc.Print(0, 0, DateTime.Now.ToString("d"));
            lc.Print(0, 1, DateTime.Now.ToString("T"));
        }













        public static void Run(string[] args)
        {
            Console.WriteLine("Nusbio initialization");
            var serialNumber = Nusbio.Detect();
            if (serialNumber == null) // Detect the first Nusbio available
            {
                Console.WriteLine("Nusbio not detected");
                return;
            }

            using (var nusbio = new Nusbio(serialNumber))
            {
                Cls(nusbio);

                int counter       = 0;
                _machineInfo      = new MachineInfo();
                var secondTimeOut = new TimeOut(1000);
                _liquidCrystal    = new LiquidCrystal(nusbio, rs:0, enable:1, d0:2, d1:3, d2:4, d3:5);

                _liquidCrystal.Begin(16, 2);
                _liquidCrystal.Clear();

                while(nusbio.Loop())
                {
                    if (secondTimeOut.IsTimeOut())
                    {
                        counter++;
                        DisplayTime(_liquidCrystal);
                        if(counter % 5 ==0)
                            NusbioRocks(_liquidCrystal);
                        if(counter % 8 ==0)
                            ProgressBarDemo(_liquidCrystal);
                    }

                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.D)
                        {
                            DisplaySpeedTest(_liquidCrystal);
                            _liquidCrystal.Clear();
                        }
                        if (k == ConsoleKey.A)
                        {
                            ApiDemo(_liquidCrystal);
                            _liquidCrystal.Clear();
                        }
                        if (k == ConsoleKey.C)
                        {
                            CustomCharDemo(_liquidCrystal);
                            _liquidCrystal.Clear();
                        }
                        
                        if (k == ConsoleKey.F)
                        {
                            Cls(nusbio);
                        }
                        if (k == ConsoleKey.Q) {
                            _liquidCrystal.Clear();
                            break;
                        }
                        Cls(nusbio);
                    }
                }
            }

            Console.Clear();
        }

        
    }
}

