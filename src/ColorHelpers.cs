// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Windows.Media;

namespace ClearlyEditable
{
    public static class ColorHelpers
    {
        public static double RationalizeOpacity(int percentage)
        {
            var workingvalue = percentage;

            if (workingvalue < 0)
            {
                workingvalue = 0;
            }
            else if (workingvalue > 100)
            {
                workingvalue = 100;
            }

            return workingvalue / 100f;
        }

        public static SolidColorBrush GetColorBrush(string color, double opacity)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            if (!color.TrimStart().StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
            {
                color = GetHexForNamedColor(color.Trim());
            }

            Color parsedColor;

            try
            {
                parsedColor = (Color)ColorConverter.ConvertFromString(color.Trim());
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                GeneralOutputPane.Instance.Write($"Unable to translate '{color}' into a color.");

                parsedColor = Colors.Transparent;
            }

            return new SolidColorBrush(parsedColor) { Opacity = opacity };
        }

        public static string GetHexForNamedColor(string colorName)
        {
            if (string.IsNullOrEmpty(colorName))
            {
                return colorName;
            }

            switch (colorName.ToUpperInvariant())
            {
                case "ALICEBLUE": return "#F0F8FF";
                case "ANTIQUEWHITE": return "#FAEBD7";
                case "AQUA": return "#00FFFF";
                case "AQUAMARINE": return "#7FFFD4";
                case "AZURE": return "#F0FFFF";
                case "BEIGE": return "#F5F5DC";
                case "BISQUE": return "#FFE4C4";
                case "BLACK": return "#000000";
                case "BLANCHEDALMOND": return "#FFEBCD";
                case "BLUE": return "#0000FF";
                case "BLUEVIOLET": return "#8A2BE2";
                case "BROWN": return "#A52A2A";
                case "BURLYWOOD": return "#DEB887";
                case "CADETBLUE": return "#5F9EA0";
                case "CHARTREUSE": return "#7FFF00";
                case "CHOCOLATE": return "#D2691E";
                case "CORAL": return "#FF7F50";
                case "CORNFLOWERBLUE": return "#6495ED";
                case "CORNSILK": return "#FFF8DC";
                case "CRIMSON": return "#DC143C";
                case "CYAN": return "#00FFFF";
                case "DARKBLUE": return "#00008B";
                case "DARKCYAN": return "#008B8B";
                case "DARKGOLDENROD": return "#B8860B";
                case "DARKGRAY": return "#A9A9A9";
                case "DARKGREEN": return "#006400";
                case "DARKGREY": return "#A9A9A9";
                case "DARKKHAKI": return "#BDB76B";
                case "DARKMAGENTA": return "#8B008B";
                case "DARKOLIVEGREEN": return "#556B2F";
                case "DARKORANGE": return "#FF8C00";
                case "DARKORCHID": return "#9932CC";
                case "DARKRED": return "#8B0000";
                case "DARKSALMON": return "#E9967A";
                case "DARKSEAGREEN": return "#8FBC8B";
                case "DARKSLATEBLUE": return "#483D8B";
                case "DARKSLATEGRAY": return "#2F4F4F";
                case "DARKSLATEGREY": return "#2F4F4F";
                case "DARKTURQUOISE": return "#00CED1";
                case "DARKVIOLET": return "#9400D3";
                case "DEEPPINK": return "#FF1493";
                case "DEEPSKYBLUE": return "#00BFFF";
                case "DIMGRAY": return "#696969";
                case "DIMGREY": return "#696969";
                case "DODGERBLUE": return "#1E90FF";
                case "FIREBRICK": return "#B22222";
                case "FLORALWHITE": return "#FFFAF0";
                case "FORESTGREEN": return "#228B22";
                case "FUCHSIA": return "#FF00FF";
                case "GAINSBORO": return "#DCDCDC";
                case "GHOSTWHITE": return "#F8F8FF";
                case "GOLD": return "#FFD700";
                case "GOLDENROD": return "#DAA520";
                case "GRAY": return "#808080";
                case "GREEN": return "#008000";
                case "GREENYELLOW": return "#ADFF2F";
                case "GREY": return "#808080";
                case "HONEYDEW": return "#F0FFF0";
                case "HOTPINK": return "#FF69B4";
                case "INDIANRED": return "#CD5C5C";
                case "INDIGO": return "#4B0082";
                case "IVORY": return "#FFFFF0";
                case "KHAKI": return "#F0E68C";
                case "LAVENDER": return "#E6E6FA";
                case "LAVENDERBLUSH": return "#FFF0F5";
                case "LAWNGREEN": return "#7CFC00";
                case "LEMONCHIFFON": return "#FFFACD";
                case "LIGHTBLUE": return "#ADD8E6";
                case "LIGHTCORAL": return "#F08080";
                case "LIGHTCYAN": return "#E0FFFF";
                case "LIGHTGOLDENRODYELLOW": return "#FAFAD2";
                case "LIGHTGRAY": return "#D3D3D3";
                case "LIGHTGREEN": return "#90EE90";
                case "LIGHTGREY": return "#D3D3D3";
                case "LIGHTPINK": return "#FFB6C1";
                case "LIGHTSALMON": return "#FFA07A";
                case "LIGHTSEAGREEN": return "#20B2AA";
                case "LIGHTSKYBLUE": return "#87CEFA";
                case "LIGHTSLATEGRAY": return "#778899";
                case "LIGHTSLATEGREY": return "#778899";
                case "LIGHTSTEELBLUE": return "#B0C4DE";
                case "LIGHTYELLOW": return "#FFFFE0";
                case "LIME": return "#00FF00";
                case "LIMEGREEN": return "#32CD32";
                case "LINEN": return "#FAF0E6";
                case "MAGENTA": return "#FF00FF";
                case "MAROON": return "#800000";
                case "MEDIUMAQUAMARINE": return "#66CDAA";
                case "MEDIUMBLUE": return "#0000CD";
                case "MEDIUMORCHID": return "#BA55D3";
                case "MEDIUMPURPLE": return "#9370DB";
                case "MEDIUMSEAGREEN": return "#3CB371";
                case "MEDIUMSLATEBLUE": return "#7B68EE";
                case "MEDIUMSPRINGGREEN": return "#00FA9A";
                case "MEDIUMTURQUOISE": return "#48D1CC";
                case "MEDIUMVIOLETRED": return "#C71585";
                case "MIDNIGHTBLUE": return "#191970";
                case "MINTCREAM": return "#F5FFFA";
                case "MISTYROSE": return "#FFE4E1";
                case "MOCCASIN": return "#FFE4B5";
                case "NAVAJOWHITE": return "#FFDEAD";
                case "NAVY": return "#000080";
                case "OLDLACE": return "#FDF5E6";
                case "OLIVE": return "#808000";
                case "OLIVEDRAB": return "#6B8E23";
                case "ORANGE": return "#FFA500";
                case "ORANGERED": return "#FF4500";
                case "ORCHID": return "#DA70D6";
                case "PALEGOLDENROD": return "#EEE8AA";
                case "PALEGREEN": return "#98FB98";
                case "PALETURQUOISE": return "#AFEEEE";
                case "PALEVIOLETRED": return "#DB7093";
                case "PAPAYAWHIP": return "#FFEFD5";
                case "PEACHPUFF": return "#FFDAB9";
                case "PERU": return "#CD853F";
                case "PINK": return "#FFC0CB";
                case "PLUM": return "#DDA0DD";
                case "POWDERBLUE": return "#B0E0E6";
                case "PURPLE": return "#800080";
                case "REBECCAPURPLE": return "#663399";
                case "RED": return "#FF0000";
                case "ROSYBROWN": return "#BC8F8F";
                case "ROYALBLUE": return "#4169E1";
                case "SADDLEBROWN": return "#8B4513";
                case "SALMON": return "#FA8072";
                case "SANDYBROWN": return "#F4A460";
                case "SEAGREEN": return "#2E8B57";
                case "SEASHELL": return "#FFF5EE";
                case "SIENNA": return "#A0522D";
                case "SILVER": return "#C0C0C0";
                case "SKYBLUE": return "#87CEEB";
                case "SLATEBLUE": return "#6A5ACD";
                case "SLATEGRAY": return "#708090";
                case "SLATEGREY": return "#708090";
                case "SNOW": return "#FFFAFA";
                case "SPRINGGREEN": return "#00FF7F";
                case "STEELBLUE": return "#4682B4";
                case "TAN": return "#D2B48C";
                case "TEAL": return "#008080";
                case "THISTLE": return "#D8BFD8";
                case "TOMATO": return "#FF6347";
                case "TURQUOISE": return "#40E0D0";
                case "VIOLET": return "#EE82EE";
                case "WHEAT": return "#F5DEB3";
                case "WHITE": return "#FFFFFF";
                case "WHITESMOKE": return "#F5F5F5";
                case "YELLOW": return "#FFFF00";
                case "YELLOWGREEN": return "#9ACD32";
                default: return colorName;
            }
        }
    }
}
