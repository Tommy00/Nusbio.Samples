<#
	Nusbio Library For PowerShell

    Copyright (C) 2015 MadeInTheUSB.net
    Written by Frederic Torres for MadeInTheUSB.net

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
 
$MadeInTheUSB_Nusbio_Lib_dll  = "..\Components\MadeInTheUSB.Nusbio.Lib.dll"
$Script:POWERSHELL_VERSION    = "3.0"
$script:__WebServerUrlEvent__ = $null

function Nusbio_UnregisterWebServerUrlEvent() {

    if($script:__WebServerUrlEvent__ -ne $null) {
        
        "Dispose __WebServerUrlEvent__ $($script:__WebServerUrlEvent__.Id)"
        Unregister-Event $script:__WebServerUrlEvent__.Id
        $script:__WebServerUrlEvent__ = $null
    }
}

function Nusbio_RegisterWebServerUrlEvent {
    param (
        $nusbio,
        [ScriptBlock] $scriptBlock = $(throw "Parameter -scriptBlock is required.")
    )
    #"Register __WebServerUrlEvent__ "
    $script:__WebServerUrlEvent__ = Register-ObjectEvent -InputObject $nusbio -EventName "UrlEvent" -Action $scriptBlock #{ param($s) Write-Host ("HTTP:" + $s) }
}

function pUsing {
    param (
        [System.IDisposable] $obj = $(throw "Parameter -obj is required."),
        [ScriptBlock] $scriptBlock = $(throw "Parameter -scriptBlock is required.")
    )
    Try { 
        &$scriptBlock
    }
    Finally {
        if ($obj -ne $null) {
            if ($obj.psbase -eq $null) {
                #"Nusbio $($obj.SerialNumber) disposed"
                UnregisterWebServerUrlEvent
                $obj.Dispose()
            } else {
                #"Nusbio $($obj.SerialNumber) disposed."
                Nusbio_UnregisterWebServerUrlEvent
                $obj.psbase.Dispose()
            }
        }
    }
}

function Nusbio_CheckVersion() {

    $t = $PSVersionTable
    Write-Host "PowerShell:$($t.PSVersion), 64BitProcess:$([Environment]::Is64BitProcess), CLR:$($t.CLRVersion), PSCompatibleVersions:$($t.PSCompatibleVersions), Host:$((Get-Host).Version)"
    if($t.PSVersion -lt $Script:POWERSHELL_VERSION) {
        Write-Error "Invalid PowerShell Version"
        Sleep -Seconds 3
        Exit 1
    }
}

function Nusbio_AddGpioProperties($nusbio) {

    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio0" -Value $nusbio.GetGPIO("Gpio0")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio1" -Value $nusbio.GetGPIO("Gpio1")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio2" -Value $nusbio.GetGPIO("Gpio2")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio3" -Value $nusbio.GetGPIO("Gpio3")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio4" -Value $nusbio.GetGPIO("Gpio4")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio5" -Value $nusbio.GetGPIO("Gpio5")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio6" -Value $nusbio.GetGPIO("Gpio6")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio7" -Value $nusbio.GetGPIO("Gpio7")
}

Write-Host "Nusbio PowerShell Module Initialization"

Nusbio_CheckVersion

Write-Host "Loading $MadeInTheUSB_Nusbio_Lib_dll"
Add-Type -Path $MadeInTheUSB_Nusbio_Lib_dll

[MadeInTheUSB.Devices]::Initialize()

# Create variable to manipulate C# Enum type from the library
$Gpio0 = [MadeInTheUSB.NusbioGpio]::Gpio0
$Gpio1 = [MadeInTheUSB.NusbioGpio]::Gpio1
$Gpio2 = [MadeInTheUSB.NusbioGpio]::Gpio2
$Gpio3 = [MadeInTheUSB.NusbioGpio]::Gpio3
$Gpio4 = [MadeInTheUSB.NusbioGpio]::Gpio4
$Gpio5 = [MadeInTheUSB.NusbioGpio]::Gpio5
$Gpio6 = [MadeInTheUSB.NusbioGpio]::Gpio6
$Gpio7 = [MadeInTheUSB.NusbioGpio]::Gpio7
$High  = [MadeInTheUSB.GPIO.PinState]::High
$Low   = [MadeInTheUSB.GPIO.PinState]::Low

Export-ModuleMember -function pUsing, Nusbio_RegisterWebServerUrlEvent, Nusbio_AddGpioProperties
Export-ModuleMember -Variable $Gpio0, $Gpio1, $Gpio2, $Gpio3, $Gpio4, $Gpio5, $Gpio6, $Gpio7
Export-ModuleMember -Variable $High, $Low
#Export-ModuleMember -Variable $script:__WebServerUrlEvent__ 
