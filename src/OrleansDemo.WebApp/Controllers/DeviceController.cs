using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansDemo.Contracts;

namespace OrleansDemo.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IClusterClient _client;
        
        public DeviceController(IClusterClient client)
        {
            _client = client;
        }

        // GET api/device/5
        [HttpGet("{deviceId}")]
        public async Task<double> Get(int deviceId)
        {
            var device = _client.GetGrain<IDevice>(deviceId);
            return await device.GetTemperature();
        }

        // PUT api/device/5
        [HttpPut("{deviceId}")]
        public async Task Put(int deviceId, [FromForm] string value)
        {
            var device = _client.GetGrain<IDevice>(deviceId);
            await device.SetTemperature(double.Parse(value));
        }
    }
}
