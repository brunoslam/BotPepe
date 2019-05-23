using BotPepe.Models;
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
    public class Office365Dialog : ComponentDialog
    {
        private BotAccessors accessors;
        private const string Office365DialogID = "office365Dialog";
        private const string PowerAppsDialogID = "PowerAppsDialogID";
        private const string TeamsDialogID = "office365Dialog";
        public Office365Dialog(string id, BotAccessors _accessors) : base(id)
        {

            //InitialDialogId = Id;
            InitialDialogId = id;
            accessors = _accessors;
            var waterfallSteps = new WaterfallStep[]
            {
                    ConsultarTecnologiaAsync,
                    DerivarTecnologiaAsync
            };
            AddDialog(new WaterfallDialog(Office365DialogID, waterfallSteps));
            AddDialog(new PowerAppsDialog(PowerAppsDialogID, _accessors));
            // Define the conversation flow using a waterfall model.
            AddDialog(new ChoicePrompt("servicioOffice365"));
        }


        private async Task<DialogTurnResult> ConsultarTecnologiaAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            PromptOptions options = new PromptOptions()
            {
                Prompt = step.Context.Activity.CreateReply("En qué servicio tienes duda?"),
                Choices = new List<Choice>()

            };
            options.Choices.Add(new Choice() { Value = "PowerApps" });
            options.Choices.Add(new Choice() { Value = "Teams" });
            options.Choices.Add(new Choice() { Value = "OneDrive" });
            options.Choices.Add(new Choice() { Value = "Flow" });
            return await step.PromptAsync("servicioOffice365", options, cancellationToken);


            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.

        }


        private async Task<DialogTurnResult> DerivarTecnologiaAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            FoundChoice respuesta = step.Result as FoundChoice;
            string resultado = (string)respuesta.Value;
            // Get the current profile object from user state.
            var userProfile = await accessors.UserProfile.GetAsync(step.Context, () => new Usuario(), cancellationToken);

            var dialogSiguiente = "";
            switch (resultado)
            {
                case ("PowerApps"):
                    dialogSiguiente = PowerAppsDialogID;
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

            return await step.BeginDialogAsync(dialogSiguiente, cancellationToken: cancellationToken);

            //return await step.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
