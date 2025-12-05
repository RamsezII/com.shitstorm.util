using System;
using System.Collections.Generic;
using UnityEngine;

namespace _UTIL_
{
    [Serializable]
    public sealed class LintTheme
    {
        public Color
            argument = Color.deepPink,
            argument_coma = Color.lightPink,
            point = Color.yellow,
            flags = Color.beige,
            options = Color.bisque,
            option_args = Color.sandyBrown,
            operators = Color.lightGray,
            contracts = Color.darkSlateBlue,
            sub_contracts = Color.darkSlateBlue,
            functions = Color.deepSkyBlue,
            variables = Color.mediumPurple,
            paths = Color.ivory,
            fpaths = Color.ivory,
            dpaths = Color.ivory,
            comments = Color.darkOliveGreen,
            command_separators = Color.softYellow,
            keywords = Color.magenta,
            bracket_0 = Color.yellow,
            bracket_1 = Color.rebeccaPurple,
            bracket_2 = Color.navyBlue,
            literal = Color.limeGreen,
            constants = Color.deepSkyBlue,
            strings = Color.orange,
            quotes = Color.yellowNice,
            error = Color.red,
            fallback_default = Color.gray
            ;

        public static readonly LintTheme
            theme_dark = new()
            {

            },
            theme_light = new()
            {

            };

        [Serializable]
        struct Pair
        {
            public string type;
            public Color color;
            public Pair(in Type type, in Color color)
            {
                this.type = type.FullName;
                this.color = color;
            }
        }

        [SerializeField] Color fallback_color = Color.gray9;
        [SerializeField] Pair[] pairs;
        readonly Dictionary<Type, Color> lints = new();

        //----------------------------------------------------------------------------------------------------------

        public void RebuildDictionary()
        {
            lints.Clear();
            for (int i = 0; i < pairs.Length; i++)
            {
                Pair pair = pairs[i];
                if (pair.type.TryGetType(out Type type, include_abstracts: true))
                    lints[type] = pair.color;
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public bool TryGetLint(in object value, out Color lint)
        {
            Type type = value.GetType();
            foreach (var pair in lints)
                if (pair.Key.IsAssignableFrom(type))
                {
                    lint = pair.Value;
                    return true;
                }
            lint = fallback_color;
            return false;
        }
    }
}