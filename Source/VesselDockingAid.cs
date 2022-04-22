using SmartDockingAid.Flight;

namespace SmartDockingAid
{
    class VesselModuleDockingAid : VesselModule
    {
        public SmartDockingAid.TargetMode targetMode { get; private set; }

        private bool active = false;

        public override bool ShouldBeActive()
        {
            return vessel.loaded && vessel.IsControllable && vessel.Autopilot.Enabled;
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
            if (active && vessel.targetObject != null)
            {
                vessel.Autopilot.SAS.lockedMode = false;
                vessel.Autopilot.SAS.SetTargetOrientation(vessel.targetObject.getAttitude(targetMode), false);
            }
        }
    }
}
