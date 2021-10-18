# Space Engineers Scripting

Here are the steps to set up Visual Studio Code for Space Engineers ingame
script development.  

This will provide an environment where Autocompletion / Intellisense works for the Space Engineers API.  

## Setup

* I'll assume you already have SE installed!  
* Install Visual Studio Code (VSC)  
* Install the following VSC Extensions:  
  * c# extension in VSC (OmniSharp)  
  * NuGet Package manager VSC Extension  
* install .net cli tools / SDK (you may not need those)
* install net framework dev-pack 4.6.1
  * https://www.microsoft.com/en-us/download/confirmation.aspx?id=49978
* Then clone this repository.

## Configuration and operation

* If you have a non-standard SE location, change `SpaceEngineers.csproj` to link to your location
* 'open folder' (this folder) from VSC.
* You can use the 'script_manager.sh' if you have WSL/Linux installed. It creates new files and pushes/pulls scripts from your SE Folder. 
  * Otherwise you will have to do it manually or re-create the script for batch/powershell. (I won't)
  To create a new Script manually copy the `default` folder into the Scripts folder and rename it.
  > DO NOT rename the script itself!
  * Secondly you need to change the namespace of the script, otherwise it will conflict with the `default`.
* Something you always have to do manually is to add your Script to the `SEScripts.csproj` file. There is a section with a list of `<Compile Include="Scripts/[Script_Name].cs />`. Add your Script to those. Omnisharp will only provide Code completion and Intellisense when this Include was added.
* Put your code changes between the region tags (see comment in template)


Credit to https://github.com/mrdaemon for the working `.csproj` file.

