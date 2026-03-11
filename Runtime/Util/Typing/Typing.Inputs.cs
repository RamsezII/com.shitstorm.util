using UnityEngine;

namespace _UTIL_
{
    public static partial class Typing
    {
        static float holdtimer;
        const byte holdspeed1 = 7, holdspeed2 = 20;

        public static bool getCtrl_hold => 
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || 
            Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

        public static bool getCtrlV_down => Input.GetKeyDown(KeyCode.V) && getCtrl_hold;

        public static bool getShift_hold =>
            Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        public static bool getReturn_down => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

        //----------------------------------------------------------------------------------------------------------

        public static bool GetKeyHold(KeyCode key)
        {
            if (Input.GetKeyDown(key))
            {
                holdtimer = -1;
                return true;
            }
            else if (Input.GetKey(key))
            {
                if (holdtimer < 0)
                    holdtimer += Time.unscaledDeltaTime * holdspeed1;
                else
                {
                    holdtimer += Time.unscaledDeltaTime * holdspeed2;

                    if (holdtimer >= 1)
                    {
                        holdtimer %= 1;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}