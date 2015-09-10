/*
   This class is based on the Arduino LiquidCrystal_I2C V2.0 library
   Copyright (C) 2015 MadeInTheUSB.net
   Ported to C# and the Nusbio by Frederic Torres for MadeInTheUSB.net

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
using MadeInTheUSB;
using MadeInTheUSB.GPIO;

using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using size_t = System.Int16;

using MadeInTheUSB.WinUtil;
using MadeInTheUSB.i2c;

namespace MadeInTheUSB.Display
{
    /// <summary>
    /// The class drive an LCD via an I2C chip PCF8574
    /// Where to buy:
    ///     http://www.aliexpress.com/snapshot/6653662565.html?orderId=67164328734214
    /// Detail Information about the LCD + I2C and I2C
    ///     https://www.youtube.com/watch?v=KTDw3Z_amiU
    /// Arduino Reference
    ///     http://playground.arduino.cc/Code/LCDi2c
    /// </summary>
    public class LiquidCrystal_I2C : LiquidCrystalBase
    {
        // Enable bit
        protected int En
        {
            get
            {
                return WinUtil.BitUtil.ParseBinary("B00010000");
            }
        }
        protected int Rw
        {
            get
            {
                return WinUtil.BitUtil.ParseBinary("B00100000");
            }
        }
        protected int Rs
        {
            get
            {
                return WinUtil.BitUtil.ParseBinary("B01000000");
            }
        }

        int _Addr;
        int _displayfunction;
        int _displaycontrol;
        int _displaymode;
        int _numlines;
        int _cols;
        int _rows;
        int _backlightval;

        private I2CEngine _i2c;

        public LiquidCrystal_I2C(Nusbio nusbio, NusbioGpio sdaOutPin, NusbioGpio sclPin, int cols, int rows, byte deviceId = 0x27, bool debug = false)
        {
            this._cols = (uint8_t)cols;
            this._rows = (uint8_t)rows;
            this._nusbio = nusbio;
            this._i2c = new I2CEngine(nusbio, sdaOutPin, sdaOutPin, sclPin, 0, debug);
            this._backlightval = LCD_NOBACKLIGHT;
            this._i2c.DeviceId = deviceId;
        }

        public override void Print(string format, params object[] args)
        {
            var text = String.Format(format, args);
            foreach (var c in text)
            {
                //this.write((uint8_t)c);
            }
        }

        public void Print(int x, int y, string format, params object[] args) 
        {
            this.setCursor(x, y);
            this.Print(format, args);
        }

        void init()
        {
            init_priv();
        }

        void init_priv()
        {
            this._displayfunction = LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS;
            begin(this._cols, this._rows);
        }

        public void begin(int cols, int lines, int dotsize = LCD_5x8DOTS)
        {
            begin((uint8_t)cols, (uint8_t)lines, (uint8_t)dotsize);
        }

        public void begin(uint8_t cols, uint8_t lines, uint8_t dotsize = LCD_5x8DOTS)
        {
            if (lines > 1)
            {
                _displayfunction |= LCD_2LINE;
            }
            _numlines = lines;

            // for some 1 line displays you can select a 10 pixel high font
            if ((dotsize != 0) && (lines == 1))
            {
                _displayfunction |= LCD_5x10DOTS;
            }

            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40ms after power rises above 2.7V
            // before sending commands. Arduino can turn on way befer 4.5V so we'll wait 50
            DelayMicroseconds(50000);

            // Now we pull both RS and R/W low to begin commands
            expanderWrite(_backlightval);	// reset expanderand turn backlight off (Bit 8 =1)
            Delay(1000);

            //put the LCD into 4 bit mode
            // this is according to the hitachi HD44780 datasheet
            // figure 24, pg 46

            // we start in 8bit mode, try to set 4 bit mode
            write4bits(0x03);
            DelayMicroseconds(4500); // wait min 4.1ms

            // second try
            write4bits(0x03);
            DelayMicroseconds(4500); // wait min 4.1ms

            // third go!
            write4bits(0x03);
            DelayMicroseconds(150);

            // finally, set to 4-bit interface
            write4bits(0x02);

            // set # lines, font size, etc.
            command(LCD_FUNCTIONSET | _displayfunction);

            // turn the display on with no cursor or blinking default
            _displaycontrol = LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF;
            display();

            // clear it off
            clear();

            // Initialize to default text direction (for roman languages)
            _displaymode = LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT;

            // set the entry mode
            command(LCD_ENTRYMODESET | _displaymode);

            home();
        }


        /********** high level commands, for the user! */
        public void clear()
        {
            command(LCD_CLEARDISPLAY);// clear display, set cursor position to zero
            DelayMicroseconds(2000);  // this command takes a long time!
        }

        public void home()
        {
            command(LCD_RETURNHOME);  // set cursor position to zero
            DelayMicroseconds(2000);  // this command takes a long time!
        }

        public void setCursor(int col, int row)
        {
            this.setCursor((uint8_t)col, (uint8_t)row);
        }

        public void setCursor(uint8_t col, uint8_t row)
        {
            int[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };
            if (row > _numlines)
            {
                row = (byte)(_numlines - 1);    // we count rows starting w/0
            }
            command(LCD_SETDDRAMADDR | (col + row_offsets[row]));
        }

        // Turn the display on/off (quickly)
        public void noDisplay()
        {
            _displaycontrol &= ~LCD_DISPLAYON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        public void display()
        {
            _displaycontrol |= LCD_DISPLAYON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turns the underline cursor on/off
        public void noCursor()
        {
            _displaycontrol &= ~LCD_CURSORON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        public void cursor()
        {
            _displaycontrol |= LCD_CURSORON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turn on and off the blinking cursor
        public void noBlink()
        {
            _displaycontrol &= (~LCD_BLINKON);
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        public void blink()
        {
            _displaycontrol |= LCD_BLINKON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // These commands scroll the display without changing the RAM
        public void scrollDisplayLeft()
        {
            command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT);
        }
        public void scrollDisplayRight()
        {
            command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT);
        }

        // This is for text that flows Left to Right
        public void leftToRight()
        {
            _displaymode |= LCD_ENTRYLEFT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // This is for text that flows Right to Left
        public void rightToLeft()
        {
            _displaymode &= ~LCD_ENTRYLEFT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // This will 'right justify' text from the cursor
        public void autoscroll()
        {
            _displaymode |= LCD_ENTRYSHIFTINCREMENT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // This will 'left justify' text from the cursor
        public void noAutoscroll()
        {
            _displaymode &= ~LCD_ENTRYSHIFTINCREMENT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // Allows us to fill the first 8 CGRAM locations
        // with custom characters
        public void createChar(uint8_t location, uint8_t[] charmap)
        {
            location &= 0x7; // we only have 8 locations 0-7
            command(LCD_SETCGRAMADDR | (location << 3));
            for (int i = 0; i < 8; i++)
            {
                write(charmap[i]);
            }
        }

        // Turn the (optional) backlight off/on
        public void noBacklight()
        {
            _backlightval = LCD_NOBACKLIGHT;
            expanderWrite(0);
        }

        void backlight()
        {
            _backlightval = LCD_BACKLIGHT;
            expanderWrite(0);
        }

        /*********** mid level commands, for sending data/cmds */

        private void command(int value)
        {
            command((uint8_t)value);
        }

        private void command(uint8_t value)
        {
            send((byte)value, (byte)0);
        }

        private size_t write(int value)
        {
            return write((uint8_t)value);
        }
        private size_t write(uint8_t value)
        {
            send(value, Rs);
            return 0;
        }

        /************ low level data pushing commands **********/

        void send(int value, int mode)
        {

            send((uint8_t)value, (uint8_t)mode);
        }
        // write either command or data
        void send(uint8_t value, uint8_t mode)
        {

            uint8_t highnib = (uint8_t)(value >> 4);
            uint8_t lownib = (uint8_t)(value & 0x0F);
            write4bits((highnib) | mode);
            write4bits((lownib) | mode);
        }

        void write4bits(int value)
        {
            write4bits((uint8_t)value);
        }
        void write4bits(uint8_t value)
        {

            expanderWrite(value);
            pulseEnable(value);
        }

        bool expanderWrite(int _data)
        {
            return expanderWrite((uint8_t)_data);
        }

        bool expanderWrite(uint8_t _data)
        {
            //Wire.beginTransmission(_Addr);
            //Wire.write((int)(_data) | _backlightval);
            //Wire.endTransmission();   
            // here
            return this._i2c.Send1ByteCommand((uint8_t)(_data | _backlightval));
        }

        void pulseEnable(uint8_t _data)
        {
            expanderWrite(_data | En);	// En high
            DelayMicroseconds(1);		// enable pulse must be >450ns

            expanderWrite(_data & ~En);	// En low
            DelayMicroseconds(50);		// commands need > 37us to settle
        }

        // Alias functions

        void cursor_on()
        {
            cursor();
        }

        void cursor_off()
        {
            noCursor();
        }

        void blink_on()
        {
            blink();
        }

        void blink_off()
        {
            noBlink();
        }

        void load_custom_character(uint8_t char_num, uint8_t[] rows)
        {

            createChar(char_num, rows);
        }

        void setBacklight(uint8_t new_val)
        {
            if (new_val > 0)
            {
                backlight();		// turn backlight on
            }
            else
            {
                noBacklight();		// turn backlight off
            }
        }

        void printstr(char[] c)
        {
            //This function is not identical to the function used for "real" I2C displays
            //it's here so the user sketch doesn't have to be changed 
            //print(c);
        }


        // unsupported API functions
        void off()
        {
            throw new NotImplementedException();
        }
        void on()
        {
            throw new NotImplementedException();
        }
        void setDelay(int cmdDelay, int charDelay)
        {
            throw new NotImplementedException();
        }
        uint8_t status()
        {
            return 0;
        }
        uint8_t keypad()
        {
            return 0;
        }
        uint8_t init_bargraph(uint8_t graphtype)
        {
            return 0;
        }
        void draw_horizontal_graph(uint8_t row, uint8_t column, uint8_t len, uint8_t pixel_col_end)
        {
            throw new NotImplementedException();
        }
        void draw_vertical_graph(uint8_t row, uint8_t column, uint8_t len, uint8_t pixel_row_end)
        {
            throw new NotImplementedException();
        }
        void setContrast(uint8_t new_val)
        {
            throw new NotImplementedException();
        }



    }
}
