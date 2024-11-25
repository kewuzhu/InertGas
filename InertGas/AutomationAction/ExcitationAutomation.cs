using InertGas.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertGas.Application.AutomationAction
{
    internal class ExcitationAutomation : AutomationAction
    {
        public ExcitationAutomation()
        {
            CurrentPhase = Model.WorkingPhases.Excitation;

            var plcControls = AppModel.HardwareControls.Where(x => x.HardwareType == HardwareTypes.Plc).ToList();

            internalActions_.Add(async (token) => await Task.Delay(AppModel.CollectionDuration * 1000, token));
        }

        protected override async Task InternalExecute(CancellationTokenSource cancellationTokenSource)
        {
            for (int i = 0; i < internalActions_.Count; i++)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                logger_.Info($"Excuting WorkingPhase{CurrentPhase}, instruction{i + 1}");

                await internalActions_[i].Invoke(cancellationTokenSource.Token);
            }
        }
    }
}
