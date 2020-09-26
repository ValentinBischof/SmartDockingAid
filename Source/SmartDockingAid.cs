using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.UI.TooltipTypes;
using SmartDockingAid.UI;
using System;

namespace SmartDockingAid
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SmartDockingAid : MonoBehaviour
    {
        private UIStateToggleButton[] modebuttons;

        private UIStateToggleButton.ButtonState buttonActive;
        private UIStateToggleButton.ButtonState buttonDisabled;

        private UIStateToggleButton parallelPlus;
        private UIStateToggleButton parallelNegative;
        private UIStateToggleButton lastButton;

        private Vessel vessel;
        private VesselDockingAid module;

        private bool autopilotState;
        private bool buttonInit = false;

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
        }


        private void onFlightReady()
        {
            if (!buttonInit)
            {
                modebuttons = FindObjectOfType<VesselAutopilotUI>().modeButtons;
                modebuttons[0].getSpriteStates(out buttonActive, out buttonDisabled);
                lastButton = modebuttons[0];

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

                Debug.Log("[SmartDockingAid] UI initiated");
            }

            vessel = FlightGlobals.ActiveVessel;
            module = vessel.GetComponent<VesselDockingAid>() as VesselDockingAid;

            if (module == null)
                return;
            module.Setup(out autopilotState);
            if (!autopilotState)
            {
                parallelNegative.gameObject.SetActive(false);
                parallelPlus.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[SmartDockingAid] Active on vessel {vessel.GetDisplayName()}");
            }
        }

        private void onVesselChange(Vessel vessel5, Vessel vessel6)
        {
            if (vessel6 != null)
                this.vessel = vessel6;
            else
                vessel = vessel5;

            module = vessel.GetComponent<VesselDockingAid>() as VesselDockingAid;

            if (module == null)
                return;
            module.Setup(out autopilotState);

            if (autopilotState)
            {
                Debug.Log($"[SmartDockingAid] Active on vessel {vessel.GetDisplayName()}");
                TargetMode mode = module.targetMode;
                if (mode == TargetMode.OFF)
                {
                    modebuttons[0].changeState(buttonActive);
                    lastButton.SetState(false);
                    lastButton = modebuttons[0];
                }
                else if (mode == TargetMode.PARALLEL_NEGATIVE)
                {
                    modebuttons[0].changeState(buttonDisabled);
                    parallelNegative.SetState(true);
                    lastButton.SetState(false);
                    lastButton = parallelNegative;
                    parallelNegative.SetState(UIStateToggleButton.BtnState.True);
                    parallelNegative.SetState(true);
                }
                else if (mode == TargetMode.PARALLEL_PLUS)
                {
                    modebuttons[0].changeState(buttonDisabled);
                    parallelPlus.SetState(true);
                    lastButton.SetState(false);
                    lastButton = parallelPlus;
                    parallelPlus.SetState(true);
                    lastButton.SetState(UIStateToggleButton.BtnState.True);
                }
            }
            else
            {
                parallelNegative.gameObject.SetActive(false);
                parallelPlus.gameObject.SetActive(false);
            }
        }

        private void onDockingComplete(GameEvents.FromToAction<Part, Part> part)
        {
            module = vessel.GetComponent<VesselDockingAid>() as VesselDockingAid;

            if (module == null)
                return;

            module.Setup(out autopilotState);
            if (autopilotState)
            {
                Debug.Log($"[SmartDockingAid] Active on vessel {vessel.GetDisplayName()}");
            }
            module.onModeChange(TargetMode.OFF);
            lastButton.SetState(false);
            lastButton = modebuttons[0];
            modebuttons[0].changeState(buttonActive);
            vessel.Autopilot.Enabled = false;
        }

        private void onToggleButtonPressed(UIStateToggleButton button)
        {
            if (lastButton.name != button.name)
            {
                if (button.name == "ParallelNegative")
                {
                    parallelNegative.SetState(true);
                    lastButton.SetState(false);
                    lastButton = parallelNegative;
                    module.onModeChange(TargetMode.PARALLEL_NEGATIVE);
                }
                if (button.name == "ParallelPlus")
                {
                    parallelPlus.SetState(true);
                    lastButton.SetState(false);
                    lastButton = parallelPlus;
                    module.onModeChange(TargetMode.PARALLEL_PLUS);
                }

                modebuttons[0].changeState(buttonDisabled);
            }
        }

        private void onSASbuttonPressed(UIStateToggleButton button)
        {
            if (autopilotState)
            {
                modebuttons[0].changeState(buttonActive);
                lastButton.SetState(false);
                lastButton = button;
                module.onModeChange(TargetMode.OFF);
            }
        }
        private void onButtonInteractableStateChanged()
        {
            parallelNegative.interactable = modebuttons[8].interactable;
            parallelPlus.interactable = modebuttons[8].interactable;
            if (!modebuttons[8].interactable)
            {
                module.onModeChange(TargetMode.OFF);
            }
        }

        private void onButtonStateChanged()
        {   
            parallelNegative.gameObject.SetActive(modebuttons[8].gameObject.activeSelf);
            parallelPlus.gameObject.SetActive(modebuttons[8].gameObject.activeSelf);
            if (!modebuttons[8].gameObject.activeSelf)
            {
                module.onModeChange(TargetMode.OFF);
            }
        }


        private void Update()
        {
            if (autopilotState)
            {
                if (modebuttons[8].gameObject.activeSelf != parallelNegative.gameObject.activeSelf)
                    onButtonStateChanged();

                if (modebuttons[8].interactable != parallelNegative.interactable)
                    onButtonInteractableStateChanged();
            }
        }

        private void onGameScenceSwitch(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            Debug.Log($"[SmartDockingAid] Destroy()");
            parallelNegative.onClick.RemoveAllListeners();
            parallelPlus.onClick.RemoveAllListeners();

            foreach (UIStateToggleButton button in modebuttons)
            {
                button.onClick.RemoveListener(delegate { onSASbuttonPressed(button); });
            }
            Destroy(parallelNegative.gameObject);
            Destroy(parallelPlus.gameObject);
            buttonInit = false;
            module = null;
            autopilotState = false;
        }

        public void OnDestroy()
        {
            GameEvents.onFlightReady.Remove(onFlightReady);
            GameEvents.onVesselSwitching.Remove(onVesselChange);
            GameEvents.onDockingComplete.Remove(onDockingComplete);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameScenceSwitch);
        }
    }
}
