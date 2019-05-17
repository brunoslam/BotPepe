using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotPepe.Dialogs
{
    public class EncuestaDialog : ComponentDialog
    {
        private const string ConnectionSettingName = "OauthBot";
        private BotAccessors accessors;
        public EncuestaDialog(string id, BotAccessors _accessors) : base(id)
        {

            //InitialDialogId = Id;
            InitialDialogId = id;
            accessors = _accessors;
            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                
                //EnviarAccionesOutlook
            };
            WaterfallStep[] waterfallStepsOtro = new WaterfallStep[]
            {
                //CorreosStepAsync
                //EnviarAccionesOutlook
            };
            //AddDialog(OAuthHelpers.Prompt(ConnectionSettingName));
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
            AddDialog(new WaterfallDialog("waterfallStepsOtro", waterfallStepsOtro));
            AddDialog(new TextPrompt("name"));
            AddDialog(new TextPrompt("OtroOutlook"));
            AddDialog(new ConfirmPrompt("ConfirmacionOtroOutlook"));

        }
    }
}
