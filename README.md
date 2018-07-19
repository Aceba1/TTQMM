### Installation:

File and details available at 

https://www.nexusmods.com/terratech/mods/1/
___

### Note to developers

To support your mod for the QMods system, you need to learn how `mod.json` is implemented (or will be, once complete). The critical keys are:  

```csharp
{
  "Id": "id.ShouldNotContainAnySpaces",
  "DisplayName": "The display name of your mod. This should be formatted",
  "Author": "The name of the author",
  "Version": "1.0.0",
  "Requires":[], // You can leave this out
  "Enable": true,
  "AssemblyName": "dllName.dll",
  "EntryMethod": "YOURNAMESPACE.QPatch.Patch", // Look below
  "Config": {}
}
```

`AssemblyName` must be the case sensitive name of the dll file containing your patching method

`EntryMethod` is the entry method for your patch

```cs
using Harmony;

namespace YOURNAMESPACE
{
    class QPatch()
    {
        public static void Patch()
        {
            // Harmony.PatchAll() or equivalent
        }
    }
}
```
