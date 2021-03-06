using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Wordle_Tracker_Telegram_Bot.Services;

namespace Wordle_Tracker_Telegram_Bot.Controllers
{
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                         [FromBody] Update update)
        {
            await handleUpdateService.EchoAsync(update);
            return Ok();
        }
    }
}