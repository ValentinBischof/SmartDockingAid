using System.Collections.Generic;
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

        public void Setup(out bool state)
        {
            pilotAvailable = false;
            moduleAvailable = false;

            crew = vessel.GetVesselCrew();
            foreach (ProtoCrewMember kerbal in crew)
            {
                if (kerbal.experienceTrait.CrewMemberExperienceLevel() > AssetLoader.minPilotLevel)
                {
                    pilotAvailable = true;
                }
            }

            foreach (Part part in vessel.parts)
            {
                if (part.HasModuleImplementing<ModuleDockingAid>())
                { 
                    moduleAvailable = true;
                }
            }


            if (pilotAvailable || moduleAvailable || HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInSandbox)
                state = true;
            else
                state = false;
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

        private void Update()
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
