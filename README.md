![image](https://i.imgur.com/LWJbNG3.png "")

Smart Docking Aid adds two new SAS modes to level 3 probe cores and level 3 pilots. Most KSP-players know the issue with aligning two docking ports correctly while docking two ships or station parts together. Smart Docking Aid's new mode aligns the player controlled ship's docking port to the target docking port. So you only have to translate your ship in front of the target docking port and move forward until it docks. There is no more weird camara movement nessassary to see if you will dock properly or ram your target out of orbit.

![https://i.imgur.com/GCsuwWk.png](https://i.imgur.com/GCsuwWk.png "")

### License
This work is licensed under the Attribution-NonCommercial-ShareAlike 4.0 International license (CC BY-NC-SA 4.0)

### Building
After loading the solution in your IDE, add a `ReferencePath` to the root of your KSP install :

For Visual Studio, right-click on the `SmartDockingAid` project > `Properties` > `Reference Paths`, then save, close and re-open the solution for the changes to propagate.

Alternatively, create a `SmartDockingAid.csproj.user` file in the `Source` folder, with the following content :
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ReferencePath>Absolute\Path\To\Your\KSP\Install\Folder</ReferencePath>
  </PropertyGroup>
</Project>
```
Then close / re-open the solution.