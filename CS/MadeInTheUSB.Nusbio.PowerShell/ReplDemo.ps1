<#
	Nusbio Demo Using PowerShell

    Copyright (C) 2015 MadeInTheUSB.net

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
 #>
 
Param(
  [Alias("o")]   [string] $operation = "" # edit to edit sourcecode
) 

if($operation.ToLowerInvariant() -eq "sourcecode") {

    powershell_ise.exe "ReplDemo.ps1,..\Components\Nusbio.psm1"
    Exit 0
}

function Help() {
    Cls
    "Nusbio  - SerialNumber:{0}, Description:{1} " -f  $serialNumber, $nusbio.Description
    "Gpio 0) 1) 2) 3) 4) 5) 6) 7)"
    "C)lear W)eb Browser A)ll off  Q)uit"
}

$Nusbio_psm1 = "..\Components\Nusbio.psm1"
Import-Module ($Nusbio_psm1) -Force

$WEB_SERVER_LISTENING_PORT = 1964
$WEB_SERVER_LISTENING_PORT = -1

"Nusbio Initializing"
$serialNumber = [MadeInTheUSB.Nusbio]::Detect()
if($serialNumber -eq $null) {
    Write-Error "Nusbio not detected"
    Exit 1
}


function ReplHelp() {

    Write-Host "Nusbio REPL PowerShell Mode" -ForegroundColor Cyan
    Write-Host "  Nusbio $($global:Nusbio.SerialNumber) Ready" -ForegroundColor DarkCyan
    Write-Host "  Variable: `$nusbio" -ForegroundColor DarkCyan
    Write-Host "  Web Server: http://localhost:$WEB_SERVER_LISTENING_PORT" -ForegroundColor DarkCyan
    Write-Host ""
    Write-Host "REPL Samples:" -ForegroundColor Green
	Write-Host "  PS> `$nusbio[0].DigitalWrite(`$High)" -ForegroundColor DarkGreen
	Write-Host "  PS> `$nusbio[0].DigitalWrite(`$Low)" -ForegroundColor DarkGreen
    Write-Host "  PS> `$nusbio[0].High()" -ForegroundColor DarkGreen
	Write-Host "  PS> `$nusbio[0].Low()" -ForegroundColor DarkGreen	 

    Write-Host ""
    Write-Host "HTTP Samples:" -ForegroundColor Green
    Write-Host "  http://localhost:1964/gpio/0/high" -ForegroundColor DarkGreen
    Write-Host "  Http://localhost:1964/gpio/0/low" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:1964/gpio/0/reverse" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:1964/gpio/all/low" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:1964/gpio/0/blink/500/0" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:1964/gpio/0/blink/1000/100" -ForegroundColor DarkGreen
    Write-Host "  http://localhost:1964/nusbio/state" -ForegroundColor DarkGreen
    Write-Host ""
    Write-Host " CURL.exe Samples:"  -ForegroundColor Green
    Write-Host "    curl.exe -X GET http://localhost:1964/gpio/0/high" -ForegroundColor DarkGreen
    Write-Host "    curl.exe -X GET http://localhost:1964/gpio/1/low" -ForegroundColor DarkGreen
}

#Cls


$global:nusbio = New-Object MadeInTheUSB.Nusbio($serialNumber, 0, $WEB_SERVER_LISTENING_PORT)
Nusbio_RegisterWebServerUrlEvent $global:nusbio { param($s) Write-Host ("HTTP:" + $s) }
ReplHelp

<#

$indexes = 0,1,2,3,4,5,6,7
foreach($x in $indexes) { $nusbio.GetGPIO("Gpio"+$x).High() }
foreach($x in $indexes) { $nusbio[$x].High() }
$gDevice.SetAllGpioLow()

foreach($x in 0,2,4,6)  { $nusbio[$x].ReverseSet() }
foreach($x in 0,2,4,6)  { $nusbio[$x].ReverseSet() }

#>



