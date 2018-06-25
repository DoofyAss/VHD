## Virtual Disk Manager
<sup>.NET Framework 3.5</sup>  
<sup>• **Windows 7 / 8 / 10**</sup>

### Application

<img src="https://github.com/mrblackvein/VHD/blob/master/Screen/app.png">

<img src="https://github.com/mrblackvein/VHD/blob/master/Screen/console.png">

<img src="https://github.com/mrblackvein/VHD/blob/master/Screen/contextMenu.png">

### Library (COM)

Register

```Batch
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm /codebase "C:\VirtualDiskManager, x86.dll"
```

```Batch
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm /codebase "C:\VirtualDiskManager, x64.dll"
```

Unregister

```Batch
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm /u "C:\VirtualDiskManager, x86.dll"
```

```Batch
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm /u "C:\VirtualDiskManager, x64.dll"
```

VBScript Example

```C#
  Set VHD = CreateObject("VirtualDiskManager.VHD")
  
  VHD.Attach("C:\image.vhd")
  VHD.Detach("C:\image.vhd")
  VHD.Toggle("C:\image.vhd")
```

<sub>[vk.com/ShitSoftware](http://vk.com/ShitSoftware) :shit:</sub>