using System;
using System.Collections.Generic;
using System.Text;
using FRG.Core;

using UnityEngine;

namespace FRG.Core {
    public class TextUtil {

        private static string _hexChars = "0123456789ABCDEF";
        public static string ColorToHex(Color32 color) {

            using (Pooled<StringBuilder> pooled = RecyclingPool.SpawnStringBuilder())
            {
                StringBuilder builder = pooled.Value;

                builder.Append('#');
                AppendHexByte(builder, color.r);
                AppendHexByte(builder, color.g);
                AppendHexByte(builder, color.b);
                AppendHexByte(builder, color.a);
                return builder.ToString();
            }
        }

        private static void AppendHexByte(StringBuilder builder, byte value)
        {
            builder.Append(_hexChars[value / 16]);
            builder.Append(_hexChars[value % 16]);
        }

        public static string Boldify(string str_) {
            if(string.IsNullOrEmpty(str_.Trim())) return "";
            else return "<b>" + str_ + "</b>";
        }

        public static string Italicize(string str_) {
            if(string.IsNullOrEmpty(str_.Trim())) return "";
            else return "<i>" + str_ + "</i>";
        }

        public static string BoldAndItalicize(string str_) {
            if(string.IsNullOrEmpty(str_.Trim())) return "";
            else return "<b><i>" + str_ + "</i></b>";
        }

        public static string ColorizeRichText(LocalizedStringEntry originalString_, Color32 col_) {
            return ColorizeRichText(originalString_.ToLocalizedString(), col_);
        }

        public static string ColorizeRichText(string originalString_, Color32 col_) {
            if(string.IsNullOrEmpty(originalString_) || string.IsNullOrEmpty(originalString_.Trim())) return "";
            return "<color=" + ColorToHex(col_) + ">" + originalString_ + "</color>";
        }

        public static string CombineLines(params string[] lines_) {
            string ret = "";
            for(int i=0; i<lines_.Length; ++i) {
                ret += (string.IsNullOrEmpty(lines_[i].Trim()) ? "" : (i>0?"\n":"") + lines_[i].Trim());
            }
            return ret;
        }

        public static string CombineLines(params KeyValuePair<LocalizedStringEntry, Color>[] lines_) {
            KeyValuePair<string,Color>[] lines = new KeyValuePair<string,Color>[lines_.Length];
            for(int i=0; i<lines_.Length; ++i) lines[i] = new KeyValuePair<string,Color>(lines_[i].Key.ToLocalizedString(), lines_[i].Value);
            return CombineLines(lines);
        }

        public static string CombineLines(params KeyValuePair<string, Color>[] lines_) {
            string ret = "";
            for(int i=0; i<lines_.Length; ++i) {
                ret += (string.IsNullOrEmpty(lines_[i].Key) 
                    ? "" 
                    : (i>0?"\n":"") + ColorizeRichText(lines_[i].Key, lines_[i].Value));
            }
            return ret;
        }
    }
}
