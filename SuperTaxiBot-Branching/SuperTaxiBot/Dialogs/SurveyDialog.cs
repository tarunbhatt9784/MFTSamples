using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace SuperTaxiBot.Dialogs
{
    public class SurveyDialog : ComponentDialog
    {
        public SurveyDialog()
            : base(nameof(SurveyDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt("CarTypeDialog"));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ScoreStepAsync,
                ConcludeSurveyStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> ScoreStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var question = "Thanks for taking the time out to fill out this survey.";
            question = "\\n\\n On a scale from 0-10, How likely are you to recommend our services to your friends and family";
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = MessageFactory.Text(question) }, cancellationToken);            
        }


        private async Task<DialogTurnResult> ConcludeSurveyStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            CarBooking booking = SuperTaxiBotDialog.GetCarBookingObj(stepContext);
            booking.NpsScore = (int)stepContext.Result;
            var msg = "";
            if (booking.NpsScore >= 0 && booking.NpsScore < 10)
            {
                msg = $"Thanks for participating in the survey. We have recorded your feedback and out team will work on them to improve our services";
                await stepContext.Context.SendActivityAsync($"{msg}");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                msg = "Invalid Value. NPS value has to be between 0-10.";
                await stepContext.Context.SendActivityAsync($"{msg}");
                return await stepContext.ReplaceDialogAsync(nameof(SurveyDialog), null, cancellationToken);
            }
        }
    }
}
