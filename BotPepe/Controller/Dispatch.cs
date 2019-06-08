using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotPepe.Controller
{
    public class Dispatch
    {
        private const string ConnectionSettingName = "OauthBot";
        private BotServices botServices;

        public Dispatch(BotServices botServices)
        {
            this.botServices = botServices;
        }

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
    }
}
