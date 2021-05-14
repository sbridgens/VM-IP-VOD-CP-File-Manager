using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VM_IP_VOD_CP_File_Manager.BusinessLogic.Contracts
{  
    public interface IWorkflowProcessor
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
