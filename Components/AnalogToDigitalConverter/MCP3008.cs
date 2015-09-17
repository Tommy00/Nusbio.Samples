/*
   Copyright (C) 2015 MadeInTheUSB LLC
   Written by FT for MadeInTheUSB
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
  
    MIT license, all text above must be included in any redistribution
  
    Written with the help of
        https://rheingoldheavy.com/mcp3008-tutorial-02-sampling-dc-voltage/
  
    MCP3008 10bit ADC Breakout Board from RheinGoldHeavy.com supported
    https://rheingoldheavy.com/product/breakout-board-mcp3008/
  
  
    Datasheet http://www.adafruit.com/datasheets/MCP3008.pdf
 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadeInTheUSB.i2c;
using MadeInTheUSB.GPIO;
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using MadeInTheUSB.spi;

namespace MadeInTheUSB
{
    /// <summary>
    /// 
    /// </summary>
    public class MCP3008_Base
    {
        SPIEngine _spiEngine;

        public readonly int MAX_PORT = 8;

        private List<int> _channels = new List<int>() {
            0x08,
            0x09,
            0x0A,
            0x0B,
            0x0C,
            0x0D,
            0x0E,
            0x0F
        };


        /// <summary>
        /// Read the value of the analog port using software bit banging
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public int ReadSpiSoftware(int port)
        {
            this._spiEngine.Nusbio.SetPinMode(this._spiEngine.MisoGpio, PinMode.Input);
            int adcValue = 0;
            int command  = WinUtil.BitUtil.ParseBinary("B11000000");
            command |= (port) << 3;
            var nusbio   = this._spiEngine.Nusbio;

            this._spiEngine.Select();
            
            for (int i = 7; i >= 4; i--)
            {
                var r = command & (1 << i);
                nusbio[this._spiEngine.MosiGpio].DigitalWrite(r != 0);  
    
                nusbio[this._spiEngine.ClockGpio].DigitalWrite(true);
                nusbio[this._spiEngine.ClockGpio].DigitalWrite(false);    
            }
             
            nusbio[this._spiEngine.ClockGpio].DigitalWrite(true); // ignores 2 null bits
            nusbio[this._spiEngine.ClockGpio].DigitalWrite(false);    

            nusbio[this._spiEngine.ClockGpio].DigitalWrite(true);
            nusbio[this._spiEngine.ClockGpio].DigitalWrite(false);    

            // Read bits from adc since it is ADC is 10 bits
            for(var i = 10; i > 0; i--) {
                
                adcValue +=  Nusbio.ConvertTo1Or0(nusbio[this._spiEngine.MisoGpio].DigitalRead()) << i;
    
                nusbio[this._spiEngine.ClockGpio].DigitalWrite(true);
                nusbio[this._spiEngine.ClockGpio].DigitalWrite(false);    
            }
            this._spiEngine.Unselect();
            return adcValue;
        }

        public MCP3008_Base(Nusbio nusbio, NusbioGpio selectGpio, NusbioGpio mosiGpio, NusbioGpio misoGpio, NusbioGpio clockGpio, NusbioGpio resetGpio = NusbioGpio.None, bool debug = false) {

            this._spiEngine = new SPIEngine(nusbio, selectGpio, mosiGpio, misoGpio, clockGpio, resetGpio, debug);
        }

        public void Begin()
        {
            _spiEngine.Begin();
        }

        public int Read(int port)
        {
            if ((port > 7) || (port < 0)) return -1; // Wrong adc address return -1
                
            //var port2 = (byte)((_channels[port] << 4) + 0x03);
            var port2 = (byte)((_channels[port] << 4));
            var junk  = (byte)0;

            //var r1 = this._spiEngine.Transfer(new List<Byte>() { 0x01 });
            if (true)
            {
                var r1 = this._spiEngine.Transfer( new List<Byte>() {0x1, port2, junk} );
                //var r2 = this._spiEngine.Transfer( new List<Byte>() {} );
                return ValidateOperation(r1);
            }
            else return -1;
        }

        public int ValidateOperation(MadeInTheUSB.spi.SPIEngine.SPIResult r0)
        {
            if (r0.Succeeded && r0.ReadBuffer.Count == 3)
            {
                int r = 0;
                if (WinUtil.BitUtil.IsSet(r0.ReadBuffer[1], 1))
                    r += 256;
                if (WinUtil.BitUtil.IsSet(r0.ReadBuffer[1], 2))
                    r += 512;
                r += r0.ReadBuffer[2];
                return r;
            }
            return -1;
        }
    }
    
    public class MCP3008 : MCP3008_Base
    {
        public MCP3008(Nusbio nusbio, NusbioGpio selectGpio, NusbioGpio mosiGpio, NusbioGpio misoGpio, NusbioGpio clockGpio, bool debug = false)
        : base(nusbio, selectGpio, mosiGpio, misoGpio,clockGpio, NusbioGpio.None, debug )
        {
        }


        
    }


}

