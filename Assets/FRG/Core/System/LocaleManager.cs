
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace FRG.Core
{
    public static class LocaleManager
    {
        
        public static Dictionary<SystemLanguage, string> LanguageCodes
        {
            get
            {
                return Statics.LanguageCodes;
            }
        }

        private static CultureInfo _platformCulture = null;
        //private static CultureInfo _currentCulture = null;
        //private static CultureInfo _originalCurrentCulture;
        //private static CultureInfo _originalUICulture;
        //private static string _deviceCultureString = "<unassigned>";
        //private static string _cultureSource = "<uninitialized>";

        /// <summary>
        /// The currently-set culture for the device. Note that this is different from language (can be completely different in some contexts).
        /// </summary>
        public static CultureInfo DisplayCulture
        {
            get
            {
                return _platformCulture ?? CultureInfo.InvariantCulture;
            }
            //set
            //{
            //    _currentCulture = value;
            //    Statics.FontToDisplayCultureLookup.Clear();
            //    Statics.CultureAndCaseSensitivityToStringComparerLookup.Clear();
            //}
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCulture()
        {
            //LoggingConfigurator.Configure();

            using (ProfileUtil.PushSample("LocaleManager.InitializeCultureEditor"))
            {
                InitializeCultureImplementation();
            }
        }

        private static void InitializeCultureImplementation()
        {
            if (_platformCulture != null) return;

            bool shouldPrint = false;

            //_originalCurrentCulture = CultureInfo.CurrentCulture;
            //_originalUICulture = CultureInfo.CurrentUICulture;

            CultureInfo culture;
            try
            {
                culture = GetDeviceCulture();
            }
            catch (Exception e)
            {
                ReflectionUtil.CheckDangerousException(e);

                //logger.Error("Exception occurred while trying to grab culture info from device.", e);

                culture = null;
            }

            _platformCulture = culture;
            //_cultureSource = "device locale";

            if (_platformCulture == null)
            {
                shouldPrint = true;
                _platformCulture = GetLanguageCulture(LocalizedStringEntry.CurrentLanguage);
                //_cultureSource = "Application.systemLanguage";
            }

            if (_platformCulture == null)
            {
                _platformCulture = GetSpecificCulture(CultureInfo.CurrentCulture);
                //_cultureSource = "CultureInfo.CurrentCulture";
            }

            if (_platformCulture == null)
            {
                _platformCulture = CultureInfo.InvariantCulture;
                //_cultureSource = "CultureInfo.InvariantCulture";
            }

            // Unity normally does this anyway on nearly every platform.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (shouldPrint)
            {
                PrintCulturesShort();
            }
        }

#if UNITY_EDITOR_WIN || !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_WINRT || UNITY_XBOX360 || UNITY_XBOXONE)
        // Windows (may work on XBOX and WINRT but can't test)
        private static CultureInfo GetDeviceCulture()
        {
            //int lcid = GetSystemDefaultLCID();
            //_deviceCultureString = "LCID " + lcid.ToString();
            return GetCultureFromLcid(GetSystemDefaultLCID());
        }

        [DllImport("KERNEL32.DLL")]
        private static extern int GetSystemDefaultLCID();

#elif !UNITY_EDITOR && UNITY_ANDROID
        
        // Android
        private static CultureInfo GetDeviceCulture()
        {
            AndroidJavaClass localeClass = new AndroidJavaClass("java/util/Locale");
            AndroidJavaObject defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault");
            string culture = (defaultLocale != null) ? defaultLocale.Call<string>("toString") : null;
            //_deviceCultureString = culture;
            return GetGenericCulture(culture, true);
        }

#elif UNITY_EDITOR_OSX || !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_IOS)
       
        private static CultureInfo GetDeviceCulture()
        {
            // These pointers aren't actually guaranteed to be around,
            // but I'm assuming it's going to keep the default one in memory.

            IntPtr locale = CFLocaleGetSystem();
            if (locale == IntPtr.Zero) return null;

            IntPtr identifier = CFLocaleGetIdentifier(locale);
            if (identifier == IntPtr.Zero) return null;

            byte[] buffer = new byte[256];
            if (!CFStringGetCString(identifier, buffer, buffer.Length, kCFStringEncodingASCII)) return null;

            int length = Array.IndexOf<byte>(buffer, 0);
            if (length <= 0) return null;

            string localeName = Encoding.ASCII.GetString(buffer, 0, length);

            //_deviceCultureString = localeName;
            return GetGenericCulture(localeName, false);
        }

        private const int kCFStringEncodingASCII = 0x0600;
        private const int kCFStringEncodingUTF8 = 0x08000100;

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr CFLocaleGetSystem();

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern IntPtr CFLocaleGetIdentifier(IntPtr locale);

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern bool CFStringGetCString(IntPtr theString, byte[] buffer, int bufferSize, int encoding);

#elif UNITY_EDITOR_LINUX || !UNITY_EDITOR && UNITY_LINUX
        
        // Unix
        // Assume most things are Unix of some sort
        // This may be a bad assumption, so add more exceptions if it doesn't work
        // May need an objective-C plugin instead to get language and country codes for iOS/OSX.
        private static CultureInfo GetDeviceCulture()
        {
            string localeName;

            IntPtr localeNamePtr = get_current_locale_name();
            if (localeNamePtr == IntPtr.Zero) return null;
            try
            {
                localeName = Marshal.PtrToStringAnsi(localeNamePtr);
            }
            finally
            {
                g_free(localeNamePtr);
            }
            
            // Could go either way on region_before_script
            return GetGenericCulture(localeName, true);
        }

        [DllImport("libmono")]
        private static extern IntPtr get_current_locale_name();

        [DllImport("libmono")]
        private static extern void g_free(IntPtr ptr);

#elif !UNITY_EDITOR && UNITY_WEBGL
        
        // WebGL could possibly get something from the server
        private static CultureInfo GetDeviceCulture()
        {
            //_deviceCultureString = "<nothing specified>";
            return null;
        }
        
#else
        // Default to error (that will just get logged and ignored)
        private static CultureInfo GetDeviceCulture()
        {
            //_deviceCultureString = "<unimplemented platform>";
            throw new NotImplementedException("Could not determine culture information for this type of device.");
        }
        
#endif

        /// <summary>
        /// Gets a (default) specific culture based upon a neutral culture.
        /// Specific cultures are returned directly.
        /// </summary>
        private static CultureInfo GetSpecificCulture(CultureInfo culture)
        {
            if (culture == null || !culture.IsNeutralCulture) return culture;

            try
            {
                return CultureInfo.CreateSpecificCulture(culture.Name);
            }
            // Mono doesn't use a different exception
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a CultureInfo object based upon an LCID (Windows locale ID).
        /// </summary>
        private static CultureInfo GetCultureFromLcid(int lcid)
        {
            try
            {
                return GetSpecificCulture(CultureInfo.GetCultureInfo(lcid));
            }
            // Mono doesn't use a different exception
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a culture from the specified name.
        /// </summary>
        private static CultureInfo GetCultureFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            try
            {
                return GetSpecificCulture(CultureInfo.GetCultureInfo(name));
            }
            // Mono doesn't use a different exception
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static CultureInfo GetCultureFromStrings(string language, string script, string region)
        {
            // language2-Script4-REGION2
            // language2-REGION2
            // language2-Script4
            // language2

            language = CleanComponent(language);
            region = CleanComponent(region);
            script = CleanComponent(script);

            CultureInfo culture = null;
            if (!string.IsNullOrEmpty(language))
            {

                if (!string.IsNullOrEmpty(region))
                {
                    if (!string.IsNullOrEmpty(script))
                    {
                        culture = GetCultureFromName(language + "-" + script + "-" + region);
                        if (culture != null) return culture;
                    }

                    culture = GetCultureFromName(language + "-" + region);
                    if (culture != null) return culture;
                }

                if (!string.IsNullOrEmpty(script))
                {
                    culture = GetCultureFromName(language + "-" + script);
                    if (culture != null) return culture;
                }

                culture = GetCultureFromName(language);
            }
            return culture;
        }

        // Apple tends to be: language2_Script4_REGION2 or language2_REGION2 or language2_Script4
        // Linux varies widely.
        private static CultureInfo GetGenericCulture(string culture, bool regionBeforeScript)
        {
            // Truncate in case there are periods or at signs.
            culture = CleanComponent(culture);

            if (string.IsNullOrEmpty(culture)) return null;

            string[] components = culture.Split(new char[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (components.Length <= 0) return null;

            string language = components[0];
            string script = null;
            string region = null;

            // When length == 2, component[1] could be Script4 or REGION2
            // Assume this component is the script so we can complete neutral cultures.
            if (components.Length == 2 || !regionBeforeScript)
            {
                script = components.Length > 1 ? components[1] : null;
                region = components.Length > 2 ? components[2] : null;
            }
            else
            {
                script = components.Length > 2 ? components[2] : null;
                region = components.Length > 1 ? components[1] : null;
            }

            return GetCultureFromStrings(language, script, region);
        }

        /// <summary>
        /// Keeps only the starting alphanumeric characters of a locale code component.
        /// </summary>
        private static string CleanComponent(string component)
        {
            component = component ?? "";
            for (int i = 0; i < component.Length; ++i)
            {
                char c = component[i];
                if (!(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '-' || c == '_'))
                {
                    return component.Substring(0, i);
                }
            }
            return component;
        }

        /// <summary>
        /// Truncate the specified datetime to a specified minute.
        /// </summary>
        public static DateTime TruncateToMinute(DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerMinute));
        }

        /// <summary>
        /// Truncate the specified datetime to a specified second.
        /// </summary>
        public static DateTime TruncateToSecond(DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
        }

        /// <summary>
        /// Formatting a DateTime to the current locale.
        /// For the options, see <a href="https://msdn.microsoft.com/en-us/library/az4se3k1.aspx">Standard Date and Time Format Strings</a>.
        /// </summary>
        private static string FormatLocalizedDateTime(Font font, DateTime dateTime, string format)
        {
            if (string.IsNullOrEmpty(format)) throw new ArgumentNullException("format");
            

            return dateTime.ToLocalTime().ToString(format, GetDisplayCultureForFont(font));
        }

        /// <summary>
        /// Formatting a DateTime to the current locale.
        /// For the options, see <a href="https://msdn.microsoft.com/en-us/library/az4se3k1.aspx">Standard Date and Time Format Strings</a>.
        /// </summary>
        private static string FormatLocalizedDateTime(TMPro.TMP_FontAsset font, DateTime dateTime, string format)
        {
            if (string.IsNullOrEmpty(format)) throw new ArgumentNullException("format");


            return dateTime.ToLocalTime().ToString(format, GetDisplayCultureForFont(font));
        }

        /// <summary>
        /// A numerical representation of the date.
        /// </summary>
        /// <remarks>
        /// Depends on user's current locale. Can vary, but usually something like the following:
        /// Short date: "6/15/2009" or "2009/06/15"
        /// Long date: "Monday, June 15, 2009" or "Montag, 15. Juni 2009"
        /// </remarks>
        public static string FormatLocalizedDate(Font font, DateTime dateTime, bool longDate)
        {
            return FormatLocalizedDateTime(font, dateTime, longDate ? "D" : "d");
        }

        /// <summary>
        /// A numerical representation of the date.
        /// </summary>
        /// <param name="longTime">Specifies this is a "long" time; usually whether to use seconds or not.</param>
        /// <remarks>
        /// Depends on user's current locale. Can vary, but usually something like the following:
        /// Short time: "1:45 PM" or "13:45" (might include stuff like different alphabets)
        /// Long time: "1:45:30 PM" or "13:45:30"
        /// </remarks>
        public static string FormatLocalizedTime(Font font, DateTime dateTime, bool longTime)
        {
            return FormatLocalizedDateTime(font, dateTime, longTime ? "T" : "t");
        }

        /// <summary>
        /// A numerical representation of the date.
        /// </summary>
        /// <param name="longTime">Specifies this is a "long" time; usually whether to use seconds or not.</param>
        /// <remarks>
        /// Depends on user's current locale. Can vary, but usually something like the following:
        /// Short time: "1:45 PM" or "13:45" (might include stuff like different alphabets)
        /// Long time: "1:45:30 PM" or "13:45:30"
        /// </remarks>
        public static string FormatLocalizedTime(TMPro.TMP_FontAsset font, DateTime dateTime, bool longTime)
        {
            return FormatLocalizedDateTime(font, dateTime, longTime ? "T" : "t");
        }

        /// <summary>
        /// Usually a concatenation of the two above values.
        /// </summary>
        public static string FormatLocalizedDateAndTime(Font font, DateTime dateTime, bool longDate, bool longTime)
        {
            string format;
            if (longDate) format = longTime ? "F" : "f";
            else format = longTime ? "G" : "g";

            return FormatLocalizedDateTime(font, dateTime, format);
        }

        /// <summary>
        /// Usually a concatenation of the two above values.
        /// </summary>
        public static string FormatLocalizedDateAndTime(TMPro.TMP_FontAsset font, DateTime dateTime, bool longDate, bool longTime)
        {
            string format;
            if (longDate) format = longTime ? "F" : "f";
            else format = longTime ? "G" : "g";

            return FormatLocalizedDateTime(font, dateTime, format);
        }

        /// <summary>
        /// Gets the current <see cref="DisplayCulture"/>, but ensuring that
        /// it has number and datetime formatting that a specific font can display,
        /// falling back to InvariantCulture.
        /// </summary>
        public static CultureInfo GetDisplayCultureForFont(Font font)
        {
            CultureInfo displayCulture;
            if (!Statics.FontToDisplayCultureLookup.TryGetValue(font, out displayCulture))
            {
                displayCulture = DisplayCulture;
                displayCulture = (CultureInfo)displayCulture.Clone();
                if (!IsDateTimeFormatDisplayable(font, displayCulture.DateTimeFormat) && IsDateTimeFormatDisplayable(font, CultureInfo.InvariantCulture.DateTimeFormat))
                {
                    displayCulture.DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
                }
                ConvertNumberFormatInfo(font, displayCulture.NumberFormat, CultureInfo.InvariantCulture.NumberFormat);
                Statics.FontToDisplayCultureLookup[font] = displayCulture;
            }
            return displayCulture;
        }

        /// <summary>
        /// Gets the current <see cref="DisplayCulture"/>, but ensuring that
        /// it has number and datetime formatting that a specific font can display,
        /// falling back to InvariantCulture.
        /// </summary>
        public static CultureInfo GetDisplayCultureForFont(TMPro.TMP_FontAsset font)
        {
            CultureInfo displayCulture;
            if (!Statics.TMPFontToDisplayCultureLookup.TryGetValue(font, out displayCulture))
            {
                displayCulture = DisplayCulture;
                displayCulture = (CultureInfo)displayCulture.Clone();
                if (!IsDateTimeFormatDisplayable(font, displayCulture.DateTimeFormat) && IsDateTimeFormatDisplayable(font, CultureInfo.InvariantCulture.DateTimeFormat))
                {
                    displayCulture.DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
                }
                ConvertNumberFormatInfo(font, displayCulture.NumberFormat, CultureInfo.InvariantCulture.NumberFormat);
                Statics.TMPFontToDisplayCultureLookup[font] = displayCulture;
            }
            return displayCulture;
        }

        /// <summary>
        /// A case-sensitive string comparer for <see cref="DisplayCulture"/>.
        /// </summary>
        public static StringComparer GetStringComparer(bool ignoreCase)
        {
            return GetStringComparerForCulture(DisplayCulture, ignoreCase);
        }

        /// <summary>
        /// A case-sensitive string comparer for <see cref="DisplayCulture"/>.
        /// </summary>
        public static StringComparer GetStringComparerForCulture(CultureInfo cultureInfo, bool ignoreCase)
        {
            CultureBool key = new CultureBool(cultureInfo, ignoreCase);
            StringComparer comparer;
            if (!Statics.CultureAndCaseSensitivityToStringComparerLookup.TryGetValue(key, out comparer))
            {
                CultureInfo displayCulture = DisplayCulture;
                comparer = StringComparer.Create(displayCulture, ignoreCase);
                Statics.CultureAndCaseSensitivityToStringComparerLookup[key] = comparer;
            }
            return comparer;
        }

        private static bool IsDateTimeFormatDisplayable(Font font, DateTimeFormatInfo f)
        {
            return (IsDisplayble(font, f.AbbreviatedDayNames) &&
                IsDisplayble(font, f.AbbreviatedMonthGenitiveNames) &&
                IsDisplayble(font, f.AbbreviatedMonthNames) &&
                IsDisplayble(font, f.AMDesignator) &&
                IsDisplayble(font, f.DateSeparator) &&
                IsDisplayble(font, f.DayNames) &&
                IsDisplayble(font, f.FullDateTimePattern) &&
                IsDisplayble(font, f.LongDatePattern) &&
                IsDisplayble(font, f.LongTimePattern) &&
                IsDisplayble(font, f.MonthDayPattern) &&
                IsDisplayble(font, f.MonthGenitiveNames) &&
                IsDisplayble(font, f.MonthNames) &&
                IsDisplayble(font, f.NativeCalendarName) &&
                IsDisplayble(font, f.PMDesignator) &&
                IsDisplayble(font, f.RFC1123Pattern) &&
                IsDisplayble(font, f.ShortDatePattern) &&
                IsDisplayble(font, f.ShortestDayNames) &&
                IsDisplayble(font, f.ShortTimePattern) &&
                IsDisplayble(font, f.SortableDateTimePattern) &&
                IsDisplayble(font, f.TimeSeparator) &&
                IsDisplayble(font, f.UniversalSortableDateTimePattern) &&
                IsDisplayble(font, f.YearMonthPattern));
        }

        private static bool IsDateTimeFormatDisplayable(TMPro.TMP_FontAsset font, DateTimeFormatInfo f)
        {
            return (IsDisplayble(font, f.AbbreviatedDayNames) &&
                IsDisplayble(font, f.AbbreviatedMonthGenitiveNames) &&
                IsDisplayble(font, f.AbbreviatedMonthNames) &&
                IsDisplayble(font, f.AMDesignator) &&
                IsDisplayble(font, f.DateSeparator) &&
                IsDisplayble(font, f.DayNames) &&
                IsDisplayble(font, f.FullDateTimePattern) &&
                IsDisplayble(font, f.LongDatePattern) &&
                IsDisplayble(font, f.LongTimePattern) &&
                IsDisplayble(font, f.MonthDayPattern) &&
                IsDisplayble(font, f.MonthGenitiveNames) &&
                IsDisplayble(font, f.MonthNames) &&
                IsDisplayble(font, f.NativeCalendarName) &&
                IsDisplayble(font, f.PMDesignator) &&
                IsDisplayble(font, f.RFC1123Pattern) &&
                IsDisplayble(font, f.ShortDatePattern) &&
                IsDisplayble(font, f.ShortestDayNames) &&
                IsDisplayble(font, f.ShortTimePattern) &&
                IsDisplayble(font, f.SortableDateTimePattern) &&
                IsDisplayble(font, f.TimeSeparator) &&
                IsDisplayble(font, f.UniversalSortableDateTimePattern) &&
                IsDisplayble(font, f.YearMonthPattern));
        }

        private static void ConvertNumberFormatInfo(Font font, NumberFormatInfo original, NumberFormatInfo template)
        {
            original.CurrencyDecimalSeparator = ConvertToDisplayable(font, original.CurrencyDecimalSeparator, template.CurrencyDecimalSeparator);
            original.CurrencyGroupSeparator = ConvertToDisplayable(font, original.CurrencyGroupSeparator, template.CurrencyGroupSeparator);
            original.CurrencySymbol = ConvertToDisplayable(font, original.CurrencySymbol, template.CurrencySymbol);
            original.PositiveInfinitySymbol = ConvertToDisplayable(font, original.PositiveInfinitySymbol, template.PositiveInfinitySymbol);
            original.NegativeInfinitySymbol = ConvertToDisplayable(font, original.NegativeInfinitySymbol, template.NegativeInfinitySymbol);
            original.NaNSymbol = ConvertToDisplayable(font, original.NaNSymbol, template.NaNSymbol);
            original.NegativeSign = ConvertToDisplayable(font, original.NegativeSign, template.NegativeSign);
            original.NumberDecimalSeparator = ConvertToDisplayable(font, original.NumberDecimalSeparator, template.NumberDecimalSeparator);
            original.NumberGroupSeparator = ConvertToDisplayable(font, original.NumberGroupSeparator, template.NumberGroupSeparator);
            original.PercentDecimalSeparator = ConvertToDisplayable(font, original.PercentDecimalSeparator, template.PercentDecimalSeparator);
            original.PercentGroupSeparator = ConvertToDisplayable(font, original.PercentGroupSeparator, template.PercentGroupSeparator);
            original.PercentSymbol = ConvertToDisplayable(font, original.PercentSymbol, template.PercentSymbol);
            original.PerMilleSymbol = ConvertToDisplayable(font, original.PerMilleSymbol, template.PerMilleSymbol);
            original.PositiveSign = ConvertToDisplayable(font, original.PositiveSign, template.PositiveSign);
        }

        private static void ConvertNumberFormatInfo(TMPro.TMP_FontAsset font, NumberFormatInfo original, NumberFormatInfo template)
        {
            original.CurrencyDecimalSeparator = ConvertToDisplayable(font, original.CurrencyDecimalSeparator, template.CurrencyDecimalSeparator);
            original.CurrencyGroupSeparator = ConvertToDisplayable(font, original.CurrencyGroupSeparator, template.CurrencyGroupSeparator);
            original.CurrencySymbol = ConvertToDisplayable(font, original.CurrencySymbol, template.CurrencySymbol);
            original.PositiveInfinitySymbol = ConvertToDisplayable(font, original.PositiveInfinitySymbol, template.PositiveInfinitySymbol);
            original.NegativeInfinitySymbol = ConvertToDisplayable(font, original.NegativeInfinitySymbol, template.NegativeInfinitySymbol);
            original.NaNSymbol = ConvertToDisplayable(font, original.NaNSymbol, template.NaNSymbol);
            original.NegativeSign = ConvertToDisplayable(font, original.NegativeSign, template.NegativeSign);
            original.NumberDecimalSeparator = ConvertToDisplayable(font, original.NumberDecimalSeparator, template.NumberDecimalSeparator);
            original.NumberGroupSeparator = ConvertToDisplayable(font, original.NumberGroupSeparator, template.NumberGroupSeparator);
            original.PercentDecimalSeparator = ConvertToDisplayable(font, original.PercentDecimalSeparator, template.PercentDecimalSeparator);
            original.PercentGroupSeparator = ConvertToDisplayable(font, original.PercentGroupSeparator, template.PercentGroupSeparator);
            original.PercentSymbol = ConvertToDisplayable(font, original.PercentSymbol, template.PercentSymbol);
            original.PerMilleSymbol = ConvertToDisplayable(font, original.PerMilleSymbol, template.PerMilleSymbol);
            original.PositiveSign = ConvertToDisplayable(font, original.PositiveSign, template.PositiveSign);
        }

        private static string ConvertToDisplayable(Font font, string original, string template)
        {
            if (!IsDisplayble(font, original) && IsDisplayble(font, template))
            {
                return template;
            }
            return original;
        }

        private static string ConvertToDisplayable(TMPro.TMP_FontAsset font, string original, string template)
        {
            if (!IsDisplayble(font, original) && IsDisplayble(font, template))
            {
                return template;
            }
            return original;
        }

        public static bool IsDisplayble(Font font, IList<string> textList)
        {
            if (ReferenceEquals(textList, null)) return true;

            for (int i = 0; i < textList.Count; ++i)
            {
                if (!IsDisplayble(font, textList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDisplayble(TMPro.TMP_FontAsset font, IList<string> textList)
        {
            if (ReferenceEquals(textList, null)) return true;

            for (int i = 0; i < textList.Count; ++i)
            {
                if (!IsDisplayble(font, textList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDisplayble(Font font, string text)
        {
            if (ReferenceEquals(text, null)) return true;

            for (int i = 0; i < text.Length; ++i)
            {
                if (!IsDisplayble(font, text[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDisplayble(TMPro.TMP_FontAsset font, string text)
        {
            if (ReferenceEquals(text, null)) return true;

            for (int i = 0; i < text.Length; ++i)
            {
                if (!IsDisplayble(font, text[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Whether the specified character can be rendered by the given font.
        /// </summary>
        public static bool IsDisplayble(Font font, char character)
        {
            if (font.HasCharacter(character)) return true;

            switch (character)
            {
                case ' ':
                case '\u200B':
                case '\u00a0':
                case '\r':
                case '\n':
                case '\t':
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Whether the specified character can be rendered by the given font.
        /// </summary>
        public static bool IsDisplayble(TMPro.TMP_FontAsset font, char character)
        {
            if (font.HasCharacter(character)) return true;

            switch (character)
            {
                case ' ':
                case '\u200B':
                case '\u00a0':
                case '\r':
                case '\n':
                case '\t':
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a random CultureInfo for the given language.
        /// </summary>
        public static CultureInfo GetLanguageCulture(SystemLanguage language)
        {
            if (language == SystemLanguage.Unknown)
            {
                return null;
            }

            string culture;
            LanguageCodes.TryGetValue(language, out culture);
            return GetCultureFromName(culture);
        }

        /// <summary>
        /// Prints all the cultures to debug.
        /// </summary>
        public static void PrintCulturesShort()
        {
            //if (logger.IsInfoEnabled)
            //{
            //    using (Pooled<StringBuilder> pooled = RecyclingPool.SpawnStringBuilder())
            //    {
            //        StringBuilder builder = pooled.Value;

            //        builder.Append("LocaleManager.DisplayCulture (deduced from ");
            //        builder.Append(_cultureSource);
            //        builder.Append(", device specified \"");
            //        builder.Append(_deviceCultureString);
            //        builder.Append("\"): ");
            //        PrintCulture(builder, DisplayCulture, false);

            //        builder.Append("\nCultureInfo.InvariantCulture: ");
            //        PrintCulture(builder, CultureInfo.InvariantCulture, false);
            //        builder.Append("\nCultureInfo.CurrentCulture: ");
            //        PrintCulture(builder, CultureInfo.CurrentCulture, false);
            //        builder.Append("\nCultureInfo.CurrentUICulture: ");
            //        PrintCulture(builder, CultureInfo.CurrentUICulture, false);
            //        builder.Append("\nStarting CultureInfo.CurrentCulture: ");
            //        PrintCulture(builder, _originalCurrentCulture, false);
            //        builder.Append("\nStarting CultureInfo.CurrentUICulture: ");
            //        PrintCulture(builder, _originalUICulture, false);

            //        builder.Append("\nNeutral cultures: ");
            //        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            //        PrintCultures(builder, cultures);

            //        builder.Append("\nSpecific cultures: ");
            //        cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            //        PrintCultures(builder, cultures);

            //        logger.Info(builder.ToString());
            //    }
            //}
        }

        private static void PrintCultures(StringBuilder buffer, CultureInfo[] cultures)
        {
            bool first = true;
            for (int i = 0; i < cultures.Length; ++i)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    buffer.Append(", ");
                }
                PrintCulture(buffer, cultures[i], true);
            }
        }

        private static void PrintCulture(StringBuilder buffer, CultureInfo culture, bool brief)
        {
            if (culture == null)
            {
                buffer.Append("<null>");
            }
            else
            {
                buffer.Append(culture.Name);
                if (!brief)
                {
                    buffer.Append(" [");
                    buffer.Append(culture.EnglishName);
                    buffer.Append("]");
                }
            }
        }

        /// <summary>
        /// Wrapper class so the members do not get initialized too early.
        /// </summary>
        private static class Statics
        {
            public static Dictionary<SystemLanguage, string> LanguageCodes = new Dictionary<SystemLanguage, string>(EnumEqualityComparer<SystemLanguage>.Default)
            {
                { SystemLanguage.Afrikaans, "af" },
                { SystemLanguage.Arabic, "ar" },
                { SystemLanguage.Basque, "eu" },
                { SystemLanguage.Belarusian, "be" },
                { SystemLanguage.Bulgarian, "bg" },
                { SystemLanguage.Catalan, "ca" },
                { SystemLanguage.Chinese, "zh" },
                { SystemLanguage.ChineseSimplified, "zh-Hans" },
                { SystemLanguage.ChineseTraditional, "zh-Hant" },
                { SystemLanguage.Czech, "cs" },
                { SystemLanguage.Danish, "da" },
                { SystemLanguage.Dutch, "nl" },
                { SystemLanguage.English, "en" },
                { SystemLanguage.Estonian, "et" },
                { SystemLanguage.Faroese, "fo" },
                { SystemLanguage.Finnish, "fi" },
                { SystemLanguage.French, "fr" },
                { SystemLanguage.German, "de" },
                { SystemLanguage.Greek, "el" },
                { SystemLanguage.Hebrew, "he" },
                { SystemLanguage.Hungarian, "hu" },
                { SystemLanguage.Icelandic, "is" },
                { SystemLanguage.Indonesian, "id" },
                { SystemLanguage.Italian, "it" },
                { SystemLanguage.Japanese, "ja" },
                { SystemLanguage.Korean, "ko" },
                { SystemLanguage.Latvian, "lv" },
                { SystemLanguage.Lithuanian, "lt" },
                { SystemLanguage.Norwegian, "no" },
                { SystemLanguage.Polish, "pl" },
                { SystemLanguage.Portuguese, "pt" },
                { SystemLanguage.Romanian, "ro" },
                { SystemLanguage.Russian, "ru" },
                { SystemLanguage.SerboCroatian, "sh" },
                { SystemLanguage.Slovak, "sk" },
                { SystemLanguage.Slovenian, "sl" },
                { SystemLanguage.Spanish, "es" },
                { SystemLanguage.Swedish, "sv" },
                { SystemLanguage.Thai, "th" },
                { SystemLanguage.Turkish, "tr" },
                { SystemLanguage.Ukrainian, "uk" },
                { SystemLanguage.Vietnamese, "vi" },
            };

            public static Dictionary<Font, CultureInfo> FontToDisplayCultureLookup = new Dictionary<Font, CultureInfo>();
            public static Dictionary<TMPro.TMP_FontAsset, CultureInfo> TMPFontToDisplayCultureLookup = new Dictionary<TMPro.TMP_FontAsset, CultureInfo>();
            public static Dictionary<CultureBool, StringComparer> CultureAndCaseSensitivityToStringComparerLookup = new Dictionary<CultureBool, StringComparer>();
        }

        /// <summary>
        /// A tuple struct that only contains immutable data.
        /// </summary>
        private struct CultureBool : IEquatable<CultureBool>, IArrayConvertible
        {

            /// <summary>
            /// A tuple member.
            /// </summary>
            public readonly CultureInfo Item1;
            /// <summary>
            /// A tuple member.
            /// </summary>
            public readonly bool Item2;

            /// <summary>
            /// Creates a new key tuple.
            /// </summary>
            /// <param name="item1">A tuple member.</param>
            /// <param name="item2">A tuple member.</param>
            public CultureBool(CultureInfo item1, bool item2)
            {
                Item1 = item1;
                Item2 = item2;
            }

            /// <summary>
            /// Compare two key tuples for equality.
            /// </summary>
            /// <param name="a">A key tuple.</param>
            /// <param name="b">Another key tuple.</param>
            /// <returns>true if equal, else false.</returns>
            public static bool operator ==(CultureBool a, CultureBool b)
            {
                return a.Equals(b);
            }

            /// <summary>
            /// Compare two key tuples for inequality.
            /// </summary>
            /// <param name="a">A key tuple.</param>
            /// <param name="b">Another key tuple.</param>
            /// <returns>true if not equal, else false.</returns>
            public static bool operator !=(CultureBool a, CultureBool b)
            {
                return !a.Equals(b);
            }

            /// <summary>
            /// Compares against another object for equality.
            /// </summary>
            /// <param name="obj">The object to compare with.</param>
            /// <returns>true if equal, else false.</returns>
            public bool Equals(CultureBool obj)
            {
                return (SafeEqualityComparer<CultureInfo>.Default.Equals(Item1, obj.Item1) &&
                    SafeEqualityComparer<bool>.Default.Equals(Item2, obj.Item2));
            }

            /// <summary>
            /// Compares against another object for equality.
            /// </summary>
            /// <param name="obj">The object to compare with.</param>
            /// <returns>true if equal, else false.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is CultureBool))
                {
                    return false;
                }
                return (this == (CultureBool)obj);
            }

            /// <summary>
            /// Gets a hash code for this tuple.
            /// </summary>
            /// <returns>The hash code for this tuple.</returns>
            public override int GetHashCode()
            {
                int hash = SafeEqualityComparer<CultureInfo>.Default.GetHashCode(Item1);
                hash = BitUtil.CombineHashCodes(hash, (Item2 ? 1 : 0));
                return hash;
            }

            /// <summary>
            /// Converts this tuple to object array.
            /// </summary>
            /// <returns>This tuple expressed as an object array.</returns>
            public object[] ToArray()
            {
                return new object[] { Item1, Item2 };
            }

            /// <summary>
            /// Converts this request to a string representation.
            /// </summary>
            /// <returns>The string representation of this object.</returns>
            public override string ToString()
            {
                return "FontBool(" + Item1 + ", " + Item2 + ")";
            }
        }
    }
}