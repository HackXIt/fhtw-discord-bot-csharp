using System;
using System.Threading.Tasks;
using FHTW.DiscordBot.Services;
using FHTW.Scraper.Scrapers.Userprofile;
using FHTW.Scraper.Services;
using FHTW.Shared;
using FHTW.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FHTW.WebApp.Controllers;

[Route("api/fhtw/[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IScraperService _scraperService;
    private readonly ILogger<RegistrationController> _logger;
    private readonly EventService _eventService;

    public RegistrationController(IUserService userService, IScraperService scraperService, ILogger<RegistrationController> logger, EventService eventService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scraperService = scraperService ?? throw new ArgumentNullException(nameof(scraperService));
        _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
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
        try
        {
            var scrape = await _scraperService.Scrape(new UserprofileScrapeArguments(user.MailUsername));
            if (scrape is not { Success: true, UserprofileScrapeResult: not null }) return false;
            if (string.Compare(user.MailUsername, scrape.UserprofileScrapeResult.Username, StringComparison.Ordinal) != 0)
            {
                _logger.LogInformation("Scraped username does not match user!");
                return false;
            }
            var activatedUser =
                await _userService.ActivateUserWithStudentInformation(user.UserId, scrape.UserprofileScrapeResult);
            if (activatedUser == null)
            {
                _logger.LogInformation("User activation failed!");
                return false;
            }

            var student = await _userService.GetStudentByDiscordIdAsync(activatedUser.UserId);
            if (student == null)
            {
                _logger.LogInformation("Newly activated student not found!");
                return false;
            }

            // Notify about the completed registration.
            _eventService.RaiseStudentRegistered(student, activatedUser.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during completion of registration: {}", ex.Message);
            return false;
        }
        
        
        return true;
    }
}