using UnityEngine;
using KSP.UI;
using ButtonState = KSP.UI.UIStateToggleButton.ButtonState;

namespace SmartDockingAid.UI
{
    public static class ExtensionsUI
    {
        /// <summary>
        /// Outputs both UIStateToggleButton Button States (stateTrue = buttonActive, stateFalse = buttonDisabled) 
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonActive"></param>
        /// <param name="buttonDisabled"></param>
        public static void getSpriteStates(this UIStateToggleButton button, out ButtonState buttonActive, out ButtonState buttonDisabled)
        {
            buttonActive = button.stateTrue;
            buttonDisabled = button.stateFalse;
        }

        /// <summary>
        /// Offsets position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset-x"></param>
        /// <param name="offset-y"></param>
        /// <returns></returns>
        public static Vector3 positionOffset(this Vector3 position, int x, int y)
        {
            return new Vector3(position.x + x, position.y + y);
        }

        /// <summary>
        /// Converts Texture2D to Sprite
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static Sprite toSprite(this Texture2D texture2D)
        {
            return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Exchanges UIStateToggleButton Button States
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonState"></param>
        public static void changeState(this UIStateToggleButton button, ButtonState buttonState)
        {
            button.stateTrue = buttonState;
            button.SetState(UIStateToggleButton.BtnState.True);
        }
    }
}
