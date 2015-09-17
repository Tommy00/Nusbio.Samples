/*
    Copyright (C) 2015 MadeInTheUSB LLC

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    associated documentation files (the "Software"), to deal in the Software without restriction, 
    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial 
    portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
using MadeInTheUSB.Components;
using System.Drawing;
using MadeInTheUSB.Components.APA;

namespace LightSensorConsole
{
    class Demo
    {

        static string GetAssemblyProduct()
        {
            Assembly currentAssem = typeof(Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if(attribs.Length > 0)
                return  ((AssemblyProductAttribute) attribs[0]).Product;
            return null;
        }

        static char AskForStripType()
        {
            Console.Clear();

            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight-2, Nusbio.GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            var r = ConsoleEx.Question(ConsoleEx.WindowHeight-3, "Strip Type?    3)0 LED/Meter   6)0 LED/Meter  I) do not know", new List<Char>() {'3', '6', 'I'});
            return r;
        }

        static void Cls(Nusbio nusbio)
        {
            Console.Clear();

            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 5, "B)rightness Demo   R)GB Demo   S)croll Demo   SP)eed Demo");
            ConsoleEx.WriteMenu(-1, 7, "A)mp Test   RainboW) Demo   L)ine Demo   AlT)ernate Line Demo");
            ConsoleEx.WriteMenu(-1, 9, "Q)uit");
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight-2, Nusbio.GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.Bar(0, ConsoleEx.WindowHeight-3, string.Format("Nusbio SerialNumber:{0}, Description:{1}", nusbio.SerialNumber, nusbio.Description), ConsoleColor.Black, ConsoleColor.DarkCyan);
        }

        
        private static string TargetColors = @"Blue
BlueViolet
Brown
Chartreuse
Chocolate
CornflowerBlue
Crimson
Cyan
DarkOrange
DarkOrchid
DarkRed
DarkTurquoise
DarkViolet
DarkBlue
DarkCyan
DarkGoldenrod
DarkGreen
DarkMagenta
DeepPink
DeepSkyBlue
DodgerBlue
Firebrick
ForestGreen
Fuchsia
Gold
Green
Indigo
LawnGreen
LightSeaGreen
Lime
Maroon
MediumBlue
MediumSpringGreen
MediumVioletRed
MidnightBlue
Navy
Olive
Orange
OrangeRed
Purple
Red
RoyalBlue
SeaGreen
SpringGreen
Teal
Turquoise
Yellow
";

        private static int GetWaitTimeUnit(APA102LEDStrip ledStrip)
        {
            var wait = (int)((80.0/(2*ledStrip.MaxLed))*10);
            return wait;
        }

        public static void ScrollDemo(APA102LEDStrip ledStrip)
        {
            var wait = GetWaitTimeUnit(ledStrip);
            var quit             = false;
            ledStrip.Brightness = 16;
            ledStrip.AllOff();

            Console.Clear();
            ConsoleEx.TitleBar(0, "Scroll Demo");
            ConsoleEx.WriteMenu(-1, 2, "Q)uit");
            ConsoleEx.WriteMenu(-1, 3, "");
            
            var bkColors = TargetColors.Replace(Environment.NewLine, ",").Split(',').ToList();
            
            while (!quit)
            {
                foreach (var sBColor in bkColors) 
                {
                    if(string.IsNullOrEmpty(sBColor.Trim()))
                        continue;

                    var bkColor = APA102LEDStrip.DrawingColors[sBColor];

                    bkColor = APA102LEDStrip.ToBrighter(bkColor, -65);

                    Console.WriteLine(String.Format("Background Color:{0}, Html:{1}, Dec:{2}", 
                        bkColor.Name.PadRight(16), 
                        APA102LEDStrip.ToHexValue(bkColor), 
                        APA102LEDStrip.ToDecValue(bkColor)));

                    var fgColor = APA102LEDStrip.ToBrighter(bkColor, 20);

                    ledStrip.AddRGBSequence(true, 4, ledStrip.MaxLed-1, bkColor);
                    ledStrip.InsertRGBSequence(0, 15, fgColor);
                    ledStrip.ShowAndShiftRightAllSequence(wait);
                    
                    if (Console.KeyAvailable)
                    {
                        quit = true;
                        break;
                    }
                }
            }
            ledStrip.AllOff();
            var k = Console.ReadKey(true).Key;
        }

        public static void SpeedTest(APA102LEDStrip ledStrip)
        {
            ledStrip.Brightness = 16;
            ledStrip.AllOff();
            Console.Clear();            
            Console.WriteLine("Running test...");
            
            var bkColor   = Color.Red;
            var sw        = Stopwatch.StartNew();
            var testCount = 1000;

            // This loop set the strip 500 x 2 == 1000 times
            for (var t = 0; t < (testCount/2); t++)
            {
                // Light up in red the 60 led strips
                ledStrip.Reset();
                for (var l = 0; l < ledStrip.MaxLed; l ++)
                    ledStrip.AddRGBSequence(false, 7, bkColor);
                ledStrip.Show();

                // Turn it off the 60 led strips
                ledStrip.Reset();
                for (var l = 0; l < ledStrip.MaxLed; l ++)
                    ledStrip.AddRGBSequence(false, 7, Color.Black);
                ledStrip.Show();
            }

            sw.Stop();
            var bytePerSeconds = ledStrip.MaxLed * 4 * testCount / (sw.ElapsedMilliseconds/1000);

            Console.WriteLine("test Duration:{0}, BytePerSecond:{1}, NumberOfLedTurnOnOrOff:{2}",  
                sw.ElapsedMilliseconds,
                bytePerSeconds,
                ledStrip.MaxLed*testCount
                );
            var k = Console.ReadKey(true).Key;

            ledStrip.AllOff();
            k = Console.ReadKey(true).Key;
        }



        public static void AlternateLineDemo(APA102LEDStrip ledStrip)
        {
            var wait = GetWaitTimeUnit(ledStrip);
            if (ledStrip.MaxLed <= 10)
                wait *= 3;

            var quit             = false;
            ledStrip.Brightness = 16;
            ledStrip.AllOff();

            Console.Clear();
            ConsoleEx.TitleBar(0, "Alternate Line Demo");
            ConsoleEx.WriteMenu(-1, 2, "Q)uit");
            ConsoleEx.WriteMenu(-1, 3, "");
            
            var bkColors = TargetColors.Replace(Environment.NewLine, ",").Split(',').ToList();
            
            while (!quit)
            {
                foreach (var sBColor in bkColors) 
                {
                    if(string.IsNullOrEmpty(sBColor.Trim()))
                        continue;

                    var bkColor = APA102LEDStrip.ToBrighter(APA102LEDStrip.DrawingColors[sBColor], -75);

                    Console.WriteLine(String.Format("Background Color:{0}, Html:{1}, Dec:{2}", 
                        bkColor.Name.PadRight(16), 
                        APA102LEDStrip.ToHexValue(bkColor), 
                        APA102LEDStrip.ToDecValue(bkColor)));

                    var fgColor = APA102LEDStrip.ToBrighter(bkColor, 40);

                    ledStrip.Reset();
                    for (var l = 0; l < ledStrip.MaxLed; l += 2)
                    {
                        ledStrip.AddRGBSequence(false, 4, bkColor);
                        ledStrip.AddRGBSequence(false, 6, fgColor);
                    }

                    for (var i = 0; i < ledStrip.MaxLed*3; i+=4)
                    {
                        ledStrip.Show().ShiftRightSequence().Wait(wait);
                        if (Console.KeyAvailable) break;
                    }
                    
                    if (Console.KeyAvailable)
                    {
                        quit = true;
                        break;
                    }
                }
            }
            ledStrip.AllOff();
            var k = Console.ReadKey(true).Key;
        }


        public static void RainbowDemo(APA102LEDStrip ledStrip, int jStep, APA102LEDStrip ledStrip2 = null)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rainbow Demo");
            ConsoleEx.WriteMenu(-1, 2, "Q)uit");
            ConsoleEx.WriteMenu(-1, 3, "");

            int brigthness = 6;
            int wait        = GetWaitTimeUnit(ledStrip)/2;
            var quit        = false;
            ledStrip.AllOff();
            if(ledStrip2 != null) ledStrip2.AllOff();

            while (!quit) 
            {                
                for(var j=0; j < 256; j += jStep) 
                {
                    ConsoleEx.Gotoxy(0, 4);
                    ledStrip.Reset();
                    if(ledStrip2 != null) ledStrip2.Reset();

                    for (var i = 0; i < ledStrip.MaxLed; i++)
                    {
                        ledStrip.AddRGBSequence(false, brigthness, APA102LEDStrip.Wheel(((i*256/ledStrip.MaxLed) + j)));
                    }
                    if (ledStrip2 != null)
                    {
                        for (var i = 0; i < ledStrip2.MaxLed; i++)
                        {
                            ledStrip2.AddRGBSequence(false, brigthness, APA102LEDStrip.Wheel(((i*256/ledStrip.MaxLed) + j) ));
                        }
                    }

                    foreach (var bkColor in ledStrip.LedColors) 
                       Console.WriteLine(String.Format("Color:{0}, Html:{1}, Dec:{2}",  bkColor.Name, APA102LEDStrip.ToHexValue(bkColor), APA102LEDStrip.ToDecValue(bkColor)));

                    if (ledStrip2 != null)
                    {
                        ledStrip.Show();
                        ledStrip2.Show().Wait(wait);
                    }
                    else
                    {
                        ledStrip.Show().Wait(wait);    
                    }

                    if(Console.KeyAvailable) 
                    {
                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q) 
                        { 
                            quit = true;
                            break;
                        }
                    }
                }
            }
            ledStrip.AllOff();
            if(ledStrip2 != null) 
                ledStrip2.AllOff();
        }


        public static void MultiShades(APA102LEDStrip ledStrip)
        {
            int wait        = 0;
            var quit        = false;
            ledStrip.AllOff();

            while (!quit) 
            {                
                for(var j=0; j < 256; j++) 
                {
                    Console.Clear();
                    ledStrip.Reset();

                    for (var i = 0; i < ledStrip.MaxLed; i++) 
                    { 
                        ledStrip.AddRGBSequence(false, 10, APA102LEDStrip.Wheel(((i+j))));
                    }

                    foreach (var bkColor in ledStrip.LedColors) 
                    { 
                       Console.WriteLine(String.Format("Color:{0}, Html:{1}, Dec:{2}",  bkColor.Name.PadRight(16), APA102LEDStrip.ToHexValue(bkColor), APA102LEDStrip.ToDecValue(bkColor)));
                    }

                    ledStrip.Show().Wait(wait);

                    if(Console.KeyAvailable) 
                    {
                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q) 
                        { 
                            quit = true;
                            break;
                        }
                    }
                }
            }
            ledStrip.AllOff();
        }


        public static void RGBDemo(APA102LEDStrip ledStrip)
        {
            int wait             = GetWaitTimeUnit(ledStrip);
            int waitStep         = 10;
            int maxWait          = 200;
            var quit             = false;
            var userMessage      = "Speed:{0}. Use Left and Right keys to change the speed";
            ledStrip.Brightness  = 22;
            ledStrip.AllOff();

            Console.Clear();
            ConsoleEx.TitleBar(0, "RGB Demo");
            ConsoleEx.WriteMenu(-1, 4, "Q)uit");
            ConsoleEx.WriteLine(0, 2, string.Format(userMessage, wait), ConsoleColor.DarkGray);

            while (!quit) 
            {
                ledStrip.AddRGBSequence(true, 2, ledStrip.MaxLed-1, Color.Blue); 
                ledStrip.InsertRGBSequence(0, 14, Color.Red);
                ledStrip.ShowAndShiftRightAllSequence(wait);
                
                if(!Console.KeyAvailable) {

                    ledStrip.AddRGBSequence(true, 3, ledStrip.MaxLed-1, Color.Green); 
                    ledStrip.InsertRGBSequence(0, 16, Color.Red);
                    ledStrip.ShowAndShiftRightAllSequence(wait);
                }

                if(Console.KeyAvailable) {

                    while (Console.KeyAvailable) { 

                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q)  
                            quit = true;
                        if (k == ConsoleKey.RightArrow) { 
                            wait += waitStep;
                            if(wait > maxWait) wait = maxWait;
                        }
                        if (k == ConsoleKey.LeftArrow) { 
                            wait -= waitStep;
                            if(wait < 0) wait = 0;
                        }
                    }
                    ConsoleEx.WriteLine(0, 2, string.Format(userMessage, wait), ConsoleColor.DarkGray);
                }
            }
            ledStrip.AllOff();
        }

        public static void LineDemo(APA102LEDStrip ledStrip)
        {
            int wait             = ledStrip.MaxLed <= 10 ? 55 : 0;
            var quit             = false;
            ledStrip.AllOff();

            Console.Clear();
            ConsoleEx.TitleBar(0, "Line Demo");
            ConsoleEx.WriteMenu(-1, 2, "Q)uit");
            ConsoleEx.WriteMenu(-1, 3, "");

            while (!quit) 
            {
                var j = 0;

                for (var i = 0; i < ledStrip.MaxLed; i++)
                {
                    // Remark: there should be a faster way to draw the line, by first setting all the led
                    // to black and only resetting the one in color. Once we light up all the led, we would
                    // turn them all off and re start... Todo, totry.
                    var bkColor = APA102LEDStrip.Wheel(((i * 256 / ledStrip.MaxLed) + j));
                    ledStrip.AddRGBSequence(true, 2, i + 1, bkColor);
                    if(++j >= 256) 
                        j = 0;
                    while(!ledStrip.IsFull) 
                        ledStrip.AddRGBSequence(false, 2, Color.Black); 

                    ledStrip.Show().Wait(wait);

                    Console.WriteLine(String.Format("Color:{0}, Html:{1}, Dec:{2}",  bkColor.Name.PadRight(16), APA102LEDStrip.ToHexValue(bkColor), APA102LEDStrip.ToDecValue(bkColor)));

                    if(Console.KeyAvailable) {

                        while (Console.KeyAvailable) { 

                            var k = Console.ReadKey(true).Key;
                            if (k == ConsoleKey.Q) { 
                                quit = true;
                                break;
                            }
                        }
                    }
                }
                ledStrip.Wait(wait*3).AllOff();
            }
            ledStrip.AllOff();
        }

        public static void BrigthnessDemo(APA102LEDStrip ledStrip)
        {
            int maxBrightness = APA102LEDStrip.MAX_BRIGHTNESS / 2;
            int wait = GetWaitTimeUnit(ledStrip)/2;
            int step = 1;
            ledStrip.AllOff();
            Console.Clear();
            ConsoleEx.WriteMenu(-1, 3, "Q)uit");
            while (!Console.KeyAvailable) 
            {
                for (var b = 1; b <= maxBrightness; b += step) {

                    ledStrip.Reset();
                    for (var l = 0; l < ledStrip.MaxLed; l++) { 
                        if(!ledStrip.IsFull) ledStrip.AddRGB(Color.Red, b);
                        if(!ledStrip.IsFull) ledStrip.AddRGB(Color.Green, b);
                        if(!ledStrip.IsFull) ledStrip.AddRGB(Color.Blue, b);
                    }
                    ConsoleEx.Write(0, 0,string.Format("Brightness {0:00}", b), ConsoleColor.DarkCyan);
                    ledStrip.Show().Wait(wait);
                }
                ledStrip.Wait(wait*10);
                for (var b = maxBrightness; b >= 1; b -= step) { 

                    ledStrip.Reset();
                    for (var l = 0; l < ledStrip.MaxLed; l++) { 
                        if(!ledStrip.IsFull) ledStrip.AddRGB(Color.Red, b);
                        if(!ledStrip.IsFull) ledStrip.AddRGB(Color.Green, b);
                        if(!ledStrip.IsFull) ledStrip.AddRGB(Color.Blue, b);
                    }
                    ConsoleEx.Write(0, 0,string.Format("Brightness {0:00}", b), ConsoleColor.DarkCyan);
                    ledStrip.Show().Wait(wait);
                }
                //ledStrip.AllOff();
                ledStrip.Wait(wait*10);
                if(Console.KeyAvailable)
                    break;
            }
            ledStrip.AllOff();
            var k = Console.ReadKey(true).Key;
        }


        /// <summary>
        /// 
        /// *** ATTENTION ***
        /// 
        /// WHEN CONTROLLING AN APA LED STRIP WITH NUSBIO YOU MUST KNOW THE AMP CONSUMPTION.
        /// 
        /// USB DEVICE ARE LIMITED TO 500 MILLI AMP.
        /// 
        /// AN LED IN GENERAL CONSUMES FROM 20 TO 25 MILLI AMP. AN RGB LED CONSUMES 3 TIMES 
        /// MORE IF THE RED, GREEN AND BLUE ARE SET TO THE 255, 255, 255 WHICH IS WHITE
        /// AT THE MAXIMUN INTENSISTY WHICH IS 31.
        /// 
        /// YOU MUST KNOW WHAT IS THE MAXIMUN CONSUMPTION OF YOUR APA 102 RGB LEB STRIP WHEN THE 
        /// RGB IS SET TO WHITE, WHITE, WHITE AND THE BRIGTHNESS IS AT THE MAXIMUM.
        /// 
        ///    -------------------------------------------------------------------------------
        ///    --- NEVER GO OVER 300 MILLI AMP IF THE LED STRIP IS POWERED FROM THE NUSBIO ---
        ///    -------------------------------------------------------------------------------
        /// 
        ///         POWER ONLY A LED STRIP OF 5 LED WHEN DIRECTLY PLUGGED INTO NUSBIO.
        /// 
        /// THE FUNCTION AmpTest() WILL LIGHT UP THE FIRST LED OF THE STRIP AT MAXIMUM BRIGHTNESS.
        /// USE A MULTI METER TO WATCH THE AMP COMSUMPTION.
        /// 
        /// IF YOU WANT TO POWER MORE THAN 5 LEDS, THERE ARE 2 SOLUTIONS:
        /// 
        /// (1) ONLY FOR 6 to 10 LEDs. ADD BETWEEN NUSBIO VCC AND THE STRIP 5V PIN A 47 OHM RESISTORS.
        /// YOU WILL LOOSE SOME BRIGTHNESS, BUT IT IS SIMPLER. THE RESISTOR LIMIT THE CURRENT THAT
        /// CAN BE USED FROM THE USB.
        /// 
        /// (2) USE A SECOND SOURCE OF POWER LIKE:
        /// 
        ///  - A 5 VOLTS 1 AMPS ADAPTERS TO POWER A 30 LED STRIP
        ///  - A 5 VOLTS 2 AMPS ADAPTERS TO POWER A 60 LED STRIP
        ///  
        /// ~~~ ATTENTION ~~~
        /// 
        ///     WHEN USING A SECOND SOURCE OF POWER IN THE SAME BREADBOARD OR PCB, ~ NEVER ~ 
        ///     CONNECT THE POSISTIVE OF THE SECOND SOURCE OF POWER WITH THE NUSBIO VCC.
        /// 
        /// SEE OUR WEB SITE 'LED STRIP TUTORIAL' FOR MORE INFO.
        /// 
        /// </summary>
        /// <param name="ledStrip"></param>
        public static void AmpTest(APA102LEDStrip ledStrip)
        {
            int wait             = 37;
            ledStrip.Brightness = 22;
            var quit             = false;
            ledStrip.AllOff();

            Console.Clear();

            // Set the first LED of the strip to max brithness and all other led will be off
            ledStrip.Reset();
            ledStrip.AddRGBSequence(true, APA102LEDStrip.MAX_BRIGHTNESS, Color.White);
            while (!ledStrip.IsFull)
            {
                ledStrip.AddRGBSequence(false, 1, Color.Black); // Color black does not consume current
            }
            ledStrip.Show().Wait(wait);

            Console.WriteLine("Measure the AMP consumption - Hit enter to continue");
            Console.ReadLine();

            //////10 LED all white, minimun intensisty
            var b = 1;
            Console.WriteLine("Brigthness {0}", b);
            ledStrip.AllOff();
            ledStrip.Reset();
            while (!ledStrip.IsFull)
            {
                ledStrip.AddRGBSequence(false, b, Color.White);
            }
            ledStrip.Show().Wait(wait);

            Console.WriteLine("Measure the AMP consumption - Hit enter to continue");
            Console.ReadLine();

            ledStrip.AllOff();
        }

        public static void Run(string[] args)
        {
            Console.WriteLine("Nusbio initialization");
            var serialNumber = Nusbio.Detect();
            if (serialNumber == null) // Detect the first Nusbio available
            {
                Console.WriteLine("nusbio not detected");
                return;
            }
            Console.Clear();
            
            using (var nusbio = new Nusbio(serialNumber))
            {
                // 30 led per meter strip
                APA102LEDStrip ledStrip0 = APA102LEDStrip.Extensions.TwoStripAdapter.Init(nusbio, APA102LEDStrip.Extensions.LedPerMeter._30LedPerMeter, APA102LEDStrip.Extensions.StripIndex._0, 10);

                //ledStrip0 = new APA102LEDStrip(nusbio, 10, 2, 3).AllOff();
                
                // 60 led per meter strip

                if (AskForStripType() == '6')
                {
                    ledStrip0 = APA102LEDStrip.Extensions.TwoStripAdapter.Init(nusbio, APA102LEDStrip.Extensions.LedPerMeter._60LedPerMeter, APA102LEDStrip.Extensions.StripIndex._0, 60);
                }

                ledStrip0.AllOff();
                Cls(nusbio);

                // For more information about the Nusbio APA102 2 Strip Adapter to control up to 2 strips 
                // with 10 RGB LED on each strip powered from Nusbio. See following url
                // http://www.madeintheusb.net/TutorialExtension/Index#Apa102RgbLedStrip

                APA102LEDStrip ledStrip1 = APA102LEDStrip.Extensions.TwoStripAdapter.Init(nusbio, APA102LEDStrip.Extensions.LedPerMeter._30LedPerMeter, APA102LEDStrip.Extensions.StripIndex._1, 10);
                //ledStrip2 = null;
                if(ledStrip1 != null)
                    ledStrip1.AllOff();

                while(nusbio.Loop())
                {
                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q)
                            break;

                        if (k == ConsoleKey.R)
                            RGBDemo(ledStrip0);

                        //if (k == ConsoleKey.Z)
                        //    RGBDemo(ledStrip2);

                        if (k == ConsoleKey.B)
                            BrigthnessDemo(ledStrip0);

                        if (k == ConsoleKey.A)
                            AmpTest(ledStrip0);

                        if (k == ConsoleKey.W)
                            RainbowDemo(ledStrip0, 6, ledStrip1);

                        if (k == ConsoleKey.D1)
                            RainbowDemo(ledStrip0, 1);

                        if (k == ConsoleKey.M)
                            MultiShades(ledStrip0);

                        if (k == ConsoleKey.S)
                            ScrollDemo(ledStrip0);

                        if (k == ConsoleKey.T)
                            AlternateLineDemo(ledStrip0);

                        if (k == ConsoleKey.P)
                            SpeedTest(ledStrip0);

                        if (k == ConsoleKey.L)
                            LineDemo(ledStrip0);

                        Cls(nusbio);
                    }
                }
            }
            Console.Clear();
        }
    }
}


