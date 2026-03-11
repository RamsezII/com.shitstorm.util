using System;
using UnityEngine;

namespace _UTIL_
{
    [Obsolete]
    public static partial class Typing
    {
        //http://digitalnativestudios.com/textmeshpro/docs/rich-text/

        public enum TypingFilters { text, cobra, name, addressIP, port, _last_ }
        public static string value;
        public static int lastFrame;

        //----------------------------------------------------------------------------------------------------------

        public static string pullValue
        {
            get
            {
                string current = value;
                Reset();
                return current;
            }
        }

        public static void Reset(in string reset = null)
        {
            flags |= TypingFlags.changes | TypingFlags.refresh;
            value = reset;
            bar_toggle = true;
            bar_time = .5f;
        }

        public static bool Clamp(in byte clamp)
        {
            if (value == null || value.Length <= clamp)
                return false;
            else
            {
                value = value.Substring(0, clamp);
                return true;
            }
        }

        public static bool Read(in TypingFilters filter, in Action<bool> onErase = null)
        {
            PullFlags(TypingFlags.changes);

            if (getCtrl_hold || Time.frameCount == lastFrame)
                return false;
            else
                lastFrame = Time.frameCount;

            string old = value;
            bool maj = getShift_hold;
            bool altgr = Input.GetKey(KeyCode.AltGr);

            switch (filter)
            {
                case TypingFilters.text:
                case TypingFilters.name:
                case TypingFilters.cobra:
                    for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
                        if (GetKeyHold(key))
                            value += maj ? "" + key : ("" + key).ToLower();
                    break;

                case TypingFilters.addressIP:
                    for (KeyCode key = KeyCode.A; key <= KeyCode.F; key++)
                        if (GetKeyHold(key))
                            value += ("" + key).ToLower();
                    break;
            }

            for (KeyCode key = KeyCode.Keypad0; key <= KeyCode.KeypadPlus; key++)
                if (GetKeyHold(key))
                {
                    if (key <= KeyCode.Keypad9)
                        value += key - KeyCode.Keypad0;
                    else if (filter != TypingFilters.port)
                        switch (filter)
                        {
                            case TypingFilters.text:
                            case TypingFilters.cobra:
                                switch (key)
                                {
                                    case KeyCode.KeypadPeriod: value += '.'; break;
                                    case KeyCode.KeypadDivide: value += '/'; break;
                                    case KeyCode.KeypadMultiply: value += '*'; break;
                                    case KeyCode.KeypadMinus: value += '-'; break;
                                    case KeyCode.KeypadPlus: value += '+'; break;
                                }
                                break;

                            case TypingFilters.name:
                                switch (key)
                                {
                                    case KeyCode.KeypadMultiply: value += '*'; break;
                                    case KeyCode.KeypadMinus: value += '-'; break;
                                    case KeyCode.KeypadPlus: value += '+'; break;
                                }
                                break;

                            case TypingFilters.addressIP:
                                switch (key)
                                {
                                    case KeyCode.KeypadPeriod: value += '.'; break;
                                    case KeyCode.KeypadDivide: value += '/'; break;
                                }
                                break;
                        }
                }

            switch (filter)
            {
                case TypingFilters.text:
                case TypingFilters.cobra:
                    for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
                        if (GetKeyHold(key))
                        {
                            if (maj)
                                value += key - KeyCode.Alpha0;
                            else
                                switch (key)
                                {
                                    case KeyCode.Alpha0: value += altgr ? '@' : 'à'; break;
                                    case KeyCode.Alpha1: value += '&'; break;
                                    case KeyCode.Alpha2: value += altgr ? '~' : 'é'; break;
                                    case KeyCode.Alpha3: value += altgr ? '#' : '"'; break;
                                    case KeyCode.Alpha4: value += altgr ? '{' : '\''; break;
                                    case KeyCode.Alpha5: value += altgr ? '[' : '('; break;
                                    case KeyCode.Alpha6: value += altgr ? '|' : '-'; break;
                                    case KeyCode.Alpha7: value += altgr ? '`' : 'è'; break;
                                    case KeyCode.Alpha8: value += altgr ? '\\' : '_'; break;
                                    case KeyCode.Alpha9: value += altgr ? '^' : 'ç'; break;
                                }
                        }
                    break;
            }

            switch (filter)
            {
                case TypingFilters.text:
                case TypingFilters.cobra:
                    if (maj && getReturn_down)
                        value += "<br>";
                    break;
            }

            switch (filter)
            {
                case TypingFilters.text:
                case TypingFilters.cobra:
                    {
                        if (GetKeyHold(KeyCode.RightBracket))
                            value += maj ? '¨' : '^';

                        if (GetKeyHold(KeyCode.Semicolon))
                            value += maj ? '£' : altgr ? '¤' : '$';

                        if (GetKeyHold(KeyCode.LeftBracket))
                            value += maj ? '°' : altgr ? ']' : ')';

                        if (GetKeyHold(KeyCode.Equals))
                            value += maj ? '+' : altgr ? '}' : '=';

                        if (GetKeyHold(KeyCode.Quote))
                            value += '²';

                        if (GetKeyHold(KeyCode.Backslash))
                            //result += maj ? 'µ' : '*';
                            value += maj ? '>' : '<';

                        if (GetKeyHold(KeyCode.Comma))
                            value += maj ? '?' : ',';

                        if (GetKeyHold(KeyCode.Period))
                            value += maj ? '.' : ';';

                        if (GetKeyHold(KeyCode.Slash))
                            value += maj ? '/' : ':';

                        if (GetKeyHold(KeyCode.BackQuote))
                            //result += maj ? '%' : 'ù';
                            value += maj ? '§' : '!';

                        if (GetKeyHold(KeyCode.Space))
                            value += ' ';
                    }
                    break;
            }

            if (getCtrlV_down)
                value += GUIUtility.systemCopyBuffer;

            if (!string.IsNullOrEmpty(value) && GetKeyHold(KeyCode.Backspace))
            {
                value = maj ? null : value.Substring(0, value.Length - 1);
                onErase?.Invoke(maj);
            }

            bar_time += 1 / .35f * Time.unscaledDeltaTime;

            if (!GetFlags(TypingFlags.refresh))
            {
                if (value == null)
                {
                    if (old != null)
                        flags |= TypingFlags.refresh | TypingFlags.changes;
                }
                else if (value != old)
                    flags |= TypingFlags.refresh | TypingFlags.changes;

                if (GetFlags(TypingFlags.refresh))
                {
                    bar_toggle = true;
                    bar_time = .7f;
                }
            }

            if (bar_time >= 1)
            {
                SetFlags(TypingFlags.refresh);
                bar_toggle = !bar_toggle;
                bar_time %= 1;
            }

            return PullFlags(TypingFlags.refresh);
        }
    }
}