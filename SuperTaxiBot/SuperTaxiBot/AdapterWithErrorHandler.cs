// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SuperTaxiBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                var message = $"Exception caught : {exception.Message} \n\n {exception.InnerException}\n\n {exception.StackTrace}";
                // Log any leaked exception from the application.
                logger.LogError(message);

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(message);
            };
        }
    }
}
