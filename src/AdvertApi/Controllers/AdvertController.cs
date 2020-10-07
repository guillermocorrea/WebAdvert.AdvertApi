using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AdvertApi.Models;
using AdvertApi.Models.Messages;
using AdvertApi.Services;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdvertApi.Controllers
{
    [ApiController]
    [Route("api/v1/adverts")]
    public class AdvertController : ControllerBase
    {
        private readonly IAdvertStorageService _advertStorageService;
        private readonly ILogger<AdvertController> _logger;
        private readonly IConfiguration _configuration;

        public AdvertController(
          IAdvertStorageService advertStorageService,
          ILogger<AdvertController> logger,
          IConfiguration configuration)
        {
            _advertStorageService = advertStorageService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(CreateAdvertResponse), 201)]
        public async Task<IActionResult> Create(AdvertModel model)
        {
            string recordId;
            try
            {
                recordId = await _advertStorageService.Add(model);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return StatusCode(201,
              new CreateAdvertResponse { Id = recordId });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm(string id, [FromBody] ConfirmAdvertModel model)
        {
            var confirmModel = new ConfirmAdvertModel
            {
                Id = id,
                Status = model.Status
            };
            try
            {
                await _advertStorageService.Confirm(confirmModel);
                await RaiseConfirmedAdvertMessage(confirmModel);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return StatusCode(200);
        }

        private async Task RaiseConfirmedAdvertMessage(ConfirmAdvertModel model)
        {
          var topicArn = _configuration.GetValue<string>("TopicArn");
          var dbModel = await _advertStorageService.GetByIdAsync(model.Id);
            using (var client = new AmazonSimpleNotificationServiceClient())
            {
              var message = new AdvertConfirmedMessage {
                Id = model.Id,
                Title = dbModel.Title
              };
              var messageJSON = JsonSerializer.Serialize(message);
              await client.PublishAsync(topicArn, messageJSON);
            }
        }
    }
}