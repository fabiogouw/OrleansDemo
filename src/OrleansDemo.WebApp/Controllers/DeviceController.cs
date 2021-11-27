using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansDemo.Contracts;
using System;
using System.Threading.Tasks;

namespace OrleansDemo.WebApp.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DeviceController : ControllerBase
    {
        private readonly IClusterClient _client;
        
        public DeviceController(IClusterClient client)
        {
            _client = client;
        }

        // GET api/devices/5
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> Get(int deviceId)
        {
            try 
            {
                var device = _client.GetGrain<IDevice>(deviceId);
                double temperature = await device.GetTemperature();
                return Ok(temperature);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // PUT api/devices/5
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> Put(int deviceId, [FromForm] string value)
        {
            try
            {
                var device = _client.GetGrain<IDevice>(deviceId);
                await device.SetTemperature(double.Parse(value));
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}