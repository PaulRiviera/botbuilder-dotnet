﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    /// <summary>
    /// Step which calls another dialog
    /// </summary>
    public abstract class BaseCallDialog : Dialog, IDialogDependencies
    {
        protected string dialogIdToCall;

        /// <summary>
        /// configurable options for the dialog
        /// </summary>
        public object Options { get; set; }

        /// <summary>
        /// The dialog to call
        /// </summary>
        public IDialog Dialog { get; set; }

        /// <summary>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
        public override string Property
        {
            get
            {
                return InputBindings["value"];
            }
            set
            {
                InputBindings["value"] = value;
                OutputBinding = value;
            }
        }

        public BaseCallDialog(string dialogIdToCall = null, string id = null, string property = null, object options = null) 
            : base()
        {
            this.dialogIdToCall = dialogIdToCall;
            this.OutputBinding = "dialog.lastResult";

            if (options != null)
            {
                this.Options = options;
            }

            if (!string.IsNullOrEmpty(property))
            {
                Property = property;
            }

            Id = id;
        }

        protected override string OnComputeId()
        {
            return $"${this.GetType().Name}[{Dialog?.Id ?? this.dialogIdToCall}:{this.BindingPath()}]";
        }

        protected IDialog resolveDialog(DialogContext dc)
        {
            var dialog = this.Dialog;
            if (dialog == null && !String.IsNullOrEmpty(this.dialogIdToCall))
            {
                dialog = dc.FindDialog(this.dialogIdToCall);
            }

            var dialogId = dialog?.Id ?? throw new Exception($"{this.GetType().Name} requires a dialog to be called.");
            return dialog;
        }

        public List<IDialog> ListDependencies()
        {
            if (Dialog != null)
            {
                return new List<IDialog>() { Dialog };
            }
            return new List<IDialog>();
        }
    }
}
