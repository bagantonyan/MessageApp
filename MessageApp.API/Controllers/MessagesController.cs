using AutoMapper;
using MessageApp.API.Hubs;
using MessageApp.API.Models.Messages;
using MessageApp.DAL.Entities;
using MessageApp.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace MessageApp.API.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<MessagesController> _logger;
        private readonly IMapper _mapper;

        public MessagesController(
            IMessageRepository messageRepository, 
            IHubContext<MessageHub> hubContext,
            ILogger<MessagesController> logger,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _hubContext = hubContext;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("sendmessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestModel requestModel)
        {
            var message = new Message
            {
                Text = requestModel.Text,
                CreatedAt = DateTime.UtcNow,
                SequenceNumber = await _messageRepository.GetNextSequenceNumberAsync()
            };

            try
            {
                _logger.LogInformation($"Trying to add message to the database {JsonConvert.SerializeObject(message)}");

                message.Id = await _messageRepository.AddMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding message to the database.");

                return StatusCode(500, "Internal server error");
            }

            var messageResponseModel = _mapper.Map<MessageResponseModel>(message);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", messageResponseModel);

            return Ok(messageResponseModel);
        }

        [HttpGet]
        [Route("getmessages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMessages([FromQuery] DateTime startTime)
        {
            List<Message> messages;

            try
            {
                _logger.LogInformation($"Trying to get messages from the database. StartTime: {startTime}");

                messages = await _messageRepository.GetMessagesAsync(startTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting messages from the database.");

                return StatusCode(500, "Internal server error");
            }

            var messageResponseModels = _mapper.Map<IEnumerable<MessageResponseModel>>(messages);

            return Ok(messageResponseModels);
        }
    }
}