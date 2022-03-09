using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Blazor.Components;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using ListEditorScheduler.Module.BusinessObjects;
using Microsoft.AspNetCore.Components;

namespace ListEditorScheduler.Module.Blazor.Editors.SchedulerListEditor
{
    [ListEditor(typeof(Task))]
    public class SchedulerListEditor : ListEditor, IComplexListEditor
    {
        private CollectionSourceBase collectionSource;
        private XafApplication application;

        public override SelectionType SelectionType => SelectionType.Full;
        public override IList GetSelectedObjects() => selectedObjects;

        public class TaskItemListViewHolder : IComponentContentHolder
        {
            private RenderFragment componentContent;
            public TaskItemListViewHolder(SchedulerListViewModel componentModel)
            {
                ComponentModel =
                    componentModel ?? throw new ArgumentNullException(nameof(componentModel));
            }
            private RenderFragment CreateComponent() =>
                ComponentModelObserver.Create(ComponentModel,
                                                SchedulerListViewRenderer.Create(ComponentModel));
            public SchedulerListViewModel ComponentModel { get; }
            RenderFragment IComponentContentHolder.ComponentContent =>
                componentContent ??= CreateComponent();
        }
        // ...
        public SchedulerListEditor(IModelListView model) : base(model) { }

        protected override object CreateControlsCore()
        {
            return new TaskItemListViewHolder(new SchedulerListViewModel());
        }
        private Task[] selectedObjects = Array.Empty<Task>();
        // ...
        protected override void OnControlsCreated()
        {
            if (Control is TaskItemListViewHolder holder)
            {
                holder.ComponentModel.AppointmentInserted += ComponentModel_AppointmentInserted;
                holder.ComponentModel.AppointmentUpdated += ComponentModel_AppointmentUpdated;
                holder.ComponentModel.AppointmentRemoved += ComponentModel_AppointmentRemoved;
                holder.ComponentModel.AppointmentInserting += ComponentModel_AppointmentInserting;
            }
            base.OnControlsCreated();
        }

        private void ComponentModel_AppointmentInserting(object sender, SchedulerAppointmentOperationEventArgs e)
        {
            e.Appointment.Id = Guid.NewGuid();
        }

        private void ComponentModel_AppointmentInserted(object sender, 
            SchedulerListViewModelAppointmentItemEventArgs e)
        {
            IObjectSpace objectSpace = collectionSource.ObjectSpace;
            ListEditorScheduler.Module.BusinessObjects.Task newTask = objectSpace.CreateObject<ListEditorScheduler.Module.BusinessObjects.Task>();
            newTask.AllDay = e.AppointmentItem.AllDay;
            newTask.Subject = e.AppointmentItem.Subject;
            newTask.Description = e.AppointmentItem.Description;
            newTask.EndDate = e.AppointmentItem.End;
            if (e.AppointmentItem.LabelId != null)
                newTask.Label = int.Parse(e.AppointmentItem.LabelId.ToString());
            newTask.Location = e.AppointmentItem.Location;
            newTask.StartOn = e.AppointmentItem.Start;
            newTask.Status = int.Parse(e.AppointmentItem.StatusId.ToString());
            newTask.RecurrenceInfoXml = e.AppointmentItem.RecurrenceInfo?.ToXml();
            newTask.Id = (System.Guid)e.AppointmentItem.Id;
            objectSpace.CommitChanges();

            //HACK: To Refresh Control datasource and see added Task in UI
            IList<Task> Tasks = objectSpace.GetObjects<Task>();
            AssignDataSourceToControl(Tasks); 
        }

        private void ComponentModel_AppointmentUpdated(object sender, SchedulerListViewModelAppointmentItemEventArgs e)
        {
            ConvertToAppointment(e.AppointmentItem);
        }

        private void ConvertToAppointment(DxSchedulerAppointmentItem appointmentItem)
        {
            IObjectSpace objectSpace = collectionSource.ObjectSpace;
            ListEditorScheduler.Module.BusinessObjects.Task task = objectSpace.FindObject<ListEditorScheduler.Module.BusinessObjects.Task>(new DevExpress.Data.Filtering.BinaryOperator("Id", appointmentItem.Id));
            if (task == null)
            {
                return;
            }
            task.AllDay = appointmentItem.AllDay;
            task.Subject = appointmentItem.Subject;
            task.Description = appointmentItem.Description;
            task.EndDate = appointmentItem.End;
            task.Label = int.Parse(appointmentItem.LabelId.ToString());
            task.Location = appointmentItem.Location;
            task.StartOn = appointmentItem.Start;
            task.Status = int.Parse(appointmentItem.StatusId.ToString());
            task.RecurrenceInfoXml = appointmentItem.RecurrenceInfo?.ToXml();
            task.Id = (System.Guid)appointmentItem.Id;
            objectSpace.CommitChanges();

            IList<Task> Tasks = objectSpace.GetObjects<Task>();
            AssignDataSourceToControl(Tasks);
        }

        private void ComponentModel_AppointmentRemoved(object sender, SchedulerListViewModelAppointmentItemEventArgs e)
        {
            IObjectSpace objectSpace = collectionSource.ObjectSpace;
            ListEditorScheduler.Module.BusinessObjects.Task task = objectSpace.FindObject<ListEditorScheduler.Module.BusinessObjects.Task>(new DevExpress.Data.Filtering.BinaryOperator("Id", e.AppointmentItem.Id));
            task.Delete();
            objectSpace.CommitChanges();

            IList<Task> Tasks = objectSpace.GetObjects<Task>();
            AssignDataSourceToControl(Tasks);
        }

        // ...
        private void ComponentModel_ItemClick(object sender,
                                                SchedulerListViewModelItemClickEventArgs e)
        {
            selectedObjects = new Task[] { e.Item };
            OnSelectionChanged();
            OnProcessSelectedItem();
        }
        public override void BreakLinksToControls()
        {
            if (Control is TaskItemListViewHolder holder)
            {
                holder.ComponentModel.AppointmentInserted -= ComponentModel_AppointmentInserted;
                holder.ComponentModel.AppointmentUpdated -= ComponentModel_AppointmentUpdated;
                holder.ComponentModel.AppointmentRemoved -= ComponentModel_AppointmentRemoved;
                holder.ComponentModel.AppointmentInserting -= ComponentModel_AppointmentInserting;
            }
            AssignDataSourceToControl(null);
            base.BreakLinksToControls();
        }
        protected override void AssignDataSourceToControl(object dataSource)
        {
            if (Control is TaskItemListViewHolder holder)
            {
                if (holder.ComponentModel.Data is IBindingList bindingList)
                {
                    bindingList.ListChanged -= BindingList_ListChanged;
                }
                holder.ComponentModel.Data =
                    (dataSource as IEnumerable)?.OfType<Task>().OrderBy(i => i.StartOn);
                if (dataSource is IBindingList newBindingList)
                {
                    newBindingList.ListChanged += BindingList_ListChanged;
                }
            }
        }

        private void BindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            Refresh();
        }



        public override void Refresh()
        {
            if (Control is TaskItemListViewHolder holder)
            {
                holder.ComponentModel.Refresh();
            }
        }

        #region IComplexListEditor Members
        public void Setup(CollectionSourceBase collectionSource, XafApplication application)
        {
            this.collectionSource = collectionSource;
            this.application = application;
        }
        #endregion

    }
}
