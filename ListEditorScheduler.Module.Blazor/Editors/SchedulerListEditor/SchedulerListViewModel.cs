using DevExpress.Blazor;
using DevExpress.ExpressApp.Blazor.Components.Models;
using ListEditorScheduler.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListEditorScheduler.Module.Blazor.Editors.SchedulerListEditor
{
    public class SchedulerListViewModel : ComponentModelBase
    {
        public IEnumerable<ListEditorScheduler.Module.BusinessObjects.Task> Data
        {
            get => GetPropertyValue<IEnumerable<ListEditorScheduler.Module.BusinessObjects.Task>>();
            set => SetPropertyValue(value);
        }

        DxSchedulerDataStorage DataStorage;
        public DxSchedulerDataStorage GetDataStorage()
        {
            var dataSource = new List<AppointmentPOCO>();
            if (Data is not null)
            {
                foreach (var item in Data)
                {
                    dataSource.Add(ConvertToAppointmentPOCO(item));
                }

                DataStorage = new DxSchedulerDataStorage()
                {
                    AppointmentsSource = dataSource,
                    AppointmentMappings = new DxSchedulerAppointmentMappings()
                    {
                        Type = "AppointmentType",
                        Start = "StartDate",
                        End = "EndDate",
                        Subject = "Caption",
                        AllDay = "AllDay",
                        Location = "Location",
                        Description = "Description",
                        LabelId = "Label",
                        StatusId = "Status",
                        RecurrenceInfo = "Recurrence",
                        Id = "OidPoco"
                    }
                };
            }
            
            return DataStorage;
        }

        public static AppointmentPOCO ConvertToAppointmentPOCO(ListEditorScheduler.Module.BusinessObjects.Task item)
        {
            AppointmentPOCO appointmentPOCO = new AppointmentPOCO
            {
                AllDay = item.AllDay,
                AppointmentType = item.Type,
                Caption = item.Subject,
                Description = item.Description,
                EndDate = item.EndDate,
                Label = item.Label,
                Location = item.Location,
                Recurrence = item.RecurrenceInfoXml,
                StartDate = item.StartOn,
                Status = item.Status,
                OidPoco = item.Id
            };

            return appointmentPOCO;
        }

        public void Refresh() => RaiseChanged();

        public event EventHandler<SchedulerListViewModelAppointmentItemEventArgs> AppointmentInserted;
        public void OnAppointmentInserted(DxSchedulerAppointmentItem appointmentItem) =>
            AppointmentInserted?.Invoke(this, new SchedulerListViewModelAppointmentItemEventArgs(appointmentItem));
        
        public event EventHandler<SchedulerListViewModelAppointmentItemEventArgs> AppointmentUpdated;
        public void OnAppointmentUpdated(DxSchedulerAppointmentItem appointmentItem) =>
            AppointmentUpdated?.Invoke(this, new SchedulerListViewModelAppointmentItemEventArgs(appointmentItem));
        
        public event EventHandler<SchedulerListViewModelAppointmentItemEventArgs> AppointmentRemoved;
        public void OnAppointmentRemoved(DxSchedulerAppointmentItem appointmentItem) =>
            AppointmentRemoved?.Invoke(this, new SchedulerListViewModelAppointmentItemEventArgs(appointmentItem));
        
        public event EventHandler<SchedulerAppointmentOperationEventArgs> AppointmentInserting;
        public void OnAppointmentInserting(SchedulerAppointmentOperationEventArgs e) =>
            AppointmentInserting?.Invoke(this, e);
    }

    public class SchedulerListViewModelAppointmentItemEventArgs : EventArgs
    {
        public SchedulerListViewModelAppointmentItemEventArgs(DxSchedulerAppointmentItem appointmentItem)
        {
            AppointmentItem = appointmentItem;
        }
        public DxSchedulerAppointmentItem AppointmentItem { get; }
    }

    public class SchedulerListViewModelItemClickEventArgs : EventArgs
    {
        public SchedulerListViewModelItemClickEventArgs(ListEditorScheduler.Module.BusinessObjects.Task item)
        {
            Item = item;
        }
        public ListEditorScheduler.Module.BusinessObjects.Task Item { get; }
    }
}
