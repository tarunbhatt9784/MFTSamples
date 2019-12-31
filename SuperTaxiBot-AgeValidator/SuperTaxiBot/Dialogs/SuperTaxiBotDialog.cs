using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StoreAzureRGRequests.Dialogs
{
    public class SuperTaxiBotDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<SuperTaxiBotDialog> _userProfileAccessor;
        public SuperTaxiBotDialog(UserState userState)
            : base(nameof(SuperTaxiBotDialog))
        {
            _userProfileAccessor = userState.CreateProperty<SuperTaxiBotDialog>("SuperTaxiBotDialog");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                WelcomeStepAsync,
                ValidAgeStepAsync,
                SetPickupLocationStepAsync,
                SetDropOffLocationStepAsync,
                SetPickupTimeStepAsync,
                SetPreferredCarTypeStepAsync,
                SummaryStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), NumberValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ChoicePrompt("CarTypeDialog")); 

 // The initial child Dialog to run.
 InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var turnContext = stepContext.Context;
            var question= $"Welcome to the Super Taxi Bot. My name is SuperTaxiBot (much like Superman) and I would be processing your booking request for the day";
            question = $"{question} \n\n Lets start with your name";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }
        private static async Task<DialogTurnResult> ValidAgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your age."),
                RetryPrompt = MessageFactory.Text("Please Enter a valid Age. Age should be a valid number greater than 0"),
            };

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }


        private static async Task<DialogTurnResult> SetPickupLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;
            var name = stepContext.Values["name"];
            var question = $"Thanks for entering a valid age {name}. Kindly provide your pickup location?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SetDropOffLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["pickupLocation"] = ((String)stepContext.Result);
            var question = $"I have saved your pickup location as {((String)stepContext.Result)}.\n\n Kindly provide your drop off location?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SetPickupTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["dropOffLocation"] = ((String)stepContext.Result);
            var question = $"I have saved your drop off location as {((String)stepContext.Result)}.\n\n Kindly provide a suitable pickup time?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SetPreferredCarTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["pickUpTime"] = ((String)stepContext.Result);
            var question = $"Kindly provide car type you would prefer";
            List<string> carTypes = new List<string>();
            carTypes.Add("Sedan");
            carTypes.Add("Hatch");

            return await stepContext.PromptAsync("CarTypeDialog", new PromptOptions()
            {

                Prompt = MessageFactory.Text(question),
                Choices = ChoiceFactory.ToChoices(carTypes),

            }, cancellationToken);
        }


        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            //if ((bool)stepContext.Result)
            //{
            // Get the current profile object from user state.
            var name = stepContext.Values["name"];
            var pickUpLocation = stepContext.Values["pickupLocation"];
            var dropOffLocation = stepContext.Values["dropOffLocation"];
            var pickUpTime = stepContext.Values["pickUpTime"];
            var carType = ((FoundChoice)stepContext.Result).Value;
            
            var reply = $"Thanks for booking a cab with us {name}.";
            reply = $"{reply} \n\nYou will picked up from {pickUpLocation} at {pickUpTime} and will be dropped at {dropOffLocation}";
            await stepContext.Context.SendActivityAsync($"{reply}");

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> NumberValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            int i;
            bool isInteger = Int32.TryParse(promptContext.Recognized.Value.ToString(), out i);
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && isInteger && promptContext.Recognized.Value > 0);
        }


    }
}
