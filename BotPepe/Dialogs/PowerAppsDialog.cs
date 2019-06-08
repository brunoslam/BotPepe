using BotPepe.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotPepe.Dialogs
{
    public class PowerAppsDialog : ComponentDialog
    {
        public static BotAccessors accessors;
        private static string PowerAppsDialogID = "PowerAppsDialogID";
        public PowerAppsDialog(string id, BotAccessors _accessors) : base(id)
        {

            //InitialDialogId = Id;
            InitialDialogId = id;
            accessors = _accessors;
            var waterfallSteps = new WaterfallStep[]
            {
                    PreguntaBasicaStepAsync,
                    ConfirmacionStepAsync,
                    FinalStepAsync
            };
            AddDialog(new WaterfallDialog(PowerAppsDialogID, waterfallSteps));
            // Define the conversation flow using a waterfall model.
            AddDialog(new ChoicePrompt("PreguntaBasica"));
            ConfirmPrompt asd = new ConfirmPrompt("confirm");
            asd.ConfirmChoices = new Tuple<Choice, Choice>( new Choice("Sí 😄"), new Choice("No 😞"));
            AddDialog(asd);
            //AddDialog(new ChoicePrompt("PreguntaBasica"));
        }

        private static async Task<DialogTurnResult> PreguntaBasicaStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Usuario userProfile =
                    await accessors.UserProfile.GetAsync(stepContext.Context, () => new Usuario());
            userProfile.EsperaRespuestaQNA = true;

            PromptOptions options = new PromptOptions()
            {
                Prompt = stepContext.Context.Activity.CreateReply("Qué dudas tienes sobre PowerApps?"),
                Choices = new List<Choice>(),
                Style = ListStyle.HeroCard
            };
            options.Choices.Add(new Choice() { Value = "Qué es PowerApps? 🤔" });
            options.Choices.Add(new Choice() { Value = "Para qué sirve PowerApps? 😵" });
            options.Choices.Add(new Choice() { Value = "Cómo funciona PowerApps? 😮"});
            
            
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("PreguntaBasica", options, cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmacionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            Usuario userProfile =
                     await accessors.UserProfile.GetAsync(stepContext.Context, () => new Usuario());
            userProfile.EsperaRespuestaQNA = false;
            
            return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Esta información sirvió de ayuda?") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            if ((bool)stepContext.Result)
            {
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                // We can send messages to the user at any point in the WaterfallStep.
                return await stepContext.BeginDialogAsync(PowerAppsDialogID, cancellationToken);
            }
        }
    }

}
