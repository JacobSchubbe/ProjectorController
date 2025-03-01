using Microsoft.AspNetCore.Mvc;

namespace ProjectController.Endpoints
{
    [Route("api/logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> logger;
        
        public LogsController(ILogger<LogsController> logger)
        {
            this.logger = logger;
        }
        
        [HttpPost]
        public IActionResult Log([FromBody] LogEntry logEntry)
        {
            if (string.IsNullOrWhiteSpace(logEntry.Message))
            {
                return BadRequest("Invalid log entry.");
            }

            switch (logEntry.Level.ToLower())
            {
                case "info":
                    logger.LogInformation($"GUI: {logEntry.Message}");
                    break;
                case "debug":
                    logger.LogDebug($"GUI: {logEntry.Message}");
                    break;
                case "warning":
                    logger.LogWarning($"GUI: {logEntry.Message}");
                    break;
                case "error":
                    logger.LogError($"GUI: {logEntry.Message}");
                    break;
            }

            return Ok();
        }
    }

    // LogEntry class to represent the log payload
    public class LogEntry
    {
        public string Level { get; set; } // e.g., "info", "error"
        public string Message { get; set; } // The log message
    }
}