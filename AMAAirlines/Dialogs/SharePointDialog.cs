using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AMAAirlines.Dialogs
{
    public class SharePointDialog : ComponentDialog
    {

        public SharePointDialog(string id)
        : base(id)
        {
            InitialDialogId = Id;

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync
            };
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
            AddDialog(new TextPrompt("name"));
            //AddDialog
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("name", new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }
        
        public void EnviarMenuSharepoint()
        {
            //Enviar todos los acciones posibles

        }

        private async Task<DialogTurnResult> FlightStatusAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            RecognizerResult result = stepContext.Options as RecognizerResult;

            // We can send messages to the user at any point in the WaterfallStep.
            var message = result.Intents.FirstOrDefault().ToString();
            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            return await stepContext.ReplaceDialogAsync("statusDialog", result, cancellationToken);
        }
    }
}
