﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CarServiceGame.Domain.Entities;

namespace CarServiceGame.Domain.Contracts
{
    public interface IWorkerRepository
    {
        IEnumerable<Worker> GetUnemployedWorkers(int skip, int take);
        void FireWorker(Guid garageId, Guid workerId);
        void EmployWorker(Guid garageId, Guid workerId);
        void UpgradeWorker(Guid garageId, Guid workerId, decimal cost);
    }
}
