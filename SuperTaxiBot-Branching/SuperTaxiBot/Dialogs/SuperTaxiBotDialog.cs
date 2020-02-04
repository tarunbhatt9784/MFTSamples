using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace SuperTaxiBot.Dialogs
{
    public class SuperTaxiBotDialog : ComponentDialog
    {
        public SuperTaxiBotDialog(UserState userState)
            : base(nameof(SuperTaxiBotDialog))
        {
            AddDialog(new SurveyDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WelcomeStepAsync,
                SetPickupLocationStepAsync,
                SetDropOffLocationStepAsync,
                SetPickupTimeStepAsync,
                DisplaySummaryStepAsync,
                BranchOutStepAsync,
            }));
            AddDialog(new ChoicePrompt("SurveyChoice"));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var turnContext = stepContext.Context;
            var question = $"Welcome to the Super Taxi Bot. My name is SuperTaxiBot (much like Superman) and I would be processing your booking request for the day";
            question = $"{question} \n\n Lets start with your name";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }
        private static async Task<DialogTurnResult> SetPickupLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CarBooking booking = GetCarBookingObj(stepContext);
            booking.Name = ((String)stepContext.Result);
            stepContext.Values["CarBookingObj"] = booking;
            var question = $"Hey **{booking.Name}**, Kindly provide your pickup location?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SetDropOffLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CarBooking booking = GetCarBookingObj(stepContext);
            booking.PickupLocation = ((String)stepContext.Result);
            stepContext.Values["CarBookingObj"] = booking;
            var question = $"I have saved your pickup location as **{booking.PickupLocation}**.\n\n Kindly provide your drop off location?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SetPickupTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CarBooking booking = GetCarBookingObj(stepContext);
            booking.DropOffLocation = ((String)stepContext.Result);
            stepContext.Values["CarBookingObj"] = booking;
            var question = $"I have saved your drop off location as **{booking.DropOffLocation}**.\n\n Kindly provide a suitable pickup time?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);
        }
        
        private async Task<DialogTurnResult> DisplaySummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CarBooking booking = GetCarBookingObj(stepContext);
            booking.PickupTime = ((String)stepContext.Result);
            string summary = $"Hey **{booking.Name}**, \n\nThanks for booking a car with SuperTaxi.\n\n You will be picked up from **{booking.PickupLocation}** at **{booking.PickupTime}** and will be dropped at **{booking.DropOffLocation}**.\n\n Thanks again for your business.\n\nRegards Super Taxi";
            await stepContext.Context.SendActivityAsync($"{summary}");
            IList<string> interestedInSurvey = new List<string>();
            interestedInSurvey.Add("Yes");
            interestedInSurvey.Add("No");
            string surveyMessage = $"Would you like to spend a minute to provide feedback on our services";
return            await stepContext.PromptAsync("SurveyChoice", new PromptOptions()
            {

                Prompt = MessageFactory.Text(surveyMessage),
                Choices = ChoiceFactory.ToChoices(interestedInSurvey),

            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> BranchOutStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CarBooking booking = GetCarBookingObj(stepContext);
            booking.InterestedInSurvey = ((FoundChoice)stepContext.Result).Value;
            if (booking.InterestedInSurvey == "Yes")
                return await stepContext.BeginDialogAsync(nameof(SurveyDialog), null, cancellationToken);
            else
                return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        public static CarBooking GetCarBookingObj(WaterfallStepContext stepContext)
        {
            return (stepContext == null || stepContext.Values.Count is 0 || !(stepContext.Values.ContainsKey("CarBookingObj"))) ? new CarBooking() : (CarBooking)stepContext.Values["CarBookingObj"];
        }
    }
}
