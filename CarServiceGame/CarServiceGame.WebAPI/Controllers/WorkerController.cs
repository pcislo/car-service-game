﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CarServiceGame.Domain.Contracts;
using CarServiceGame.WebAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CarServiceGame.WebAPI.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    public class WorkerController : Controller
    {
        private IWorkerRepository workerRepository;

        public WorkerController(IWorkerRepository workerRepository)
        {
            this.workerRepository = workerRepository;
        }

        [HttpGet("api/v{version:apiVersion}/Workers")]
        public IActionResult Index(int skip = 0, int take = 20)
        {
            dynamic result = new ExpandoObject();
            try
            {
                string orders = JsonConvert.SerializeObject(workerRepository.GetUnemployedWorkers(skip, take));
                return Ok(orders);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.status = "error";
                return BadRequest(result);
            }
        }

        [HttpPost("api/v{version:apiVersion}/Workers/Fire")]
        public IActionResult FireWorker([ModelBinder(BinderType = typeof(GarageIdBinder))]Guid garageId, Guid workerId)
        {
            dynamic result = new ExpandoObject();
            try
            {
                workerRepository.FireWorker(garageId, workerId);
                result.status = "ok";
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.status = "error";
                return BadRequest(result);
            }
        }

        [HttpPost("api/v{version:apiVersion}/Workers/Employ")]
        public IActionResult EmployWorker([ModelBinder(BinderType = typeof(GarageIdBinder))]Guid garageId, Guid workerId)
        {
            dynamic result = new ExpandoObject();
            try
            {
                workerRepository.EmployWorker(garageId, workerId);
                result.status = "ok";
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.status = "error";
                return BadRequest(result);
            }
        }

        [HttpPost("api/v{version:apiVersion}/Workers/Upgrade")]
        public IActionResult UpgradeWorker([ModelBinder(BinderType = typeof(GarageIdBinder))]Guid garageId, Guid workerId, decimal cost)
        {
            dynamic result = new ExpandoObject();
            try
            {
                workerRepository.UpgradeWorker(garageId, workerId, cost);
                result.status = "ok";
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.status = "error";
                return BadRequest(result);
            }
        }
    }
}