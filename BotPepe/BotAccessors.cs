// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using BotPepe.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BotPepe
{
    /// <summary>
    /// This class is created as a Singleton and passed into the IBot-derived constructor.
    ///  - See <see cref="BotPepe"/> constructor for how that is injected.
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// </summary>
    public class BotAccessors
    {
        // The property accessor keys to use.
        public const string UsuarioAccessorName = "BotPepe.Usuario";
        public const string DialogStateAccessorName = "BotPepe.DialogState";

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAccessors"/> class.
        /// Contains the <see cref="ConversationState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>
        /// <param name="conversationState">The state object that stores the counter.</param>
        public BotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            //Usuario= usuario ?? throw new ArgumentNullException(nameof(usuario));
        }

        // The name of the dialog state.
        public static readonly string DialogStateName = $"{nameof(BotAccessors)}.DialogState";

        // The name of the command state.
        public static readonly string CommandStateName = $"{nameof(BotAccessors)}.CommandState";

        /// <summary>Gets or sets the state property accessor for the user information we're tracking.</summary>
        /// <value>Accessor for user information.</value>

        /// <summary>Gets or sets the state property accessor for the dialog state.</summary>
        /// <value>Accessor for the dialog state.</value>
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        /// <summary>Gets the conversation state for the bot.</summary>
        /// <value>The conversation state for the bot.</value>
        public ConversationState ConversationState { get; }
        public IStatePropertyAccessor<string> CommandState { get; set; }

        /// <summary>Gets the user state for the bot.</summary>
        /// <value>The user state for the bot.</value>
        public UserState UserState { get; }

        public IStatePropertyAccessor<Usuario> UserProfile { get; set; }
        public Usuario Usuario { get; set; }
    }
}
