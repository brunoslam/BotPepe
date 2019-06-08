// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BotPepe.Dialogs;
using BotPepe.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using BotPepe.Controller;
using Microsoft.Bot.Builder.Teams;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace BotPepe
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class BotPepe : IBot
    {

        private const string FAQDialogID = "faqDialog";
        private const string WelcomeDialogID = "SendWelcomeMessage";

        private const string SharePointDialogID = "sharepointDialog";
        private const string OutlookDialogID = "outlookDialog";
        private const string Office365DialogID = "office365Dialog";

        // The connection name here must match the the one from
        // your Bot Channels Registration on the settings blade in Azure.
        private const string ConnectionSettingName = "OauthBot";

        private const string WelcomeText =
            @"Aloha! Tienes duda en una de estas tecnologias?";

        // Define the dialog set for the bot.
        private readonly DialogSet _dialogs;

        private readonly BotAccessors _accessors;
        private readonly ILogger _logger;
        private readonly BotServices botServices;

        private bool nuevaConversacion;


        /// <summary>
        /// Initializes a new instance of the <see cref="BotPepe"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public BotPepe(BotServices services, BotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<BotPepe>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            botServices = services ?? throw new System.ArgumentNullException(nameof(services));

            if (!botServices.QnAServices.ContainsKey("PepeQnA"))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a QnA service named PepeQnA'. holamundo " + botServices.QnAServices);
            }
            if (!botServices.QnAServices.ContainsKey("QnALS"))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a QnA service named PepeQnA'. holamundo " + botServices.QnAServices);
            }

            if (!botServices.LuisServices.ContainsKey("PepeLuis"))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a Luis service named PepeLuis'.");
            }

            // Defining dialogs
            _dialogs = new DialogSet(accessors.DialogStateAccessor)
                //.Add(new BookingDialog(BookingDialogID))
                //.Add(new SoporteDialog(SoporteDialogID))
                .Add(new SharePointDialog(SharePointDialogID))
                .Add(new OutlookDialog(OutlookDialogID, _accessors))
                .Add(new Office365Dialog(Office365DialogID, _accessors))
                //.Add(OAuthHelpers.Prompt(ConnectionSettingName))
                .Add(new ChoicePrompt("choicePrompt"))
                .Add(new WaterfallDialog("SendWelcomeMessage", new WaterfallStep[] { ConsultarTecnologiaAsync, DerivarTecnologiaAsync }))
                .Add(new ChoicePrompt("name"))

                .Add(new ConfirmPrompt("confirm"));


        }
        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            DialogContext dc = null;
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            var asdd = turnContext.Activity.ChannelData;

            if (teamsContext != null)
            {
                // Now fetch the Team ID, Channel ID, and Tenant ID off of the incoming activity
                var incomingTeamId = teamsContext.Team.Id;
                var incomingChannelid = teamsContext.Channel.Id;
                var incomingTenantId = teamsContext.Tenant.Id;

                SigninStateVerificationQuery asd = teamsContext.GetSigninStateVerificationQueryData();

                // Make an operation call to fetch the list of channels in the team, and print count of channels.
                var channels = await teamsContext.Operations.FetchChannelListAsync(incomingTeamId, cancellationToken: cancellationToken);
                await turnContext.SendActivityAsync($"You have {channels.Conversations.Count} channels in this team");

                // Make an operation call to fetch details of the team where the activity was posted, and print it.
                var teamInfo = await teamsContext.Operations.FetchTeamDetailsAsync(incomingTeamId);
                await turnContext.SendActivityAsync($"Name of this team is {teamInfo.Name} and group-id is {teamInfo.AadGroupId}");
            }



            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                // Get the state properties from the turn context.
                Usuario userProfile =
                    await _accessors.UserProfile.GetAsync(turnContext, () => new Usuario());

                if (userProfile.EsperaRespuestaQNA || turnContext.Activity.Text.ToLower() == "ayuda")
                {
                    var recognizerResult = await botServices.LuisServices["DispatchPepe"].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();
                    await DispatchToTopIntentAsync(turnContext, dc, topIntent, cancellationToken);
                }
                // Continue any current dialog.
                DialogTurnResult dialogTurnResult = await dc.ContinueDialogAsync();

                

                if (dialogTurnResult.Status == DialogTurnStatus.Empty)
                //if (dialogTurnResult.Result is null)
                {
                    // Get the intent recognition result
                    var recognizerResult = await botServices.LuisServices["DispatchPepe"].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();

                    if (topIntent == null)
                    {
                        await turnContext.SendActivityAsync("Unable to get the top intent.");
                    }
                    else
                    {
                        await DispatchToTopIntentAsync(turnContext, dc, topIntent, cancellationToken);
                    }
                }else if (dialogTurnResult.Status == DialogTurnStatus.Complete)
                {
                    await dc.BeginDialogAsync("SendWelcomeMessage", cancellationToken);
                }
                //COMBINAR CON ESTO
                //
                //await ProcessInputAsync(turnContext, cancellationToken);




            }
            else if (turnContext.Activity.Type == ActivityTypes.Invoke || turnContext.Activity.Type == ActivityTypes.Event)
            {
                // This handles the Microsoft Teams Invoke Activity sent when magic code is not used.
                // See: https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/authentication/auth-oauth-card#getting-started-with-oauthcard-in-teams
                // Manifest Schema Here: https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema
                // It also handles the Event Activity sent from The Emulator when the magic code is not used.
                // See: https://blog.botframework.com/2018/08/28/testing-authentication-to-your-bot-using-the-bot-framework-emulator/

                // Sanity check the activity type and channel Id.
                if (turnContext.Activity.Type == ActivityTypes.Invoke && turnContext.Activity.ChannelId != "msteams")
                {
                    throw new InvalidOperationException("The Invoke type is only valid onthe MSTeams channel.");
                }

                dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync(OutlookDialogID);
                }

            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Send a welcome message to the user and tell them what actions they may perform to use this bot
                if (turnContext.Activity.MembersAdded != null)
                {
                    dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                    //await dc.BeginDialogAsync("SendWelcomeMessageAsync");
                    var reply = turnContext.Activity.CreateReply();
                    reply.Type = ActivityTypes.Typing;
                    await dc.Context.SendActivityAsync(reply);

                    if (turnContext.Activity.MembersAdded != null)
                    {
                        foreach (var member in turnContext.Activity.MembersAdded)
                        {
                            if (member.Id != turnContext.Activity.Recipient.Id)
                            {
                                this.nuevaConversacion = true;
                                //await SendWelcomeMessageAsync(turnContext, cancellationToken, true);
                                await dc.BeginDialogAsync("SendWelcomeMessage");
                            }
                        }
                    }
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.DeleteUserData)
            {
                string idChannel = turnContext.Activity.ChannelId;
                string idChannelx = turnContext.Activity.Conversation.AadObjectId;
                string idChannelxd = turnContext.Activity.MembersAdded.First().Id;
                await _accessors.UserState.DeleteAsync(turnContext, cancellationToken);
                await _accessors.ConversationState.DeleteAsync(turnContext, cancellationToken);
                //context.Reset();
                //context.ConversationData.Clear();
                //context.UserData.Clear();
                //context.PrivateConversationData.Clear();
                //await context.FlushAsync(CancellationToken.None);
            }
            else if (turnContext.Activity.Type == ActivityTypes.Typing)
            {

            }
            else if (turnContext.Activity.Type == ActivityTypes.Handoff)
            {

            }
            else if (turnContext.Activity.Type == ActivityTypes.Trace)
            {

            }
            else if (turnContext.Activity.Type == ActivityTypes.Suggestion)
            {

            }

            //else if (turnContext.Activity.Type == ActivityTypes.)
            //{

            //}
            // Save the new turn count into the conversation state.
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        /// <summary>
        /// Depending on the intent from Dispatch, routes to the right LUIS model or QnA service.
        /// </summary>
        private async Task DispatchToTopIntentAsync(ITurnContext context, DialogContext dc, (string intent, double score)? topIntent, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string luisO365DispatchKey = "l_Office365Luis";
            const string noneDispatchKey = "None";
            const string qnaDispatchKey = "q_PepeQnA";
            const string qnaLSDispatchKey = "q_QnALS";

            //var dc = await _dialogs.CreateContextAsync(context, cancellationToken);

            //var dc = await _dialogs.CreateContextAsync(context, cancellationToken);
            if (context.Activity.Text.ToLowerInvariant() == "signout" ||
                context.Activity.Text.ToLowerInvariant() == "logout" ||
                context.Activity.Text.ToLowerInvariant() == "signoff" ||
                context.Activity.Text.ToLowerInvariant() == "logoff" ||
                context.Activity.Text.ToLowerInvariant() == "cerrar sesión")
            {

                // The bot adapter encapsulates the authentication processes and sends
                // activities to from the Bot Connector Service.
                var botAdapter = (BotFrameworkAdapter)context.Adapter;
                await botAdapter.SignOutUserAsync(context, ConnectionSettingName, cancellationToken: cancellationToken);

                // Let the user know they are signed out.
                await context.SendActivityAsync("Has cerrado sesión correctamente.", cancellationToken: cancellationToken);

            }
            else if (context.Activity.Text.ToLowerInvariant() == "yo")
            {
                //await OAuthHelpers.ListMeAsync(context, cancellationToken);
                await context.SendActivityAsync("Crear conversación yo...", cancellationToken: cancellationToken);
            }
            else if (context.Activity.Text.ToLowerInvariant() == "ayuda")
            {
                //await OAuthHelpers.ListMeAsync(context, cancellationToken);
                //await SendWelcomeMessageAsync(context, cancellationToken, false);
                await dc.BeginDialogAsync("SendWelcomeMessage");
            }
            else
            {
                switch (topIntent.Value.intent)
                {
                    case noneDispatchKey:
                        await context.SendActivityAsync($"None intent: {topIntent.Value.intent} ({topIntent.Value.score}).");
                        break;
                    // You can provide logic here to handle the known None intent (none of the above).
                    // In this example we fall through to the QnA intent.
                    case qnaDispatchKey:
                        await DispatchToQnAMakerAsync(context, "PepeQnA", dc);
                        break;
                    case qnaLSDispatchKey:
                        await DispatchToQnAMakerAsync(context, "QnALS", dc);
                        break;
                    case luisO365DispatchKey:
                        await DispatchToLuisO365ModelAsync(context, "Office365Luis", dc);
                        break;
                    case "signout":
                    case "logout":
                    case "signoff":
                    case "logoff":
                        // The bot adapter encapsulates the authentication processes and sends
                        // activities to from the Bot Connector Service.
                        var botAdapter = (BotFrameworkAdapter)context.Adapter;
                        await botAdapter.SignOutUserAsync(context, ConnectionSettingName, cancellationToken: cancellationToken);

                        // Let the user know they are signed out.
                        await context.SendActivityAsync("You are now signed out.", cancellationToken: cancellationToken);
                        break;
                    default:
                        // The intent didn't match any case, so just display the recognition results.
                        await context.SendActivityAsync($"Dispatch intent: {topIntent.Value.intent} ({topIntent.Value.score}).");
                        break;

                }

            }


            /*
             * 
             * var dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            switch (turnContext.Activity.Text.ToLowerInvariant())
            {
                case "signout":
                case "logout":
                case "signoff":
                case "logoff":
                    // The bot adapter encapsulates the authentication processes and sends
                    // activities to from the Bot Connector Service.
                    var botAdapter = (BotFrameworkAdapter)turnContext.Adapter;
                    await botAdapter.SignOutUserAsync(turnContext, ConnectionSettingName, cancellationToken: cancellationToken);

                    // Let the user know they are signed out.
                    await turnContext.SendActivityAsync("You are now signed out.", cancellationToken: cancellationToken);
                    break;
                case "help":
                    await turnContext.SendActivityAsync(WelcomeText, cancellationToken: cancellationToken);
                    break;
                default:
                    // The user has input a command that has not been handled yet,
                    // begin the waterfall dialog to handle the input.
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("graphDialog", cancellationToken: cancellationToken);
                    }

                    break;
            }

            return dc;*/
        }

        /// <summary>
        /// Dispatches the turn to the request QnAMaker app.
        /// </summary>
        private async Task DispatchToQnAMakerAsync(ITurnContext context, string appName, DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(context.Activity.Text))
            {
                var results = await botServices.QnAServices[appName].GetAnswersAsync(context).ConfigureAwait(false);
                if (results.Any())
                {
                    await context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
                }
                else
                {
                    await context.SendActivityAsync($"Couldn't find an answer in the {appName}.");
                }
            }
        }

        /// <summary>
        /// Dispatches the turn to the requested LUIS model.
        /// </summary>
        private async Task<DialogContext> DispatchToLuisO365ModelAsync(ITurnContext context, string appName, DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await botServices.LuisServices[appName].RecognizeAsync(context, cancellationToken);
            var intent = result.Intents?.FirstOrDefault();

            if (intent?.Key?.ToString() == "SharePoint")
            {
                await dc.BeginDialogAsync(SharePointDialogID, null, cancellationToken);
            }
            else if (intent?.Key?.ToString() == "Word")
            {
                await dc.BeginDialogAsync(SharePointDialogID, null, cancellationToken);
            }
            else if (intent?.Key?.ToString() == "Outlook")
            {

                if (!context.Responded)
                {
                    await dc.BeginDialogAsync(OutlookDialogID, null, cancellationToken);
                }
                else
                {
                    await dc.ContinueDialogAsync(cancellationToken);
                }

            }
            return dc;
        }
        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken, bool saludar)
        {


            //IList<Result> asd = await google.GoogleIt(stepContext.Result.ToString());

            //foreach (var result in asd)
            //{
            //    var xd = result.Title;
            //}

            if (turnContext.Activity.MembersAdded != null)
            {
                foreach (var member in turnContext.Activity.MembersAdded)
                {
                    if (member.Id != turnContext.Activity.Recipient.Id)
                    {

                        var reply = ReplyWelcomeMessage(turnContext, member.Id, saludar);
                        await turnContext.SendActivityAsync(reply, cancellationToken);

                    }
                }
            }else if (!saludar)
            {
                var reply = ReplyWelcomeMessage(turnContext, "", saludar);
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }


        }

        private static Activity ReplyWelcomeMessage(ITurnContext turnContext, string memberId, bool saludar)
        {
            Activity reply = turnContext.Activity.CreateReply();
            reply.Text = WelcomeText;
            reply.Attachments = new List<Attachment> { CreateHeroCard(memberId, saludar).ToAttachment() };
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {



                    new CardAction(){ Title = "Consulta Office 365", Type=ActionTypes.ImBack, Value="Consulta Office 365" },

                    /*new CardAction(){ Title = "VER ÚLTIMOS CORREOS", Type=ActionTypes.ImBack, Value="Ver últimos correos" },
                            
                    new CardAction(){ Title = "CONSULTAR ESTADO DE SERVICIO O365", Type=ActionTypes.ImBack, Value="Blue" },
                    new CardAction(){ Title = "CHAT LIBRE CON BOT", Type=ActionTypes.ImBack, Value="Red" },
                    new CardAction(){ Title = "PREGUNTA ACERCA DE SERVICIO", Type=ActionTypes.ImBack, Value="Green" },
                    new CardAction(){ Title = "AYUDA", Type=ActionTypes.ImBack, Value="Ayuda" },*/
                }
            };

            return reply;
        }

        private async Task<DialogTurnResult> ConsultarTecnologiaAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            if (this.nuevaConversacion)
            {
                await step.Context.SendActivityAsync(WelcomeText);
            }

            PromptOptions options = new PromptOptions()
            {
                Prompt = step.Context.Activity.CreateReply("En qué tecnología tienes duda?"),
                Choices = new List<Choice>()

            };
            options.Choices.Add(new Choice() { Value = "Office 365" });
            return await step.PromptAsync("name", options, cancellationToken);


            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.

        }

        private static PromptOptions GenerateOptions(Activity activity)
        {
            // Create options for the prompt
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply("Question?"),
                Choices = new List<Choice>(),
            };
            options.Choices.Add(new Choice() { Value = "Example answer" });

            return options;

        }

        private async Task<DialogTurnResult> DerivarTecnologiaAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            FoundChoice respuesta = step.Result as FoundChoice;
            string resultado = (string)respuesta.Value;
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(step.Context, () => new Usuario(), cancellationToken);

            // Update the profile.
            userProfile.Nombre = resultado;
            userProfile.ErrorName = "";


            var dialogSiguiente = "";
            switch (resultado)
            {
                case ("Office 365"):
                    dialogSiguiente = Office365DialogID;
                    //await step.BeginDialogAsync(Office365DialogID, cancellationToken: cancellationToken);
                    //await step.ReplaceDialogAsync(Office365DialogID, cancellationToken: cancellationToken);

                    break;
                default:
                    break;
            }









            // We can send messages to the user at any point in the WaterfallStep.
            //await step.Context.SendActivityAsync(MessageFactory.Text($"Thanks {resultado}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            //return await step.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Would you like to give your age?") }, cancellationToken);

            return await step.ReplaceDialogAsync(dialogSiguiente, cancellationToken: cancellationToken);

            //return await step.EndDialogAsync(cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new Usuario(), cancellationToken);










                // We can send messages to the user at any point in the WaterfallStep.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I have your name as {userProfile.Nombre}."), cancellationToken);

            }
            else
            {
                // We can send messages to the user at any point in the WaterfallStep.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be kept."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }



        /// <summary>
        /// Processes input and route to the appropriate step.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the operation result of the operation.</returns>
        private async Task<DialogContext> ProcessInputAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            switch (turnContext.Activity.Text.ToLowerInvariant())
            {
                case "signout":
                case "logout":
                case "signoff":
                case "logoff":
                case "cerrar sesión":
                    // The bot adapter encapsulates the authentication processes and sends
                    // activities to from the Bot Connector Service.
                    var botAdapter = (BotFrameworkAdapter)turnContext.Adapter;
                    await botAdapter.SignOutUserAsync(turnContext, ConnectionSettingName, cancellationToken: cancellationToken);

                    // Let the user know they are signed out.
                    await turnContext.SendActivityAsync("You are now signed out.", cancellationToken: cancellationToken);
                    break;
                case "help":
                    await turnContext.SendActivityAsync(WelcomeText, cancellationToken: cancellationToken);
                    break;
                default:
                    // The user has input a command that has not been handled yet,
                    // begin the waterfall dialog to handle the input.
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        //await dc.BeginDialogAsync("graphDialog", cancellationToken: cancellationToken);
                    }

                    break;
            }

            return dc;
        }
        /// <summary>
        /// Creates a <see cref="HeroCard"/> that is sent as a welcome message to the user.
        /// </summary>
        /// <param name="newUserName"> The name of the user.</param>
        /// <returns>A <see cref="HeroCard"/> the user can interact with.</returns>
        private static HeroCard CreateHeroCard(string newUserName, bool saludar)
        {
            var titulo = saludar ? $"Bienvenido {newUserName}" : "";
            var heroCard = new HeroCard(titulo)
            {
                Text = saludar ? $"Bienvenido {newUserName}" : "",
                Subtitle = "🐶",

                Images = new List<CardImage>
                {
                    new CardImage(
                        "https://botframeworksamples.blob.core.windows.net/samples/aadlogo.png",
                        "AAD Logo",
                        new CardAction(
                            ActionTypes.OpenUrl,
                            value: "https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview")),
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, "Mi información", text: "Yo", displayText: "Yo", value: "Yo"),
                    //new CardAction(ActionTypes.ImBack, "Recent", text: "Recent", displayText: "Recent", value: "Recent"),
                    //new CardAction(ActionTypes.ImBack, "View Token", text: "View Token", displayText: "View Token", value: "View Token"),
                    //new CardAction(ActionTypes.ImBack, "obtenernoticias", text: "Obtener Noticias", displayText: "Obtener Noticias", value: "obtenernoticias"),
                    //new CardAction(ActionTypes.ImBack, "ver ultimos correos", text: "ver ultimos correos", displayText: "ver ultimos correos", value: "ver ultimos correos"),
                    //new CardAction(ActionTypes.ImBack, "Help", text: "Help", displayText: "Help", value: "Help"),
                    new CardAction(ActionTypes.ImBack, "Active Directory", text: "Active Directory", displayText: "Active Directory", value: "Active Directory"),
                    new CardAction(ActionTypes.ImBack, "Office 365", text: "Office 365", displayText: "Office 365", value: "Office 365"),
                    new CardAction(ActionTypes.ImBack, "Outlook", text: "Outlook", displayText: "Outlook", value: "Outlook"),
                    new CardAction(ActionTypes.ImBack, "SharePoint", text: "SharePoint", displayText: "SharePoint", value: "Duda con SharePoint"),
                    new CardAction(ActionTypes.ImBack, "Word", text: "Word", displayText: "Word", value: "Word"),
                    new CardAction(ActionTypes.ImBack, "Cerrar Sesión", text: "Signout", displayText: "Signout", value: "Cerrar Sesión"),
                    new CardAction(ActionTypes.ImBack, "Ayuda", text: "Ayuda", displayText: "Ayuda", value: "Ayuda"),
                },
            };
            return heroCard;
        }


        private static HeroCard CreateHeroOutlook(string newUserName)
        {
            var heroCard = new HeroCard($"Welcome {newUserName}", "OAuthBot")
            {
                Images = new List<CardImage>
                {
                    new CardImage(
                        "https://botframeworksamples.blob.core.windows.net/samples/aadlogo.png",
                        "AAD Logo",
                        new CardAction(
                            ActionTypes.OpenUrl,
                            value: "https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview")),
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, "Me", text: "Me", displayText: "Me", value: "Me"),
                    new CardAction(ActionTypes.ImBack, "Recent", text: "Recent", displayText: "Recent", value: "Recent"),
                    new CardAction(ActionTypes.ImBack, "View Token", text: "View Token", displayText: "View Token", value: "View Token"),
                    new CardAction(ActionTypes.ImBack, "obtenernoticias", text: "Obtener Noticias", displayText: "Obtener Noticias", value: "obtenernoticias"),
                    new CardAction(ActionTypes.ImBack, "ver ultimos correos", text: "ver ultimos correos", displayText: "ver ultimos correos", value: "ver ultimos correos"),
                    new CardAction(ActionTypes.ImBack, "Help", text: "Help", displayText: "Help", value: "Help"),
                    new CardAction(ActionTypes.ImBack, "Signout", text: "Signout", displayText: "Signout", value: "Signout"),
                },
            };
            return heroCard;
        }


        /// <summary>
        /// Waterfall step that will prompt the user to log in if they are not already.
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the operation result of the operation.</returns>
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var activity = step.Context.Activity;

            // Set the context if the message is not the magic code.
            if (activity.Type == ActivityTypes.Message &&
                !Regex.IsMatch(activity.Text, @"(\d{6})"))
            {
                await _accessors.CommandState.SetAsync(step.Context, activity.Text, cancellationToken);
                await _accessors.UserState.SaveChangesAsync(step.Context, cancellationToken: cancellationToken);
            }

            return await step.BeginDialogAsync("loginPrompt", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Waterfall dialog step to process the command sent by the user.
        /// </summary>
        /// <param name="step">A <see cref="WaterfallStepContext"/> provides context for the current waterfall step.</param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the operation result of the operation.</returns>
        private async Task<DialogTurnResult> ProcessStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            if (step.Result != null)
            {
                // We do not need to store the token in the bot. When we need the token we can
                // send another prompt. If the token is valid the user will not need to log back in.
                // The token will be available in the Result property of the task.
                var tokenResponse = step.Result as TokenResponse;

                // If we have the token use the user is authenticated so we may use it to make API calls.
                if (tokenResponse?.Token != null)
                {
                    var parts = _accessors.CommandState.GetAsync(step.Context, () => string.Empty, cancellationToken: cancellationToken).Result.Split(' ');
                    string command = parts[0].ToLowerInvariant();

                    if (command.StartsWith("send"))
                    {
                        await OAuthHelpers.SendMailAsync(step.Context, tokenResponse, parts[1]);
                    }
                    else if (command.StartsWith("recent"))
                    {
                        await OAuthHelpers.ListRecentMailAsync(step.Context, tokenResponse);
                    }
                    else if (command.StartsWith("obtenernoticias"))
                    {
                        await OAuthHelpers.ListItemAsync(step.Context, tokenResponse);
                    }
                    else
                    {
                        await step.Context.SendActivityAsync($"Your token is: {tokenResponse.Token}", cancellationToken: cancellationToken);
                    }

                    await _accessors.CommandState.DeleteAsync(step.Context, cancellationToken);
                }
            }
            else
            {
                await step.Context.SendActivityAsync("We couldn't log you in. Please try again later.", cancellationToken: cancellationToken);
            }

            return await step.EndDialogAsync(cancellationToken: cancellationToken);
        }


        public void LoginPorfa(string domainPorfa)
        {
            string clientId = "";
            string clientsecret = "";
            string tenant = domainPorfa + "yourdomain.onmicrosoft.com";

            var authUri = "https://login.microsoftonline.com/" + tenant + "/oauth2/token";
            var RESOURCE_URL = "https://outlook.office.com";

            HttpClient client = new HttpClient();
            var authContext = new AuthenticationContext(authUri);
            var credential = new ClientCredential(clientId: clientId, clientSecret: clientsecret);
            var result = authContext.AcquireTokenAsync(RESOURCE_URL, credential).Result;
            client.DefaultRequestHeaders.Add("Authorization", "bearer " + result.AccessToken);


            var response = client.GetAsync("https://outlook.office.com/api/v2.0/users/user1@" + domainPorfa + ".onmicrosoft/messages").Result;

            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }
}
