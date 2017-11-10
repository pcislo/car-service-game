﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CarServiceGame.Domain.Concrete;
using CarServiceGame.Domain.Contracts;
using CarServiceGame.Domain.Entities;
using CarServiceGame.Domain.Mock;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CarServiceGame.Desktop.ViewModels
{
    public class GarageViewModel : ObservableObject
    {
        private readonly int numberOfStalls = 4;

        private IWorkerRepository workersRepository;
        private IOrderRepository ordersRepository;
        private Garage model;

        private RepairProcessViewModel[] stalls;

        public RepairProcessViewModel[] Stalls
        {
            get
            {
                return stalls;
            }
            set
            {
                stalls = value;
                RaisePropertyChanged("Stalls");
            }
        }

        public ObservableCollection<WorkerViewModel> EmployeedWorkers { get; }

        public ObservableCollection<WorkerViewModel> AvailableWorkers
        {
            get
            {
                return new ObservableCollection<WorkerViewModel>((EmployeedWorkers.Where(x =>
               {
                   foreach (var v in Stalls)
                   {
                       if (v != null && v.AssignedWorker.Equals(x)) return false;
                   }
                   return true;
               })).ToList());
            }
        }

        public GarageViewModel(Garage model, IWorkerRepository workerRepository)
        {
            this.model = model;
            this.workersRepository = workerRepository;
            ordersRepository = new OrderRepository();

            EmployeedWorkers = new ObservableCollection<WorkerViewModel>(from w in model.EmployeedWorkers select new WorkerViewModel(w));
            RaisePropertyChanged("EmployeedWorkers");

            Stalls = new RepairProcessViewModel[numberOfStalls];
            int i = 0;
            foreach (var v in model.RepairProcesses)
            {
                Stalls[i++] = new RepairProcessViewModel(v, v.StallNumber);
            }

            RaisePropertyChanged("Stalls");
            RaisePropertyChanged("AvailableWorkers");
        }

        public GarageViewModel(Garage model)
        {
            this.model = model;
            workersRepository = new WorkerRepository();
            ordersRepository = new OrderRepository();

            EmployeedWorkers = new ObservableCollection<WorkerViewModel>(from w in model.EmployeedWorkers select new WorkerViewModel(w));
            RaisePropertyChanged("EmployeedWorkers");

            Stalls = new RepairProcessViewModel[numberOfStalls];
            int i = 0;
            foreach(var v in model.RepairProcesses)
            { 
                Stalls[v.StallNumber] = new RepairProcessViewModel(v, v.StallNumber);
            }
            RaisePropertyChanged("Stalls");
            RaisePropertyChanged("AvailableWorkers");
        }

        public ICommand FireWorker => new RelayCommand<WorkerViewModel>(w =>
        {
            var window = (Application.Current.MainWindow as MetroWindow);
            var progressDialog = window.ShowProgressAsync("Please wait...", "Firing worker...", false);

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(() =>
            {
                progressDialog.Result.SetIndeterminate();

                workersRepository.FireWorker(w.GetModel().WorkerId);

                progressDialog.Result.CloseAsync();

            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    window.ShowMessageAsync("", "Error while firing worker").Wait();
                }
                else
                {
                    GlobalResources.AvailableWorkers.Add(w);
                    model.FireWorker(w.GetModel());
                    EmployeedWorkers.Remove(w);
                }
            }, scheduler);

        }, w => model.RepairProcesses.All(x => x.AssignedWorker.WorkerId != w.GetModel().WorkerId));


        public ICommand FinishJob => new RelayCommand<RepairProcessViewModel>(rp =>
        {
            var window = (Application.Current.MainWindow as MetroWindow);
            var progressDialog = window.ShowProgressAsync("Please wait...", "Finishing job...", false);

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(() =>
            {
                progressDialog.Result.SetIndeterminate();

                ordersRepository.FinishOrder(rp.Order.GetModel().RepairOrderId);

                progressDialog.Result.CloseAsync();

            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    window.ShowMessageAsync("", "Error while finishing job").Wait();
                }
                else
                {
                    Stalls[rp.StallNumber] = null;
                    RaisePropertyChanged("Stalls");
                    RaisePropertyChanged("AvailableWorkers");
                }
            }, scheduler);

        });


        public void AssignOrderToStall(int stallNumber, OrderViewModel order, WorkerViewModel worker)
        {
            RepairProcessViewModel repairProcess = new RepairProcessViewModel(order, worker, stallNumber);
            var window = (Application.Current.MainWindow as MetroWindow);
            var progressDialog = window.ShowProgressAsync("Please wait...", "Accpeting order...", false);
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(() =>
            {
                progressDialog.Result.SetIndeterminate();

                ordersRepository.AssignOrder(model.GarageId, order.GetModel().RepairOrderId, worker.GetModel().WorkerId, stallNumber);

                progressDialog.Result.CloseAsync();

            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    window.ShowMessageAsync("", "Error while accepting order").Wait();
                }
                else
                {
                    Stalls[stallNumber] = repairProcess;
                    RaisePropertyChanged("Stalls");
                    RaisePropertyChanged("AvailableWorkers");
                }
            }, scheduler);
        }

        public void HireWorker(WorkerViewModel worker)
        {
            model.HireWorker(worker.GetModel());
            EmployeedWorkers.Add(worker);
        }

        public Garage GetModel()
        {
            return model;
        }

    }
}
