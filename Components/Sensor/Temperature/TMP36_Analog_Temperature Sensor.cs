/*
    Written by FT for MadeInTheUSB
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
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.Sensor
{
    public class Tmp36AnalogTemperatureSensor : AnalogTemperatureSensor
    {
        public Tmp36AnalogTemperatureSensor(Nusbio nusbio) : base(nusbio)
        {
            
        }

        public virtual void SetAnalogValue(double value)
        {
            base.SetAnalogValue(value);
            base.Voltage      = value * base.ReferenceVoltage;
            base.Voltage     /= 1024.0;
            this._celsiusValue = (Voltage - 0.5) * 100; 
        }

        public bool Begin()
        {
            return base.Begin();
        }

        public virtual double GetTemperature(TemperatureType type = TemperatureType.Celsius)
        {
            switch (type)
            {
                case TemperatureType.Celsius: return this._celsiusValue;
                case TemperatureType.Fahrenheit: return CelsiusToFahrenheit(GetTemperature(TemperatureType.Celsius));
                case TemperatureType.Kelvin: return CelsiusToKelvin(GetTemperature(TemperatureType.Celsius));
                default:
                    throw new ArgumentException();
            }
        }

    }
}

