using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartDockingAid.Extensions
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class OverwriteModuleText : MonoBehaviour
    {
        public void Start()
        {
            List<AvailablePart> parts = PartLoader.LoadedPartsList.Where(p => p.partPrefab.Modules.GetModule<ModuleSAS>()).ToList();

            foreach (AvailablePart part in parts)
            {
                ModuleSAS SASmodule = part.partPrefab.FindModuleImplementing<ModuleSAS>();
                AvailablePart.ModuleInfo moduleInfo = part.moduleInfos.Where(m => m.moduleName == "SAS").First();
                int serviceLevel = (SASmodule.SASServiceLevel + 1);
                string moduleText = string.Empty;
                for (int i = 0; i < serviceLevel; i++)
                    moduleText += $"{SASLevels[i]} \n";

                if (part.partPrefab.HasModuleImplementing<ModuleDockingAid>())
                    moduleText += $"{SASLevels[4]} \n";

                moduleInfo.info = moduleText;
            }
        }

        private string[] SASLevels = new string[5]
        {
            $"S0: Stability Assist",
            $"S1: Prograde / Retrograde",
            $"S2: Radial / Normal",
            $"S3: Maneuver / Target",
            $"S{AssetLoader.minPilotLevel}: Parallel+ / Parallel-"
        };
    }
}
