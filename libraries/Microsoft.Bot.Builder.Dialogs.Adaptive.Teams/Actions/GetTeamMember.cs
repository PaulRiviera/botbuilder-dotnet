﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Calls TeamsInfo.GetTeamMember to retrieve the list of members in a teams
    /// scoped conversation and sets the result to a memory property.
    /// </summary>
    public class GetTeamMember : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.GetTeamMember";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTeamMember"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public GetTeamMember([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to use for member id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for member id. Default value is turn.activity.from.id.
        /// </value>
        [JsonProperty("memberId")]
        public StringExpression MemberId { get; set; } = "=turn.activity.from.id";

        /// <summary>
        /// Gets or sets the expression to get the value to use for team id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for team id. Default value is turn.activity.channelData.team.id.
        /// </value>
        [JsonProperty("teamId")]
        public StringExpression TeamId { get; set; } = "=turn.activity.channelData.team.id";

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }
            
            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.ChannelId != Channels.Msteams)
            {
                throw new Exception("TeamsInfo.GetMember() works only on the Teams channel.");
            }

            string memberId = null;
            if (MemberId != null)
            {
                var (value, valueError) = MemberId.TryGetValue(dc.State);
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {MemberId.ExpressionText}. Error: {valueError}");
                }

                memberId = value as string;
            }

            if (string.IsNullOrEmpty(memberId))
            {
                throw new InvalidOperationException($"Missing {nameof(MemberId)} in {nameof(GetTeamMember)}");
            }

            string teamId = null;
            if (TeamId != null)
            {
                var (value, valueError) = TeamId.TryGetValue(dc.State);
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {TeamId.ExpressionText}. Error: {valueError}");
                }

                teamId = value as string;
            }

            var result = await TeamsInfo.GetTeamMemberAsync(dc.Context, memberId, teamId, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (this.Property != null)
            {
                dc.State.SetValue(this.Property.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.MemberId?.ToString() ?? string.Empty},{this.TeamId?.ToString() ?? string.Empty},{this.Property?.ToString() ?? string.Empty}]";
        }
    }
}