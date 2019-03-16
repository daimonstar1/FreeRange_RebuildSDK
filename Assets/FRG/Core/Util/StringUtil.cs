using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FRG.Core {
    public static class StringUtil {
        /// <summary>
        /// Return the first index after the first instance of the specified value.
        /// </summary>
        /// <param name="str">String to search IN</param>
        /// <param name="value">String to search FOR</param>
        /// <returns>Returns 0 if value string is empty; -1 if either string is null, or if the value is not found at all.
        /// Otherwise, returns the index of the first character after the found substring</returns>
        public static int IndexAfter(this string str,
          string value,
          int startIndex = 0,
          int? count = null,
          StringComparison strComp = StringComparison.Ordinal) {

            startIndex = Clamp(startIndex, 0, str.Length - 1);

            //if this string is null, return -1
            if(str == null) return -1;

            //if value is null, return -1
            if(value == null) return -1;

            //if value is empty, return 0
            if(value.Length == 0) return 0;

            //return indexOf() + value length
            int ind = str.IndexOf(value, startIndex, strComp);
            if(ind != -1) {
                return ind + value.Length;
            }
            else {
                return -1;
            }
        }

        /// <summary>
        /// Return the substring after the last occurrence of (sub).
        ///     Ex: AfterLastInstanceOf("hippopotamus", "po") == "tamus";
        /// </summary>
        public static string AfterLastInstanceOf(this string str, string sub) {
            if(str == null) throw new ArgumentNullException("str");
            if(sub == null) throw new ArgumentNullException("substring");

            int pos = sub.Length - 1;
            for(int i = str.Length - 1;i >= 0;--i) {
                if(str[i] == sub[pos]) {
                    --pos;
                    if(pos < 0) {
                        return str.Substring(i + sub.Length - 1);
                    }
                }
                else {
                    pos = sub.Length - 1;
                }
            }

            //substring does not exist, return the entire string
            return str;
        }

        /// <summary>
        /// Return the string in-between the other two specified strings
        /// </summary>
        /// <param name="str">String to search IN</param>
        /// <param name="startString">First string to search FOR</param>
        /// <param name="endString">Second string to search FOR</param>
        /// <param name="startIndex">Index to begin at</param>
        /// <returns>Returns null if any string is null, or if either start or ending string is not found.
        /// Otherwise, returns the string in-between [startString] and [endString].</returns>
        public static string StringBetween(this string str,
          string startString,
          string endString,
          int startIndex = 0,
          StringComparison strComp = StringComparison.Ordinal) {

            startIndex = Clamp(startIndex, 0, str.Length - 1);

            if(string.IsNullOrEmpty(str)) return null;
            if(string.IsNullOrEmpty(startString)) return null;
            if(string.IsNullOrEmpty(endString)) return null;

            int start = str.IndexAfter(startString, startIndex, str.Length - startIndex, strComp);

            //start the 
            int end = str.IndexOf(endString, startIndex + start, strComp);

            if(start != -1 && end != -1 && start <= end) {
                return str.Substring(start, end - start);
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Return all strings in-between the other two specified strings
        /// </summary>
        /// <param name="str">String to search IN</param>
        /// <param name="startString">First string to search FOR</param>
        /// <param name="endString">Second string to search FOR</param>
        /// <param name="startIndex">Index to begin at</param>
        /// <returns>Returns null if any string is null, or if either start or ending string is not found.
        /// Otherwise, returns the string in-between [startString] and [endString].</returns>
        public static IEnumerable<string> StringsBetween(this string str,
          string startString,
          string endString,
          int startIndex = 0,
          StringComparison strComp = StringComparison.Ordinal) {

            if(string.IsNullOrEmpty(str)) yield break;
            if(string.IsNullOrEmpty(startString)) yield break;
            if(string.IsNullOrEmpty(endString)) yield break;

            startIndex = Clamp(startIndex, 0, str.Length - 1);
            while(startIndex < str.Length) {
                int start = str.IndexAfter(startString, startIndex, str.Length - startIndex, strComp);

                if(start != -1) {
                    int end = str.IndexOf(endString, start, strComp);

                    if(end != -1 && start <= end) {
                        yield return str.Substring(start, end - start);

                        startIndex = end;
                    }
                    else {
                        yield break;
                    }
                }
                else {
                    yield break;
                }
            }
            yield break;
        }

        //Copied from FRG.Util.Clamp()
        private static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
            return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
        }


        /// <summary>
        /// Replace an old value in a string with a new one; found with the specified string comparison
        /// </summary>
        /// <param name="self">String containing the old substring.</param>
        /// <param name="oldValue">The substring to replace.</param>
        /// <param name="newValue">The new string to replace with.</param>
        /// <param name="comparisonType">The string comparison to find matches with.</param>
        public static string Replace(this string self, string oldValue, string newValue, StringComparison comparisonType) {
            using(Pooled<StringBuilder> pooled = RecyclingPool.SpawnStringBuilder()) {
                StringBuilder builder = pooled.Value;

                int index = 0;
                int previousIndex = index;
                while(true) {
                    index = self.IndexOf(oldValue, previousIndex, comparisonType);
                    if(index < 0) {
                        break;
                    }

                    if(index >= previousIndex) {
                        builder.Append(self, previousIndex, index - previousIndex);
                        index += oldValue.Length;
                    }

                    builder.Append(newValue);
                    previousIndex = index;
                }

                builder.Append(self, previousIndex, self.Length - previousIndex);
                return builder.ToString();
            }
        }

        /// <summary>
        /// Debug card name shortener
        /// </summary>
        public static string AbbreviateCard(string name) {
            const int charsBefore = 1;
            const int charsBeforeLast = 3;
            const int charsAfter = 3;
            const int charsAfterLast = 999;
            const char underChar = '_';

            StringBuilder abbrev = new StringBuilder();
            int lastUpper = 0;
            bool postUnder = false;
            for(int i = 0;i < name.Length;i++) {
                char c = name[i];
                if(char.IsUpper(c) || char.IsDigit(c)) {
                    if(i > lastUpper) {
                        int count = charsBefore;
                        if(postUnder) count = charsAfter;
                        int len = i - lastUpper;
                        if(len > count) len = count;
                        abbrev.Append(name.Substring(lastUpper, len));

                        lastUpper = i;
                    }
                    if(char.IsDigit(c)) {
                        abbrev.Append(c);
                        lastUpper = i + 1;
                    }
                }
                if(c == '_' || i == name.Length - 1) {
                    int count = charsBeforeLast;
                    if(postUnder) count = charsAfterLast;

                    postUnder = true;
                    // word's done
                    int len = i - lastUpper + 1;
                    if(len > 0) {
                        if(len > count) len = count;
                        abbrev.Append(name.Substring(lastUpper, len));
                        if(c == '_') abbrev.Append(underChar);
                        lastUpper = i + 1;
                    }
                }
            }
            return abbrev.ToString();
        }

        /// <summary>
        /// Debug card name shortener
        /// </summary>
        public static string AbbreviateUnit(string name) {
            const int charsBefore = 1;
            const int charsBeforeLast = 1;
            const int charsAfter = 999;
            const int charsAfterLast = 999;
            const char underChar = ' ';

            StringBuilder abbrev = new StringBuilder();
            int lastUpper = 0;
            bool postUnder = false;
            for(int i = 0;i < name.Length;i++) {
                char c = name[i];
                if(char.IsUpper(c) || char.IsDigit(c)) {
                    if(i > lastUpper) {
                        int count = charsBefore;
                        if(postUnder) count = charsAfter;
                        int len = i - lastUpper;
                        if(len > count) len = count;
                        abbrev.Append(name.Substring(lastUpper, len));

                        lastUpper = i;
                    }
                    if(char.IsDigit(c)) {
                        abbrev.Append(c);
                        lastUpper = i + 1;
                    }
                }
                if(c == '_' || i == name.Length - 1) {
                    int count = charsBeforeLast;
                    if(postUnder) count = charsAfterLast;

                    postUnder = true;
                    // word's done
                    int len = i - lastUpper + 1;
                    if(len > 0) {
                        if(len > count) len = count;
                        abbrev.Append(name.Substring(lastUpper, len));
                        if(c == '_') abbrev.Append(underChar);
                        lastUpper = i + 1;
                    }
                }
            }
            return abbrev.ToString();
        }

        public static string GetOrdinal(int num) {
            if(num <= 0) return num.ToString();

            switch(num % 100) {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch(num % 10) {
                case 1: return num + "st";
                case 2: return num + "nd";
                case 3: return num + "rd";
                default: return num + "th";
            }
        }
    }
}
