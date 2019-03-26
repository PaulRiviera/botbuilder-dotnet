﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogState
    {
        public DialogState()
            : this(null)
        {
        }

        public DialogState(List<DialogInstance> stack)
        {
            DialogStack = stack ?? new List<DialogInstance>();
            ConversationState = new Dictionary<string, object>();
            UserState = new Dictionary<string, object>();
        }

        public List<DialogInstance> DialogStack { get; set; }

        public Dictionary<string, object> ConversationState { get; set; }
        public Dictionary<string, object> UserState { get; set; }
    }
}