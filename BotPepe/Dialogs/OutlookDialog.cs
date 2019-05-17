using BotPepe.Controller;
using BotPepe.Models;
using Google.Apis.Customsearch.v1.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BotPepe.Dialogs
{
    public class OutlookDialog : ComponentDialog
    {
        private const string ConnectionSettingName = "OauthBot";
        private BotAccessors accessors;
        public OutlookDialog(string id, BotAccessors _accessors) : base(id)
        {

            //InitialDialogId = Id;
            InitialDialogId = id;
            accessors = _accessors;
            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                EnviarAccionesOutlook,
                ReemplazarDialog,
                PromptStepAsync,
                
                CorreosStepAsync
                //EnviarAccionesOutlook
            };
            WaterfallStep[] waterfallStepsOtro = new WaterfallStep[]
            {
                OtroOutlook,
                ConfirmacionOtroOutlook
                //CorreosStepAsync
                //EnviarAccionesOutlook
            };
            //AddDialog(OAuthHelpers.Prompt(ConnectionSettingName));
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
            AddDialog(new WaterfallDialog("waterfallStepsOtro", waterfallStepsOtro));
            AddDialog(Prompt(ConnectionSettingName));
            AddDialog(new ChoicePrompt("EnviarAccionesOutlook"));
            AddDialog(new TextPrompt("ReemplazarDialog"));
            AddDialog(new TextPrompt("OtroOutlook"));
            AddDialog(new ConfirmPrompt("ConfirmacionOtroOutlook"));

        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            var reply = turnContext.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { CreateHeroCard().ToAttachment() };
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "CONSULTAR ESTADO DE SERVICIO O365", Type=ActionTypes.ImBack, Value="Blue" },
                    new CardAction(){ Title = "CHAT LIBRE CON BOT", Type=ActionTypes.ImBack, Value="Red" },
                    new CardAction(){ Title = "PREGUNTA ACERCA DE SERVICIO", Type=ActionTypes.ImBack, Value="Green" }
                }
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task<DialogTurnResult> OtroOutlook(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var reply = stepContext.Context.Activity.CreateReply();
            //reply.Attachments = new List<Attachment> { CreateHeroCard().ToAttachment() };
            //await stepContext.Context.SendActivityAsync(reply, cancellationToken);


            //stepContext.Values["transport"] = ((FoundChoice)dc.).Value;

            return await stepContext.PromptAsync("OtroOutlook", new PromptOptions { Prompt = MessageFactory.Text("Qué desea saber? 😊") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> ReemplazarDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var reply = stepContext.Context.Activity.CreateReply();
            //reply.Attachments = new List<Attachment> { CreateHeroCard().ToAttachment() };
            //await stepContext.Context.SendActivityAsync(reply, cancellationToken);


            //stepContext.Values["transport"] = ((FoundChoice)dc.).Value;
            if (((FoundChoice)stepContext.Result).Value == "Otro")
            {
                return await stepContext.ReplaceDialogAsync("waterfallStepsOtro", null, cancellationToken);
            }

            return await stepContext.ContinueDialogAsync(cancellationToken);
        }
        private static async Task<DialogTurnResult> ConfirmacionOtroOutlook(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //GOOGLE HERE
            string respuestaGoogle = "";
            CallGoogle google = new CallGoogle();
            Answer[] answers = await google.SerpWow(stepContext.Result.ToString().Replace(" ", "+"));


            if (answers != null)
            {
                foreach (Answer answer in answers)
                {
                    if (answer.steps != null)
                    {
                        int contador = 0;
                        foreach (string steps in answer.steps)
                        {
                            contador++;
                            respuestaGoogle += $"{contador}. {steps}\n";

                        }
                        break;
                    }
                    respuestaGoogle += answer.answer;
                }
            }
            else
            {
                respuestaGoogle = "No se encontró respuesta en Google Answers. Crear método para mostrar preguntas relacionadas. 😞";
            }


            //var reply = stepContext.Context.Activity.CreateReply();
            //reply.Attachments = new List<Attachment> { CreateHeroCard().ToAttachment() };
            //await stepContext.Context.SendActivityAsync(reply, cancellationToken);


            //stepContext.Values["transport"] = ((FoundChoice)dc.).Value;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(respuestaGoogle), cancellationToken);


            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> EnviarAccionesOutlook(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var reply = stepContext.Context.Activity.CreateReply();
            //reply.Attachments = new List<Attachment> { CreateHeroCard().ToAttachment() };
            //await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            var options = new PromptOptions()
            {
                Prompt = stepContext.Context.Activity.CreateReply("Qué tiene duda sobre Outlook?"),
                Choices = new List<Choice>(),
            };

            // Add the choices for the prompt.
            //ERROR: se cae cuando se ocupa "(", "[". options.Choices.Add(new Choice() { Value = "Ver últimos correos (ejem)" });
            options.Choices.Add(new Choice() { Value = "Ver últimos correos" });
            options.Choices.Add(new Choice() { Value = "Otro" }); ;


            //return await stepContext.
            return await stepContext.PromptAsync("EnviarAccionesOutlook", options, cancellationToken);
            //foreach (var member in turnContext.Activity.MembersAdded)
            //{
            //    if (member.Id != turnContext.Activity.Recipient.Id)
            //    {
            //        var reply = turnContext.Activity.CreateReply();
            //        //reply.Text = WelcomeText;
            //        reply.Attachments = new List<Attachment> { CreateHeroCard(member.Id).ToAttachment() };
            //        reply.SuggestedActions = new SuggestedActions()
            //        {
            //            Actions = new List<CardAction>()
            //            {



            //                new CardAction(){ Title = "CONSULTAR ESTADO DE SERVICIO O365", Type=ActionTypes.ImBack, Value="Blue" },
            //                new CardAction(){ Title = "CHAT LIBRE CON BOT", Type=ActionTypes.ImBack, Value="Red" },
            //                new CardAction(){ Title = "PREGUNTA ACERCA DE SERVICIO", Type=ActionTypes.ImBack, Value="Green" }
            //            }
            //        };
            //        await turnContext.SendActivityAsync(reply, cancellationToken);

            //    }
            //}
        }

        private static HeroCard CreateHeroCard()
        {
            var heroCard = new HeroCard($"Acciones Outlook", "OAuthBot")
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

                    new CardAction(ActionTypes.ImBack, "Ver últimos correos", text: "Ver últimos correos", displayText: "Ver últimos correos", value: "Ver últimos correos"),
                    new CardAction(ActionTypes.ImBack, "Otro", text: "Otro", displayText: "Otro", value: "Otro"),
                },
            };
            return heroCard;
        }
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var activity = step.Context.Activity;
            if (step.Result.ToString() == "Ver ultimos correos")
            {
                await OAuthHelpers.ListRecentMailAsync(step.Context, step.Result as TokenResponse);
            }
            else if (step.Result.ToString() == "Otro")
            {
                var reply = step.Context.Activity.CreateReply();
                reply.Attachments = new List<Attachment> { CreateHeroCard().ToAttachment() };
                reply.Text = "Ingresa la consulta";
                return await step.BeginDialogAsync("waterfallStepsOtro", cancellationToken: cancellationToken);

                //await step.Context.SendActivityAsync(reply, cancellationToken);
            }
            // Set the context if the message is not the magic code.
            if (activity.Type == ActivityTypes.Message && !Regex.IsMatch(activity.Text, @"(\d{6})"))
            {
                await accessors.CommandState.SetAsync(step.Context, activity.Text, cancellationToken);
                await accessors.UserState.SaveChangesAsync(step.Context, cancellationToken: cancellationToken);
            }

            return await step.BeginDialogAsync("loginPrompt", cancellationToken: cancellationToken);
        }

        public static OAuthPrompt Prompt(string connectionName)
        {
            return new OAuthPrompt(
                "loginPrompt",
                new OAuthPromptSettings
                {
                    ConnectionName = connectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login

                });

        }
        private async Task<DialogTurnResult> CorreosStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.

            // return await stepContext.PromptAsync("name", new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);



            try
            {

                var tokenResponse = (TokenResponse)stepContext.Result;
                if (tokenResponse != null)
                {
                    //throw new InvalidOperationException("xd1");
                    await OAuthHelpers.ListRecentMailAsync(stepContext.Context, stepContext.Result as TokenResponse);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);

                }
                else
                {
                    //List.
                    //throw new InvalidOperationException("xd");
                    return await stepContext.ReplaceDialogAsync(this.Id, null, cancellationToken);
                }
            }
            catch (Exception error)
            {
                //accessors.UserProfile.Name
                return await stepContext.ReplaceDialogAsync(this.Id, null, cancellationToken);
                throw new InvalidOperationException(error.Message + stepContext.Result);
                //return await stepContext.ReplaceDialogAsync(this.Id, null, cancellationToken);
            }
        }





    }
}
