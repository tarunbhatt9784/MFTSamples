using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace SuperTaxiBot.Dialogs
{
    public class ChooseCarDialog : ComponentDialog
    {
        private static string[] _cars = new string[]
        {
            "Honda Civic", "Toyota Corolla", "Audi A1 1.0TFSI S-Tronic", "Skoda Fabia 81TSI DSG","Renault Zoe Life"
        };
        public ChooseCarDialog()
            : base(nameof(ChooseCarDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt("CarTypeDialog"));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChooseYourCarStepAsync,
                LoopCarAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> ChooseYourCarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            IList<string> carsSelected = stepContext.Options as IList<string> ?? new List<string>();
            stepContext.Values["carsSelected"] = carsSelected; 
            var message = String.Empty;
            if (carsSelected.Count is 0)
            {
                message = $"Please choose a car.";
            }
            else
            {
                message = $"You have selected **{carsSelected[0]}**. You can select an alternative Car";
                foreach(string selectedCar in carsSelected)
                {
                    _cars = _cars.Where(x => x != selectedCar).ToArray();
                }
            }
            return await stepContext.PromptAsync("CarTypeDialog", new PromptOptions()
            {

                Prompt = MessageFactory.Text(message),
                Choices = ChoiceFactory.ToChoices(_cars.ToList()),
            }, cancellationToken);
        }


        private async Task<DialogTurnResult> LoopCarAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            IList<string> carsSelected = (stepContext is null || !(stepContext.Values.ContainsKey("carsSelected"))) ? new List<string>() : (IList<string>)stepContext.Values["carsSelected"];
            carsSelected.Add(((FoundChoice)stepContext.Result).Value);
            stepContext.Values["carsSelected"] = carsSelected;
            if (carsSelected.Count >= 2)
            {
                return await stepContext.EndDialogAsync(carsSelected, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(ChooseCarDialog), carsSelected, cancellationToken);
            }
        }
    }
}
