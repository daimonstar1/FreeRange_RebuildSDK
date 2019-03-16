//#define FRG_SHOW_PREVIEW_STRINGS

using System;
using System.Globalization;
using UnityEngine;

namespace FRG.Core
{
    public partial class LocalizedStringEntry : ManagedEntry, IFormattable
    {


        private static SystemLanguage _currentLanguage = SystemLanguage.Unknown;
        public static SystemLanguage CurrentLanguage
        {
            get
            {
                SystemLanguage language = _currentLanguage;
                if (language == SystemLanguage.Unknown) language = Application.systemLanguage;
                if (language == SystemLanguage.Unknown) language = SystemLanguage.English;
                if (language == SystemLanguage.Chinese) language = SystemLanguage.ChineseSimplified;
                return language;
            }

            set
            {
                _currentLanguage = value;
            }
        }

        public static CultureInfo DisplayCulture
        {
            get
            {
#if !GAME_SERVER
                return FRG.Core.LocaleManager.DisplayCulture;
#else
                return CultureInfo.InvariantCulture;
#endif
            }
        }

        //[InspectorComment(ConditionMethodName = "IsEnglishEmpty", CommentText = "You must specify a valid english string.", Icon = InspectorCommentIcon.Error)]
        [TextArea, InspectorLabelDisplay(false)]
        [SerializeField]
        [InspectorDeepCopy]
        private string _english = "";

        [InspectorComment(ConditionMethodName = "HasEmptyLanguage", CommentText = "You must specify a valid string for every specified language.", Icon = InspectorCommentIcon.Error)]
        [SerializeField]
        LanguageInfo[] _languages = ArrayUtil.Empty<LanguageInfo>();

        public string English { get { return _english ?? ""; } }

        /// <summary>
        /// When false, this asset won't be built or returned from <see cref="EntryManager"/>.
        /// </summary>
        public override bool IsValid {
            get {
                if (!base.IsValid) { return false; }

                //if (IsEnglishEmpty()) {
                //    return false;
                //}
                if (HasEmptyLanguage()) {
                    return false;
                }

                return true;
            }
        }

        private bool IsEnglishEmpty()
        {
            return string.IsNullOrEmpty(English);
        }

        private bool HasEmptyLanguage()
        {
            foreach (LanguageInfo info in _languages) {
                if (string.IsNullOrEmpty(info.Text)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if there is localized text for the specified language.
        /// </summary>
        /// <param name="language">The language to check.</param>
        /// <returns></returns>
        public bool IsLocalizedForLanguage(SystemLanguage language)
        {
            return !string.IsNullOrEmpty(GetLanguageInfo(language).Text);
        }

        /// <summary>
        /// Gets the localized string for this entry.
        /// </summary>
        public string ToLocalizedString()
        {
            return RenderRuntimeString(null, CurrentLanguage);
        }

        /// <summary>
        /// Gets the localized string for this entry.
        /// </summary>
        public string ToLocalizedString(SystemLanguage language)
        {
            return RenderRuntimeString(null, language);
        }

        /// <summary>
        /// Gets the localized string for this entry.
        /// </summary>
        public string ToLocalizedString(IFormatProvider provider, SystemLanguage language)
        {
            return RenderRuntimeString(provider, language);
        }

        /// <summary>
        /// Formats a localized string with replacement parameters at runtime.
        /// </summary>
        public string FormatLocalizedString(params object[] args)
        {
            return RenderRuntimeString(null, CurrentLanguage, args);
        }

        /// <summary>
        /// Formats a localized string with replacement parameters at runtime.
        /// </summary>
        public string FormatLocalizedString(IFormatProvider provider, params object[] args)
        {
            return RenderRuntimeString(provider, CurrentLanguage, args);
        }

        /// <summary>
        /// Formats a localized string with replacement parameters at runtime.
        /// </summary>
        public string FormatLocalizedString(IFormatProvider provider, SystemLanguage language, params object[] args)
        {
            return RenderRuntimeString(provider, language, args);
        }

        /// <summary>
        /// Formats an unlocalized string with replacement parameters at runtime.
        /// </summary>
        public static string FormatUnlocalizedString(IFormatProvider provider, string text, params object[] args)
        {
            provider = provider ?? DisplayCulture;

            try
            {
                args = args ?? ArrayUtil.Empty<object>();
                return string.Format(provider, text, args);
            }
            catch (Exception e)
            {
                if (!ReflectionUtil.IsStringFormatException(e)) throw;

                //logger.Warn("Unable to format string.", e);
                return "";
            }
        }

        /// <summary>
        /// Gets a localized string that will not spew errors in the inspector.
        /// </summary>
        /// <remarks>
        /// Should not be called at all in final builds, but will work just in case.
        /// </remarks>
        public string PreviewLocalizedString(IFormatProvider provider, SystemLanguage language)
        {
#if FRG_SHOW_PREVIEW_STRINGS
            string localizedText = RenderDefaultString(SystemLanguage.English);
            ShowPreviewString("PREVIEW_LOCALIZED", localizedText);
#else
            return RenderPreviewString(provider, language);
#endif
        }
        
        /// <summary>
        /// Formats a localized string with replacement parameters in a way that will not spew errors in the inspector.
        /// </summary>
        /// <remarks>
        /// Should not be called at all in final builds, but will work just in case.
        /// </remarks>
        public string PreviewLocalizedString(IFormatProvider provider, SystemLanguage language, string[] previewArgs)
        {
#if FRG_SHOW_PREVIEW_STRINGS
            string localizedText = RenderDefaultString(SystemLanguage.English);
            return ShowPreviewString("PREVIEW_LOCALIZED", localizedText, previewArgs);
#else
            return RenderPreviewString(provider, language, previewArgs);
#endif
        }

        /// <summary>
        /// Gets an unlocalized string but shows it as a preview string.
        /// </summary>
        /// <remarks>
        /// Should not be called at all in final builds, but will work just in case.
        /// </remarks>
        public static string PreviewUnlocalizedString(IFormatProvider provider, string unlocalizedText)
        {
#if FRG_SHOW_PREVIEW_STRINGS
            return ShowPreviewString("PREVIEW_UNLOCALIZED", unlocalizedText);
#else
            return unlocalizedText;
#endif
        }

        /// <summary>
        /// Renders an unlocalized string in a very safe way with default arguments, if possible.
        /// </summary>
        /// <remarks>
        /// Should not be called at all in final builds, but will be present just in case.
        /// </remarks>
        public static string PreviewUnlocalizedString(IFormatProvider provider, string unlocalizedText, string[] previewArgs)
        {
#if FRG_SHOW_PREVIEW_STRINGS
            ShowPreviewString("PREVIEW_UNLOCALIZED", unlocalizedText, previewArgs);
#else
            // Weird covariant array cast; may need to replace.
            object[] args = previewArgs ?? ArrayUtil.Empty<object>();

            try
            {
                return string.Format(provider, unlocalizedText, args);
            }
            catch (Exception e)
            {
                if (!ReflectionUtil.IsStringFormatException(e)) throw;

                // Fall through
            }

            const int ExtendedLength = 16;
            if (previewArgs.Length < ExtendedLength)
            {
                try
                {
                    object[] extended = new object[ExtendedLength];
                    for (int i = 0; i < previewArgs.Length; ++i)
                    {
                        extended[i] = previewArgs[i] ?? "";
                    }
                    for (int i = previewArgs.Length; i < extended.Length; ++i)
                    {
                        extended[i] = "";
                    }

                    return string.Format(provider, unlocalizedText, extended);
                }
                catch (Exception e)
                {
                    if (!ReflectionUtil.IsStringFormatException(e)) throw;

                    // Fall through
                }
            }
            
            string result = "";
#if FRG_SHOW_PREVIEW_STRINGS
            result = ShowPreviewString("INVALID_PREVIEW_STRING", unlocalizedText, previewArgs);
#endif
            return result;
#endif
        }

        protected virtual string RenderRuntimeString(IFormatProvider provider, SystemLanguage language)
        {
            return RenderDefaultString(language);
        }

        protected virtual string RenderRuntimeString(IFormatProvider provider, SystemLanguage language, object[] args)
        {
            string localizedText = RenderDefaultString(language);
            return FormatUnlocalizedString(provider, localizedText, args);
        }

        protected virtual string RenderPreviewString(IFormatProvider provider, SystemLanguage language)
        {
            return RenderDefaultString(language);
        }

        protected virtual string RenderPreviewString(IFormatProvider provider, SystemLanguage language, string[] previewArgs)
        {
            string localizedText = RenderDefaultString(language);
            return PreviewUnlocalizedString(provider, localizedText, previewArgs);
        }

        protected string RenderDefaultString(SystemLanguage language)
        {
            LanguageInfo entry = GetLanguageInfo(language);
            // Both default and empty entries are ignored.
            if (!string.IsNullOrEmpty(entry.Text))
            {
                return entry.Text;
            }

            // Default to english if not localized
            return _english ?? "";
        }

        private static string ShowPreviewString(string infoText, string unlocalizedText)
        {
            return infoText + "(\"" + unlocalizedText + "\")";
        }

        private static string ShowPreviewString(string infoText, string unlocalizedText, string[] previewArgs)
        {
            string argTerm;
            if (previewArgs == null)
            {
                argTerm = "<null>";
            }
            else if (previewArgs.Length == 0)
            {
                argTerm = "[]";
            }
            else
            {
                argTerm = "[\"" + ArrayUtil.Join("\", \"", previewArgs) + "\"]";
            }

            return infoText + "(\"" + unlocalizedText + "\", " + argTerm + ")";
        }

        private LanguageInfo GetLanguageInfo(SystemLanguage language)
        {
            if (language == SystemLanguage.English)
            {
                return new LanguageInfo(SystemLanguage.English, _english);
            }

            foreach (LanguageInfo info in _languages)
            {
                if (info.Language == language && !string.IsNullOrEmpty(info.Text))
                {
                    return info;
                }
            }

            return new LanguageInfo(SystemLanguage.Unknown, "");
        }

#if UNITY_EDITOR
        internal void SetLocalizedString(SystemLanguage language, string text)
        {
            if (language == SystemLanguage.Unknown || Array.IndexOf(ReflectionUtil.GetEnumValues<SystemLanguage>(), language) < 0)
            {
                throw new ArgumentException("Unknown system language: " + language.ToString(), "language");
            }

            text = text ?? "";

            if (language == SystemLanguage.English)
            {
                _english = text;
                return;
            }

            LanguageInfo info = new LanguageInfo(language, text);

            int index = -1;
            for (int i = 0; i < _languages.Length; ++i)
            {
                if (_languages[i].Language == info.Language)
                {
                    index = i;
                }
            }
            if (index < 0)
            {
                Array.Resize(ref _languages, _languages.Length + 1);
                index = _languages.Length;
            }

            _languages[index] = info;
            Array.Sort(_languages, (a, b) => a.Language.CompareTo(b.Language));
        }
#endif

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            // Ignore format spec (makes this object work as a string.Format argument)

            // Don't actually format; this is called when we stick a localized string in a parameter array.
            return ToLocalizedString();
        }

        [Serializable]
        public struct LanguageInfo
        {
            public SystemLanguage Language;

            [TextArea, InspectorLabelDisplay(false)]
            public string  Text;

            public LanguageInfo(SystemLanguage language, string text)
            {
                Language = language;
                Text = text;
            }
        }
    }
}
