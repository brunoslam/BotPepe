using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotPepe.Dialogs
{
    public class OneDriveDialog : ComponentDialog
    {
        BotAccessors accessors;
        private string OneDriveDialogID = "OneDriveDialogID";
        public OneDriveDialog(string id, BotAccessors _accessors) : base(id)
        {

            //InitialDialogId = Id;
            InitialDialogId = id;
            accessors = _accessors;
            var waterfallSteps = new WaterfallStep[]
            {
                    
            };
            AddDialog(new WaterfallDialog(OneDriveDialogID, waterfallSteps));
            // Define the conversation flow using a waterfall model.
            AddDialog(new ChoicePrompt("servicioOffice365"));
        }
    
    }
}
