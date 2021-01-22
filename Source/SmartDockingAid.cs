using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.UI.TooltipTypes;
using KSP.UI.Screens.Flight;
using SmartDockingAid.UI;
using System;

namespace SmartDockingAid
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SmartDockingAid : MonoBehaviour
    {
        private const string DISPLAYNAME = "SmartDockingAid";

        private UIStateToggleButton[] modebuttons;

        private UIStateToggleButton.ButtonState buttonActive;
        private UIStateToggleButton.ButtonState buttonDisabled;

        private UIStateToggleButton parallelPlus;
        private UIStateToggleButton parallelNegative;

        private Vessel vessel;
        private VesselDockingAid vesselDockingAid;

        private bool autopilotState;
        private bool buttonInit = false;
        private bool SASstate;

        public enum TargetMode
        {
            OFF,
            PARALLEL_PLUS,
            PARALLEL_NEGATIVE
        }

        public void Start()
        {
            GameEvents.onFlightReady.Add(onFlightReady);
            GameEvents.onVesselSwitching.Add(onVesselChange);
            GameEvents.onDockingComplete.Add(onDockingComplete);
            GameEvents.onGameSceneSwitchRequested.Add(onGameScenceSwitch);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
        }

        private void onFlightReady()
        {
            if (!buttonInit)
            {
                modebuttons = FindObjectOfType<VesselAutopilotUI>().modeButtons;
                modebuttons[0].getSpriteStates(out buttonActive, out buttonDisabled);

                foreach (UIStateToggleButton button in modebuttons)
                {
                    button.onClick.AddListener(delegate { onSASbuttonPressed(button); });
                }

                GameObject parallelNegative = Instantiate(modebuttons[5].gameObject);
                GameObject parallelPlus = Instantiate(modebuttons[5].gameObject);

                parallelNegative.SetActive(true);
                parallelPlus.SetActive(true);

                this.parallelNegative = parallelNegative.GetComponent<UIStateToggleButton>();
                this.parallelNegative.onClick.RemoveAllListeners();
                this.parallelNegative.onClick.AddListener(delegate { onToggleButtonPressed(this.parallelNegative); });
                this.parallelNegative.stateTrue = buttonActive;
                this.parallelNegative.stateFalse = buttonDisabled;
                this.parallelNegative.name = "ParallelNegative";
                this.parallelNegative.SetState(false);
                this.parallelNegative.interactable = false;
                this.parallelNegative.transform.position = modebuttons[7].transform.position;
                this.parallelNegative.transform.localScale = new Vector3(1 * GameSettings.UI_SCALE * GameSettings.UI_SCALE_NAVBALL, 1 * GameSettings.UI_SCALE * GameSettings.UI_SCALE_NAVBALL);
                this.parallelNegative.transform.position = new Vector3(parallelNegative.gameObject.transform.position.x + ((4 * GameSettings.UI_SCALE_NAVBALL) * GameSettings.UI_SCALE), parallelNegative.gameObject.transform.position.y - ((25 * GameSettings.UI_SCALE_NAVBALL) * GameSettings.UI_SCALE));

                this.parallelPlus = parallelPlus.GetComponent<UIStateToggleButton>();
                this.parallelPlus.onClick.RemoveAllListeners();
                this.parallelPlus.onClick.AddListener(delegate { onToggleButtonPressed(this.parallelPlus); });
                this.parallelPlus.stateTrue = buttonActive;
                this.parallelPlus.stateFalse = buttonDisabled;
                this.parallelPlus.name = "ParallelPlus";
                this.parallelPlus.SetState(false);
                this.parallelPlus.interactable = false;
                this.parallelPlus.transform.position = modebuttons[8].transform.position;
                this.parallelPlus.transform.localScale = new Vector3(1 * GameSettings.UI_SCALE * GameSettings.UI_SCALE_NAVBALL , 1 * GameSettings.UI_SCALE  * GameSettings.UI_SCALE_NAVBALL);
                this.parallelPlus.transform.position = new Vector3(parallelPlus.gameObject.transform.position.x + ((4 * GameSettings.UI_SCALE_NAVBALL) * GameSettings.UI_SCALE), parallelPlus.gameObject.transform.position.y - ((25 * GameSettings.UI_SCALE_NAVBALL) * GameSettings.UI_SCALE));

                parallelNegative.GetComponent<TooltipController_Text>().textString = "Parallel -";
                parallelNegative.GetChild("Image").GetComponent<Image>().sprite = AssetLoader.parallelMinus.toSprite();
                parallelNegative.transform.SetParent(UnityEngine.Object.FindObjectOfType<VesselAutopilotUI>().transform);
                parallelPlus.GetComponent<TooltipController_Text>().textString = "Parallel +";
                parallelPlus.GetChild("Image").GetComponent<Image>().sprite = AssetLoader.parallelPlus.toSprite(); 
                parallelPlus.transform.SetParent(UnityEngine.Object.FindObjectOfType<VesselAutopilotUI>().transform);

                buttonInit = true;

                Debug.Log($"[{DISPLAYNAME}] UI initiated");
            }

            vessel = FlightGlobals.ActiveVessel;
            SetNewState(false, true);
        }

        private void SetNewState(bool disable, bool reset)
        {           
            if (reset)
            {
                vesselDockingAid = vessel.GetComponent<VesselDockingAid>() as VesselDockingAid;
                autopilotState = vesselDockingAid.Setup();
                SASstate = vessel.Autopilot.Enabled;
            }
            
            if (autopilotState && !disable)
            {
                parallelPlus.gameObject.SetActive(true);
                parallelNegative.gameObject.SetActive(true);

                switch(vesselDockingAid.targetMode)
                {
                    case TargetMode.OFF:
                        modebuttons[0].changeState(buttonActive);
                        parallelPlus.SetState(false);
                        parallelNegative.SetState(false);
                        break;
                    case TargetMode.PARALLEL_PLUS:
                        modebuttons[0].changeState(buttonDisabled);
                        parallelNegative.SetState(false);
                        parallelPlus.SetState(true);
                        break;
                    case TargetMode.PARALLEL_NEGATIVE:
                        modebuttons[0].changeState(buttonDisabled);
                        parallelNegative.SetState(true);
                        parallelPlus.SetState(false);
                        break;
                }                  
            }
            else
            {
                parallelNegative.gameObject.SetActive(false);
                parallelPlus.gameObject.SetActive(false);
            }
        }

        private void onVesselChange(Vessel vessel1, Vessel vessel2)
        {
            vessel = vessel2 != null ? vessel2 : vessel1;
            SetNewState(false, true);
        }

        private void onDockingComplete(GameEvents.FromToAction<Part, Part> part)
        {
            vessel = FlightGlobals.ActiveVessel;
            SetNewState(true, true);
        }

        private void OnGameSettingsApplied()
        { 
            SetNewState(false, true); 
        }

        private void onToggleButtonPressed(UIStateToggleButton button)
        {
            modebuttons[0].changeState(buttonDisabled);

            if (button == parallelNegative)
            {
                vesselDockingAid.onModeChange(TargetMode.PARALLEL_NEGATIVE);
                SetNewState(false, false);
            }
            else
            {
                vesselDockingAid.onModeChange(TargetMode.PARALLEL_PLUS);
                SetNewState(false, false);
            }
        }

        private void onSASbuttonPressed(UIStateToggleButton button)
        {
            if (autopilotState)
            {
                parallelNegative.SetState(false);
                parallelPlus.SetState(false);
                vesselDockingAid.onModeChange(TargetMode.OFF);
                SetNewState(false, false);
            }
        }

        private void onButtonInteractableStateChanged()
        {
            parallelNegative.interactable = modebuttons[8].interactable;
            parallelPlus.interactable = modebuttons[8].interactable;

            if (!modebuttons[8].interactable)
            {
                vesselDockingAid.onModeChange(TargetMode.OFF);
                SetNewState(false, false);
            }
        }

        private void onButtonStateChanged()
        {
            parallelNegative.gameObject.SetActive(modebuttons[8].gameObject.activeSelf);
            parallelPlus.gameObject.SetActive(modebuttons[8].gameObject.activeSelf);

            if (!modebuttons[8].gameObject.activeSelf)
            {
                vesselDockingAid.onModeChange(TargetMode.OFF);
            }
        }

        private void onSASStateChanged()
        {
            SASstate = vessel.Autopilot.Enabled;

            if (!vessel.Autopilot.Enabled)
            {
                vesselDockingAid.onModeChange(TargetMode.OFF);
                SetNewState(false, false);
            }           
        }

        public void Update()
        {
            if (autopilotState)
            {
                if (modebuttons[8].gameObject.activeSelf != parallelNegative.gameObject.activeSelf)
                    onButtonStateChanged();

                if (modebuttons[8].interactable != parallelNegative.interactable)
                    onButtonInteractableStateChanged();

                if (vessel.Autopilot.Enabled != SASstate)
                    onSASStateChanged();
            }
        }

        private void onGameScenceSwitch(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            Debug.Log($"[{DISPLAYNAME}] Destroy()");

            parallelNegative.onClick.RemoveAllListeners();
            parallelPlus.onClick.RemoveAllListeners();

            foreach (UIStateToggleButton button in modebuttons)
            {
                button.onClick.RemoveListener(delegate { onSASbuttonPressed(button); });
            }

            Destroy(parallelNegative.gameObject);
            Destroy(parallelPlus.gameObject);

            buttonInit = false;
            vesselDockingAid = null;
            autopilotState = false;
        }

        public void OnDestroy()
        {
            GameEvents.onFlightReady.Remove(onFlightReady);
            GameEvents.onVesselSwitching.Remove(onVesselChange);
            GameEvents.onDockingComplete.Remove(onDockingComplete);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameScenceSwitch);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }
    }
}
