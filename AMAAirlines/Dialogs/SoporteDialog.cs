using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AMAAirlines.Dialogs
{
    public class SoporteDialog : ComponentDialog
    {

        public SoporteDialog(string id)
        : base(id)
        {
            InitialDialogId = Id;

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameStepAsync2,
                NameStepAsync3,
                SoporteTicket
            };
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
            AddDialog(new TextPrompt("name"));
            AddDialog(new TextPrompt("name2"));
            AddDialog(new TextPrompt("name3"));
        }

        private Task<DialogTurnResult> SoporteTicket(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("name", new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> NameStepAsync2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("name2", new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> NameStepAsync3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("name3", new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }

    }
}
