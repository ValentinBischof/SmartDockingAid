using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartDockingAid
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class AssetLoader : MonoBehaviour
    {
        public static Texture2D parallelMinus = new Texture2D(16, 16, TextureFormat.DXT5, false);
        public static Texture2D parallelPlus = new Texture2D(16, 16, TextureFormat.DXT5, false);
        public static int minPilotLevel = 3;
        //public static GameObject rotSlider;

        public void Awake()
        {
            string path = KSPUtil.ApplicationRootPath + "GameData/SmartDockingAid/";
            AssetBundle asset = AssetBundle.LoadFromFile(path + "Assets/assets.ksp");

            if (asset == null)
            {
                Debug.Log($"[SmartDockingAid] Failed to load bundle");
                Debug.Log($"[SmartDockingAid] {path}");
            }
            else
            {
                parallelMinus = asset.LoadAsset<Texture2D>("ParallelMinus");
                parallelPlus = asset.LoadAsset<Texture2D>("ParallelPlus");
                //rotSlider = textures.LoadAsset("SliderPanel") as GameObject;

                Debug.Log($"[SmartDockingAid] Bundle loaded sucessfully");
                Debug.Log($"[SmartDockingAid] {path}");
            }

            if (GameDatabase.Instance.ExistsConfigNode("SmartDockingAid/SDASETTINGS"))
            {
                ConfigNode node = GameDatabase.Instance.GetConfigNode("SmartDockingAid/SDASETTINGS");
                if (node.HasValue("minPilotExperience"))
                {
                    if (Int32.TryParse(node.GetValue("minPilotExperience"), out minPilotLevel))
                    {
                        Debug.Log($"[SmartDockingAid] Settings applied");
                    }
                    else
                    {
                        Debug.Log($"[SmartDockingAid] minPilotExperience is not a valid number");
                        Debug.Log($"[SmartDockingAid] Default settings will be applied");
                    }
                }
            }
            else
            {
                Debug.Log($"[SmartDockingAid] Settings file could not be located");
                Debug.Log($"[SmartDockingAid] Default settings will be applied");
            }            
        }
    }
}
