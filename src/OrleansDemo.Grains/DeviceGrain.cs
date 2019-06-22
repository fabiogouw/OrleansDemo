using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using OrleansDemo.Contracts;

namespace OrleansDemo.Grains
{
    [StorageProvider(ProviderName="Devices")]
    public class DeviceGrain : Grain<Device>, IDevice
    {
        public Task<double> GetTemperature()
        {
            return Task.FromResult(State.LastTemperature);
        }

        public async Task SetTemperature(double temperature)
        {
            State.LastTemperature = temperature;
            await WriteStateAsync();
        }
    }
}
