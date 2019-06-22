using System;
using System.Threading.Tasks;
using Orleans;

namespace OrleansDemo.Contracts
{
    public interface IDevice : IGrainWithIntegerKey
    {
        Task SetTemperature(double temperature);
        Task<double> GetTemperature();
    }
}
