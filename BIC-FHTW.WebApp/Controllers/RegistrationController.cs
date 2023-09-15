using System;
using BIC_FHTW.DiscordBot.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BIC_FHTW.Scraper.Scrapers.Userprofile;
using BIC_FHTW.Scraper.Services;
using BIC_FHTW.Shared;
using Microsoft.Extensions.Logging;

namespace BIC_FHTW.WebApp.Controllers;

[Route("api/bic-fhtw/[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IScraperService _scraperService;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(IUserService userService, IScraperService scraperService, ILogger<RegistrationController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scraperService = scraperService ?? throw new ArgumentNullException(nameof(scraperService));
    }

    [HttpGet("register")]
    [HttpPost("register")]
    public IActionResult Register()
    {
        return Ok("Endpoint reached!");
    }

    [HttpGet("complete-registration")]
    [HttpPost("complete-registration")]
    public async Task<IActionResult> CompleteRegistration(string token)
    {
        var user = await _userService.GetUserByActivationTokenAsync(token);
        if (user != null)
        {
            _logger.LogInformation("User found with activation token {}", token);
            if(await CompleteRegistration(user))
                return Ok("User setup successful!");
            return BadRequest("User setup failed!");
        }
        _logger.LogInformation("No user found with activation token {}", token);
        return BadRequest("Provided token is invalid!");
    }
    
    private async Task<bool> CompleteRegistration(DiscordUserDTO user)
    {
        var scrape = await _scraperService.Scrape(new UserprofileScrapeArguments(user.MailUsername));
        if (scrape is not { Success: true, UserprofileScrapeResult: not null }) return false;
        if (string.Compare(user.MailUsername, scrape.UserprofileScrapeResult.Username, StringComparison.Ordinal) != 0)
        {
            _logger.LogInformation("Scraped username does not match user!");
            return false;
        }

        await _userService.ActivateUserWithStudentInformation(user.UserId, scrape.UserprofileScrapeResult);
        return true;
    }
}