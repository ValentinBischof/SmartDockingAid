using UnityEngine;

// Modifies DockingAidAutopilot to be active if this PartModule is presence on the current vessel 

namespace SmartDockingAid
{
    class ModuleDockingAid : PartModule
    {
        public override string GetInfo()
        {
            string info = $"This Probe Core has the abillity to set following additonal SAS Modes: \n Parallel - \n Parallel +";
            return info;
        }
    }
}
