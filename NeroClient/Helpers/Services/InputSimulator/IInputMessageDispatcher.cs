using NeroClient.Helpers.Services.InputSimulator.Native;

namespace NeroClient.Helpers.Services.InputSimulator
{
    internal interface IInputMessageDispatcher
    {
        void DispatchInput(INPUT[] inputs);
    }
}