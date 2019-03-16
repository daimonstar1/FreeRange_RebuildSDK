using System;
using System.Collections.Generic;
using System.Text;
using FRG.Core;

using UnityEngine;

namespace FRG.Core {
    public static class ColorUtil {

        /// <summary>
        /// Returns a color with an RGB magnitude of 1, ignoring alpha
        ///    If (includeAlpha) is true, will return a color with an RGBA magnitude of 1
        /// </summary>
        public static Color Normalize(this Color col, bool includeAlpha = false) {
            if(includeAlpha) {
                return col / Mathf.Sqrt((col.r*col.r) + (col.g*col.g) + (col.b*col.b) + (col.a*col.a));
            } else {
                Color ret = col / Mathf.Sqrt((col.r*col.r) + (col.g*col.g) + (col.b*col.b));
                ret.a = col.a;
                return ret;
            }
        }

        /// <summary>
        /// Increase the saturation of a color
        /// </summary>
        /// <param name="percent">Percent (0f - 1f) to saturate</param>
        public static Color Saturate(this Color col, float percent) {
            percent = Mathf.Clamp(percent, 0f, 1f);
            float average = (col.r + col.g + col.g) / 3f;
            return new Color(col.r + ((col.r - average) * percent),
                             col.g + ((col.g - average) * percent),
                             col.b + ((col.b - average) * percent),
                             col.a);
        }

        /// <summary>
        /// Decrease the saturation of a color
        /// </summary>
        /// <param name="percent">Percent (0f - 1f) to desaturate</param>
        public static Color Desaturate(this Color col, float percent) {
            percent = Mathf.Clamp(percent, 0f, 1f);
            float average = (col.r + col.g + col.g) / 3f;
            return new Color(col.r - ((col.r - average) * percent),
                             col.g - ((col.g - average) * percent),
                             col.b - ((col.b - average) * percent),
                             col.a);
        }

        /// <summary>
        /// Returns the average of the specified colors
        /// </summary>
        public static Color Average(params Color[] colors) {
            float r = 0f, g = 0f, b = 0f, a = 0f;
            if(colors != null) {
                for(int c = 0; c < colors.Length; ++c) {
                    Color col = colors[c];
                    r += col.r;
                    g += col.g;
                    b += col.b;
                    a += col.a;
                }
            }

            return new Color(r,g,b,a);
        }

        /// <summary>
        /// Returns a random color
        /// </summary>
        public static Color Random() {
            return new Color(UnityEngine.Random.Range(0f, 1f),
                             UnityEngine.Random.Range(0f, 1f),
                             UnityEngine.Random.Range(0f, 1f),
                             UnityEngine.Random.Range(0f, 1f));
        }

        /// <summary>
        /// Lerp a color to white by an amount
        /// </summary>
        public static Color Lighten(this Color color, float percent) {
            percent = Mathf.Clamp(percent, 0f, 1f);
            return Color.Lerp(color, Color.white.WithAlpha(color.a), percent);
        }

        /// <summary>
        /// Lerp a color to black by an amount
        /// </summary>
        public static Color Darken(this Color color, float percent) {
            percent = Mathf.Clamp(percent, 0f, 1f);
            return Color.Lerp(color, Color.black.WithAlpha(color.a), percent);
        }

        /// <summary>
        /// Replace the red-component of a color
        /// </summary>
        public static Color WithRed(this Color color, float red) {
            return new Color(red, color.g, color.b, color.a);
        }

        /// <summary>
        /// Replace the green-component of a color
        /// </summary>
        public static Color WithGreen(this Color color, float green) {
            return new Color(color.r, green, color.b, color.a);
        }

        /// <summary>
        /// Replace the blue-component of a color
        /// </summary>
        public static Color WithBlue(this Color color, float blue) {
            return new Color(color.r, color.g, blue, color.a);
        }

        /// <summary>
        /// Replace the alpha-component of a color
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha) {
            return new Color(color.r, color.g, color.b, alpha);
        }
        /// <summary>
        /// Invert the specified color, optionally inverting the alpha component
        /// </summary>
        public static Color Invert(this Color color, bool includeAlpha = false) {
            return new Color(1f - color.r,
                             1f - color.g,
                             1f - color.b,
                             includeAlpha ? (1f - color.a) : color.a);
        }

        public static uint[] ToInts(Color[] colors) {
            if(colors == null) return null;

            uint[] ret = new uint[colors.Length];
            for(int i = 0; i<colors.Length; ++i) {
                ret[i] = colors[i].ToUnsignedInt();
            }

            return ret;
        }

        public static Color[] FromInts(uint[] ints) {
            if(ints == null) return null;

            Color[] ret = new Color[ints.Length];
            for(int i = 0; i<ints.Length; ++i) {
                ret [i] = FromUnsignedInt(ints[i]);
            }

            return ret;
        }

        public static Color FromUnsignedInt(uint col) {
            return new Color(
                ((col & 0x000000FFU) >> 0) / 255f,
                ((col & 0x0000FF00U) >> 8) / 255f,
                ((col & 0x00FF0000U) >> 16) / 255f,
                ((col & 0xFF000000U) >> 24) / 255f
            );
        }

        public static uint ToUnsignedInt(this Color col) {
            return (uint)(
                    ((byte)(col.r * 255f) << 0)
                  | ((byte)(col.g * 255f) << 8)
                  | ((byte)(col.b * 255f) << 16)
                  | ((byte)(col.a * 255f) << 24));
        }

        public static byte[] ToBytes(Color[] colors) {
            if(colors == null) return null;

            byte[] ret = new byte[colors.Length*4];
            for(int i = 0; i<colors.Length; ++i) {
                ret[(i*4) + 0] = (byte)(int)(colors[i].r * 255f);
                ret[(i*4) + 1] = (byte)(int)(colors[i].g * 255f);
                ret[(i*4) + 2] = (byte)(int)(colors[i].b * 255f);
                ret[(i*4) + 3] = (byte)(int)(colors[i].a * 255f);
            }

            return ret;
        }

        public static Color[] FromBytes(byte[] bytes) {
            if(bytes == null) return null;
            if(bytes.Length % 4 != 0) {
                throw new ArgumentException("bytes.Length must be some multiple of 4");
            }

            Color[] ret = new Color[bytes.Length / 4];
            for(int i = 0; i<ret.Length; ++i) {
                ret[i] = new Color(
                    ((float)bytes[(i*4)+0])/255f, 
                    ((float)bytes[(i*4)+1])/255f, 
                    ((float)bytes[(i*4)+2])/255f, 
                    ((float)bytes[(i*4)+3])/255f);
            }

            return ret;
        }

        /// <summary>
        /// Return the hex code for a color
        /// </summary>
        public static string ToHex(this Color color) {
            return TextUtil.ColorToHex(color);
        }
    }
}
