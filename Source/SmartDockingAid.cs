using System;
using System.Collections;
using KSP.UI;
using KSP.UI.TooltipTypes;
using SmartDockingAid.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SmartDockingAid
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SmartDockingAid : MonoBehaviour
    {
        private static Sprite _parallelMinusSprite;
        private static Sprite _parallelPlusSprite;
        private static VesselAutopilot.AutopilotMode? _availableAtSASMode;


        public static Sprite ParallelMinusSprite
        {
            get
            {
                if (_parallelMinusSprite == null)
                {
                    Texture2D tex = GameDatabase.Instance.GetTexture("SmartDockingAid/Assets/ParallelMinus", false);
                    if (tex == null)
                    {
                        tex = new Texture2D(16, 16);
                        Debug.LogError($"[SmartDockingAid] Failed to load texture \"SmartDockingAid/Assets/ParallelMinus\"");
                    }

                    _parallelMinusSprite = tex.toSprite();
                }
                
                return _parallelMinusSprite;
            }
        }

        public static Sprite ParallelPlusSprite
        {
            get
            {
                if (_parallelPlusSprite == null)
                {
                    Texture2D tex = GameDatabase.Instance.GetTexture("SmartDockingAid/Assets/ParallelPlus", false);
                    if (tex == null)
                    {
                        tex = new Texture2D(16, 16);
                        Debug.LogError($"[SmartDockingAid] Failed to load texture \"SmartDockingAid/Assets/ParallelPlus\"");
                    }

                    _parallelPlusSprite = tex.toSprite();
                }

                return _parallelPlusSprite;
            }
        }

        public static VesselAutopilot.AutopilotMode AvailableAtSASMode
        {
            get
            {
                if (_availableAtSASMode == null)
                {
                    ConfigNode node = GameDatabase.Instance.GetConfigNode("SmartDockingAid/SDASETTINGS");
                    if (node == null)
                    {
                        Debug.LogWarning($"[SmartDockingAid] Settings file could not be located");
                    }
                    else
                    {
                        string val = node.GetValue("availableAtSASMode");
                        if (string.IsNullOrEmpty(val) || !Enum.TryParse(val, out VesselAutopilot.AutopilotMode mode))
                            Debug.LogWarning($"[SmartDockingAid] No valid \"minPilotExperience\" value found in settings file");
                        else
                            _availableAtSASMode = mode;
                    }

                    if (_availableAtSASMode == null)
                    {
                        _availableAtSASMode = VesselAutopilot.AutopilotMode.Target;
                        Debug.Log($"[SmartDockingAid] Default settings will be applied");
                    }
                }

                return (VesselAutopilot.AutopilotMode)_availableAtSASMode;
            }
        }

        public static int SASLevel
        {
            get
            {
                switch (AvailableAtSASMode)
                {
                    case VesselAutopilot.AutopilotMode.StabilityAssist:
                        return 0;
                    case VesselAutopilot.AutopilotMode.Prograde:
                    case VesselAutopilot.AutopilotMode.Retrograde:
                        return 1;
                    case VesselAutopilot.AutopilotMode.Normal:
                    case VesselAutopilot.AutopilotMode.Antinormal:
                    case VesselAutopilot.AutopilotMode.RadialIn:
                    case VesselAutopilot.AutopilotMode.RadialOut:
                        return 2;
                    case VesselAutopilot.AutopilotMode.Target:
                    case VesselAutopilot.AutopilotMode.AntiTarget:
                    case VesselAutopilot.AutopilotMode.Maneuver:
                        return 3;
                    default:
                        return 0;
                }
            }
        }

        private UIStateToggleButton[] modebuttons;

        private UIStateToggleButton.ButtonState buttonActive;
        private UIStateToggleButton.ButtonState buttonDisabled;

        private UIStateToggleButton parallelPlus;
        private UIStateToggleButton parallelNegative;

        private VesselModuleDockingAid VesselModule => FlightGlobals.ActiveVessel.FindVesselModuleImplementing<VesselModuleDockingAid>();

        private bool isAvailable;
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
            GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.onDockingComplete.Add(onDockingComplete);
            GameEvents.onGameSceneSwitchRequested.Add(onGameScenceSwitch);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.onKerbalLevelUp.Add(OnKerbalLevelUp);
        }

        private void onFlightReady()
        {
            if (!buttonInit)
            {
                VesselAutopilotUI autopilotUI = FlightUIModeController.Instance.navBall.GetComponentInChildren<VesselAutopilotUI>();
                modebuttons = autopilotUI.modeButtons;
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
                parallelNegative.GetChild("Image").GetComponent<Image>().sprite = ParallelMinusSprite;
                parallelNegative.transform.SetParent(autopilotUI.transform);
                parallelPlus.GetComponent<TooltipController_Text>().textString = "Parallel +";
                parallelPlus.GetChild("Image").GetComponent<Image>().sprite = ParallelPlusSprite; 
                parallelPlus.transform.SetParent(autopilotUI.transform);

                buttonInit = true;
            }

            SetNewState(true);
        }

        private void SetNewState(bool reset)
        {           
            if (reset)
            {
                SASstate = FlightGlobals.ActiveVessel.Autopilot.Enabled;
                isAvailable = AvailableAtSASMode.AvailableAtLevel(FlightGlobals.ActiveVessel);
            }
            
            if (isAvailable)
            {
                parallelPlus.gameObject.SetActive(true);
                parallelNegative.gameObject.SetActive(true);

                switch(VesselModule.targetMode)
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
                modebuttons[0].changeState(buttonActive);
                parallelPlus.SetState(false);
                parallelNegative.SetState(false);
                parallelNegative.gameObject.SetActive(false);
                parallelPlus.gameObject.SetActive(false);
            }
        }

        private void onVesselChange(Vessel vessel)
        {
            SetNewState(true);
        }

        private void onDockingComplete(GameEvents.FromToAction<Part, Part> part)
        {
            SetNewState(true);
        }

        private void OnGameSettingsApplied()
        { 
            SetNewState(true); 
        }

        private void OnKerbalLevelUp(ProtoCrewMember data)
        {
            StartCoroutine(OnKerbalLevelUpDelayed());
        }

        private IEnumerator OnKerbalLevelUpDelayed()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            SetNewState(true);
        }

        private void onToggleButtonPressed(UIStateToggleButton button)
        {
            modebuttons[0].changeState(buttonDisabled);

            if (button == parallelNegative)
            {
                VesselModule.onModeChange(TargetMode.PARALLEL_NEGATIVE);
                SetNewState(false);
            }
            else
            {
                VesselModule.onModeChange(TargetMode.PARALLEL_PLUS);
                SetNewState(false);
            }
        }

        private void onSASbuttonPressed(UIStateToggleButton button)
        {
            if (isAvailable)
            {
                parallelNegative.SetState(false);
                parallelPlus.SetState(false);
                VesselModule.onModeChange(TargetMode.OFF);
                SetNewState(false);
            }
        }

        private void onButtonInteractableStateChanged()
        {
            parallelNegative.interactable = modebuttons[8].interactable;
            parallelPlus.interactable = modebuttons[8].interactable;

            if (!modebuttons[8].interactable)
            {
                VesselModule.onModeChange(TargetMode.OFF);
                SetNewState(false);
            }
        }

        private void onButtonStateChanged()
        {
            parallelNegative.gameObject.SetActive(modebuttons[8].gameObject.activeSelf);
            parallelPlus.gameObject.SetActive(modebuttons[8].gameObject.activeSelf);

            if (!modebuttons[8].gameObject.activeSelf)
            {
                VesselModule.onModeChange(TargetMode.OFF);
            }
        }

        private void onSASStateChanged()
        {
            SASstate = FlightGlobals.ActiveVessel.Autopilot.Enabled;

            if (!FlightGlobals.ActiveVessel.Autopilot.Enabled)
            {
                VesselModule.onModeChange(TargetMode.OFF);
                SetNewState(false);
            }           
        }

        public void Update()
        {
            if (isAvailable)
            {
                if (modebuttons[8].gameObject.activeSelf != parallelNegative.gameObject.activeSelf)
                    onButtonStateChanged();

                if (modebuttons[8].interactable != parallelNegative.interactable)
                    onButtonInteractableStateChanged();

                if (FlightGlobals.ActiveVessel.Autopilot.Enabled != SASstate)
                    onSASStateChanged();
            }
        }

        private void onGameScenceSwitch(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            parallelNegative.onClick.RemoveAllListeners();
            parallelPlus.onClick.RemoveAllListeners();

            foreach (UIStateToggleButton button in modebuttons)
            {
                button.onClick.RemoveListener(delegate { onSASbuttonPressed(button); });
            }

            Destroy(parallelNegative.gameObject);
            Destroy(parallelPlus.gameObject);

            buttonInit = false;
            isAvailable = false;
        }

        public void OnDestroy()
        {
            GameEvents.onFlightReady.Remove(onFlightReady);
            GameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onDockingComplete.Remove(onDockingComplete);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameScenceSwitch);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            GameEvents.onKerbalLevelUp.Remove(OnKerbalLevelUp);
        }
    }
}
