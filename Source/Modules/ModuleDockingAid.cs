using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// Modifies DockingAidAutopilot to be active if this PartModule is presence on the current vessel 

namespace SmartDockingAid
{
    public class ModuleDockingAid : PartModule
    {
        [KSPField]
        public bool active = true;
    } 
}
