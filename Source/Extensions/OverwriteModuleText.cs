using Experience.Effects;
using KSP.Localization;
using UnityEngine;

namespace SmartDockingAid.Extensions
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class OverwriteModuleText : MonoBehaviour
    {
        public void Start()
        {
            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                if (part.partPrefab == null)
                    continue;

                ModuleSAS sasModule = part.partPrefab.FindModuleImplementing<ModuleSAS>();
                if (sasModule == null)
                    continue;

                int sasServiceLevel = sasModule.SASServiceLevel;
                int sdaRequiredLevel = SmartDockingAid.SASLevel;
                if (sasServiceLevel < sdaRequiredLevel)
                    continue;

                AvailablePart.ModuleInfo moduleInfo = part.moduleInfos.Find(p => p.moduleName == "SAS");
                if (moduleInfo == null)
                    continue;

                string text = string.Empty;
                for (int i = 0; i < Mathf.Min(sasServiceLevel + 1, AutopilotSkill.SkillsReadable.Length); i++)
                {
                    if (i != 0)
                        text += "\n";

                    if (i == sdaRequiredLevel)
                        text += "Parallel to target\n";

                    text += AutopilotSkill.SkillsReadable[i];
                }
                if (sasModule.standalone)
                    text += sasModule.resHandler.PrintModuleResources();

                if (!sasModule.moduleIsEnabled)
                    text += Localizer.Format("#autoLOC_218888");

                moduleInfo.info = text;
            }
        }
    }
}
