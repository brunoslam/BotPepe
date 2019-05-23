using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotPepe.Dialogs
{
    public class PowerAppsDialog : ComponentDialog
    {
        BotAccessors accessors;
        private string PowerAppsDialogID = "PowerAppsDialogID";
        public PowerAppsDialog(string id, BotAccessors _accessors) : base(id)
        {

            //InitialDialogId = Id;
            InitialDialogId = id;
            accessors = _accessors;
            var waterfallSteps = new WaterfallStep[]
            {
                    PreguntaBasicaStepAsync
            };
            AddDialog(new WaterfallDialog(PowerAppsDialogID, waterfallSteps));
            // Define the conversation flow using a waterfall model.
            AddDialog(new ChoicePrompt("PreguntaBasica"));
        }

        private static async Task<DialogTurnResult> PreguntaBasicaStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            PromptOptions options = new PromptOptions()
            {
                Prompt = stepContext.Context.Activity.CreateReply("Qué dudas tienes sobre PowerApps?"),
                Choices = new List<Choice>()

            };
            options.Choices.Add(new Choice() { Value = "Qué es?" });
            options.Choices.Add(new Choice() { Value = "Para qué?" });
            options.Choices.Add(new Choice() { Value = "Cómo funciona?"});

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("PreguntaBasica", options, cancellationToken);
        }

    }
}
