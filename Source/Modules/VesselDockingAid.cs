using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using SmartDockingAid.Flight;

namespace SmartDockingAid
{
    class VesselDockingAid : VesselModule
    {
        public SmartDockingAid.TargetMode targetMode { get; private set; }

        private ITargetable target;

        private List<ProtoCrewMember> crew = new List<ProtoCrewMember>();

        private bool pilotAvailable;
        private bool moduleAvailable;
        private bool active = false;

        public bool Setup()
        {
            pilotAvailable = false;
            moduleAvailable = false;

            pilotAvailable = vessel.GetVesselCrew().Any(c => c.experienceTrait.CrewMemberExperienceLevel() > AssetLoader.minPilotLevel);

            foreach (Part part in vessel.parts)
            {
                if (part.HasModuleImplementing<ModuleDockingAid>())
                {
                    ModuleDockingAid moduleDockingAid = part.Modules.GetModule<ModuleDockingAid>();
                    if (!moduleDockingAid.active) continue;
                    moduleAvailable = true;
                }
            }

            return (pilotAvailable || moduleAvailable || HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInSandbox);              
        }

        public void changeSASstate(bool isOn)
        {           
            if (isOn)
            {
                vessel.Autopilot.Enable();
            }
            else
            {
               vessel.Autopilot.Disable();
            }              
        }


        public void onModeChange(SmartDockingAid.TargetMode targetMode)
        {
            this.targetMode = targetMode;
            if (targetMode != SmartDockingAid.TargetMode.OFF)
            {
                vessel.Autopilot.SetMode(VesselAutopilot.AutopilotMode.StabilityAssist);
                active = true;
            }
            else
            {
                active = false;
                if (vessel.Autopilot.Mode == VesselAutopilot.AutopilotMode.StabilityAssist)
                {
                    vessel.Autopilot.SAS.lockedMode = true;
                }
            }
        }

        public void Update()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && active)
            {
                if (!vessel.IsControllable || !vessel.Autopilot.Enabled)
                    return;

                if (vessel.targetObject != null)
                    target = vessel.targetObject;

                vessel.Autopilot.SAS.lockedMode = false;
                vessel.Autopilot.SAS.SetTargetOrientation(target.getAttitude(targetMode), false);
            }
        }
    }
}
