﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.Completion
{
    internal static class CompletionOptions
    {
        public const string FeatureName = "Completion";

        [ExportOption]
        public static readonly PerLanguageOption<bool> HideAdvancedMembers = new PerLanguageOption<bool>(FeatureName, "HideAdvancedMembers", defaultValue: false);

        [ExportOption]
        public static readonly PerLanguageOption<bool> IncludeKeywords = new PerLanguageOption<bool>(FeatureName, "IncludeKeywords", defaultValue: true);

        [ExportOption]
        public static readonly PerLanguageOption<bool> TriggerOnTyping = new PerLanguageOption<bool>(FeatureName, "TriggerOnTyping", defaultValue: true);

        [ExportOption]
        public static readonly PerLanguageOption<bool> TriggerOnTypingLetters = new PerLanguageOption<bool>(FeatureName, "TriggerOnTypingLetters", defaultValue: true);
    }
}
