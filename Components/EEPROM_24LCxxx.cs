/*
   Copyright (C) 2015 MadeInTheUSB LLC
   Written by FT for MadeInTheUSB.net
 
   Support of the Microship EEPROM 24LCXXX series
 
   About data transfert speed:
   ===========================
   Data transfert rate from the EEPROM to .NET is around 5.4k byte per second.
   6 seconds to transfert 32k. 
 
   Here we are talking about transfering data from the EEPROM to the memory of the .NET program
   using the I2C protocol. When using SPI protocol Nusbio can transfert out at the speed of 6.8k/sec.
   
   The reason why is that though Nusbio use the hardware acceleration of the FT232RL,
   There is only a 384 byte buffer used for communication between .NET and the chip,
   to use hardware acceleration. For every buffer that we send we get a 1 milli second
   latency mostlty due the USB communication protocol. Though the baud rate set is to
   1843200, we lose 1ms every time we send a buffer.
        See video https://www.youtube.com/watch?v=XJ48zJwrZI0
   A better solution will come with a future version of Nusbio that will use a different chip.
   
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
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadeInTheUSB.i2c;
using MadeInTheUSB.WinUtil;

namespace MadeInTheUSB.EEPROM
{
    public class EEPROM_24LCXXX_BUFFER
    {
        public const byte PAGE_SIZE = 64;

        public byte[] Buffer;
        public bool Succeeded;
    }

    public abstract class EEPROM_24LCXXX_BASE
    {
        protected int _kBit; // 256kBit = 32k
        protected I2CEngine _i2c;

        abstract public bool WritePage(int addr, byte [] buffer);
        abstract public bool WriteByte(int addr, byte value);
        abstract public int ReadByte(int addr);
        abstract public EEPROM_24LCXXX_BUFFER ReadPage(int addr, int len = EEPROM_24LCXXX_BUFFER.PAGE_SIZE);

        public EEPROM_24LCXXX_BASE(Nusbio nusbio, NusbioGpio sdaPin, NusbioGpio sclPin, int kBit, bool debug = false)
        {
            this._kBit = kBit;
            this._i2c = new I2CEngine(nusbio, sdaPin, sclPin, 0, debug);
        }

        public string Name
        {
            get
            {
                return string.Format("Microship 24LC{0}", this._kBit);
            }
        }

        public int MaxBit
        {
            get
            {
                return this._kBit * 1024;
            }
        }

        public int MaxByte
        {
            get
            {
                return MaxBit / 8;
            }
        }

        public int MaxPage
        {
            get
            {
                return this.MaxByte / PAGE_SIZE;
            }
        }

        public static int PAGE_SIZE
        {
            get
            {
                return EEPROM_24LCXXX_BUFFER.PAGE_SIZE;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, Bit:{1}, Byte:{2}, Page:{3}", this.Name, this.MaxBit, this.MaxByte, this.MaxPage);
        }
    }

    public class EEPROM_24LCXXX : EEPROM_24LCXXX_BASE
    {
        public const int DEFAULT_I2C_ADDR = 0x50;       // Microship 24LC256 = 32k

        private int _waitTimeAfterWriteOperation = 5; // milli second

        public EEPROM_24LCXXX(Nusbio nusbio, NusbioGpio sdaPin, NusbioGpio sclPin, int kBit, int waitTimeAfterWriteOperation = 5, bool debug = false) :
            base(nusbio, sdaPin, sclPin, kBit)
        {
            this._waitTimeAfterWriteOperation = waitTimeAfterWriteOperation;
        }

        public bool Begin(byte _addr = DEFAULT_I2C_ADDR)
        {
            if (this._i2c.DeviceId == 0)
                this._i2c.DeviceId = (byte)(_addr);
            return true;
        }

        public override bool WritePage(int addr, byte [] buffer)
        {
            var v = this._i2c.WriteBuffer(addr, buffer.Length, buffer, 0);

            // EEPROM need a wait time after a write operation
            if (this._waitTimeAfterWriteOperation > 0)
                TimePeriod.Sleep(this._waitTimeAfterWriteOperation);

            return v;
        }

        public override bool WriteByte(int addr, byte value)
        {
            var v = this._i2c.WriteOneByte(addr, value);

            // EEPROM need a wait time after a write operation
            if (this._waitTimeAfterWriteOperation > 0)
                TimePeriod.Sleep(this._waitTimeAfterWriteOperation);

            return v;
        }

        public override int ReadByte(int addr)
        {
            return this._i2c.Ready1Byte16BitsCommand((System.Int16)addr);
        }
        
        public override EEPROM_24LCXXX_BUFFER ReadPage(int addr, int len = EEPROM_24LCXXX_BUFFER.PAGE_SIZE)
        {
            var r = new EEPROM_24LCXXX_BUFFER();
            r.Buffer = new byte[len];
            r.Succeeded = this._i2c.ReadBuffer(addr, len, r.Buffer);
            return r;
        }
    }

    /// <summary>
    /// EEPROM_24LC256
    /// Microship 32k EEPROM
    /// 256*1024/8 == 32768 = 32k
    /// </summary>
    public class EEPROM_24LC256 : EEPROM_24LCXXX
    {
        public EEPROM_24LC256(Nusbio nusbio, NusbioGpio sdaPin, NusbioGpio sclPin, int waitTimeAfterWriteOperation = 5, bool debug = false)
            : base(nusbio, sdaPin, sclPin, 256, waitTimeAfterWriteOperation, debug)
        {
            
        }
    }
    /// <summary>
    /// EEPROM_24LC256
    /// Microship 32k EEPROM
    /// 512*1024/8 == 65536 = 64k
    /// </summary>
    public class EEPROM_24LC512 : EEPROM_24LCXXX
    {
        public EEPROM_24LC512(Nusbio nusbio, NusbioGpio sdaPin, NusbioGpio sclPin, int waitTimeAfterWriteOperation = 5, bool debug = false)
            : base(nusbio, sdaPin, sclPin, 512, waitTimeAfterWriteOperation, debug)
        {
            
        }
    }
}
