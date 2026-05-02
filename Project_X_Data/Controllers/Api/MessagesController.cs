using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_X_Data.Data;
using Project_X_Data.Models.Api;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models.Rest;

namespace Project_X_Data.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly DataContext _context;

        public MessagesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory(Guid user1, Guid user2)
        {
            var dbMessages = await _context.Messages
                .Where(m => (m.SenderId == user1 && m.ReceiverId == user2) ||
                            (m.SenderId == user2 && m.ReceiverId == user1))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var apiMessages = dbMessages.Select(m => new Project_X_Data.Models.Api.Message
            {
                SenderId = m.SenderId.ToString(),
                ReceiverId = m.ReceiverId.ToString(),
                Text = m.Content,
                CreatedAt = m.SentAt
            });

            RestResponse response = new()
            {
                Status = RestStatus.Status200,
                Data = apiMessages
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Project_X_Data.Models.Api.Message newMessage)
        {
            if (string.IsNullOrWhiteSpace(newMessage.Text))
            {
                return BadRequest(new RestResponse { Status = RestStatus.Status400, Data = "Empty text" });
            }

            var dbEntity = new Project_X_Data.Data.Entities.Message
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.Parse(newMessage.SenderId),
                ReceiverId = Guid.Parse(newMessage.ReceiverId),
                Content = newMessage.Text,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(dbEntity);
            await _context.SaveChangesAsync();

            RestResponse response = new()
            {
                Status = RestStatus.Status200,
                Data = newMessage
            };
            return Ok(response);
        }
    }
}