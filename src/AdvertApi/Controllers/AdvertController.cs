using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvertApi.Models;
using AdvertApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvertApi.Controllers
{
  [ApiController]
  [Route("api/v1/adverts")]
  public class AdvertController : ControllerBase
  {
    private readonly IAdvertStorageService _advertStorageService;
    private readonly ILogger<AdvertController> _logger;

    public AdvertController(IAdvertStorageService advertStorageService, ILogger<AdvertController> logger)
    {
      _advertStorageService = advertStorageService;
      _logger = logger;
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
  }
}