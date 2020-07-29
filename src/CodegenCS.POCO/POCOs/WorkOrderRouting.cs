using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("WorkOrderRouting", Schema = "Production")]
    public partial class WorkOrderRouting : INotifyPropertyChanged
    {
        #region Members
        private int _workOrderId;
        [Key]
        public int WorkOrderId 
        { 
            get { return _workOrderId; } 
            set { SetField(ref _workOrderId, value, nameof(WorkOrderId)); } 
        }
        private int _productId;
        [Key]
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private short _operationSequence;
        [Key]
        public short OperationSequence 
        { 
            get { return _operationSequence; } 
            set { SetField(ref _operationSequence, value, nameof(OperationSequence)); } 
        }
        private decimal? _actualCost;
        public decimal? ActualCost 
        { 
            get { return _actualCost; } 
            set { SetField(ref _actualCost, value, nameof(ActualCost)); } 
        }
        private DateTime? _actualEndDate;
        public DateTime? ActualEndDate 
        { 
            get { return _actualEndDate; } 
            set { SetField(ref _actualEndDate, value, nameof(ActualEndDate)); } 
        }
        private decimal? _actualResourceHrs;
        public decimal? ActualResourceHrs 
        { 
            get { return _actualResourceHrs; } 
            set { SetField(ref _actualResourceHrs, value, nameof(ActualResourceHrs)); } 
        }
        private DateTime? _actualStartDate;
        public DateTime? ActualStartDate 
        { 
            get { return _actualStartDate; } 
            set { SetField(ref _actualStartDate, value, nameof(ActualStartDate)); } 
        }
        private short _locationId;
        public short LocationId 
        { 
            get { return _locationId; } 
            set { SetField(ref _locationId, value, nameof(LocationId)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private decimal _plannedCost;
        public decimal PlannedCost 
        { 
            get { return _plannedCost; } 
            set { SetField(ref _plannedCost, value, nameof(PlannedCost)); } 
        }
        private DateTime _scheduledEndDate;
        public DateTime ScheduledEndDate 
        { 
            get { return _scheduledEndDate; } 
            set { SetField(ref _scheduledEndDate, value, nameof(ScheduledEndDate)); } 
        }
        private DateTime _scheduledStartDate;
        public DateTime ScheduledStartDate 
        { 
            get { return _scheduledStartDate; } 
            set { SetField(ref _scheduledStartDate, value, nameof(ScheduledStartDate)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (WorkOrderId == default(int) && ProductId == default(int) && OperationSequence == default(short))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[WorkOrderRouting]
                (
                    [ActualCost],
                    [ActualEndDate],
                    [ActualResourceHrs],
                    [ActualStartDate],
                    [LocationID],
                    [ModifiedDate],
                    [OperationSequence],
                    [PlannedCost],
                    [ProductID],
                    [ScheduledEndDate],
                    [ScheduledStartDate],
                    [WorkOrderID]
                )
                VALUES
                (
                    @ActualCost,
                    @ActualEndDate,
                    @ActualResourceHrs,
                    @ActualStartDate,
                    @LocationId,
                    @ModifiedDate,
                    @OperationSequence,
                    @PlannedCost,
                    @ProductId,
                    @ScheduledEndDate,
                    @ScheduledStartDate,
                    @WorkOrderId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[WorkOrderRouting] SET
                    [ActualCost] = @ActualCost,
                    [ActualEndDate] = @ActualEndDate,
                    [ActualResourceHrs] = @ActualResourceHrs,
                    [ActualStartDate] = @ActualStartDate,
                    [LocationID] = @LocationId,
                    [ModifiedDate] = @ModifiedDate,
                    [OperationSequence] = @OperationSequence,
                    [PlannedCost] = @PlannedCost,
                    [ProductID] = @ProductId,
                    [ScheduledEndDate] = @ScheduledEndDate,
                    [ScheduledStartDate] = @ScheduledStartDate,
                    [WorkOrderID] = @WorkOrderId
                WHERE
                    [WorkOrderID] = @WorkOrderId AND 
                    [ProductID] = @ProductId AND 
                    [OperationSequence] = @OperationSequence";
                conn.Execute(cmd, this);
            }
        }
        #endregion ActiveRecord

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            WorkOrderRouting other = obj as WorkOrderRouting;
            if (other == null) return false;

            if (ActualCost != other.ActualCost)
                return false;
            if (ActualEndDate != other.ActualEndDate)
                return false;
            if (ActualResourceHrs != other.ActualResourceHrs)
                return false;
            if (ActualStartDate != other.ActualStartDate)
                return false;
            if (LocationId != other.LocationId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OperationSequence != other.OperationSequence)
                return false;
            if (PlannedCost != other.PlannedCost)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (ScheduledEndDate != other.ScheduledEndDate)
                return false;
            if (ScheduledStartDate != other.ScheduledStartDate)
                return false;
            if (WorkOrderId != other.WorkOrderId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ActualCost == null ? 0 : ActualCost.GetHashCode());
                hash = hash * 23 + (ActualEndDate == null ? 0 : ActualEndDate.GetHashCode());
                hash = hash * 23 + (ActualResourceHrs == null ? 0 : ActualResourceHrs.GetHashCode());
                hash = hash * 23 + (ActualStartDate == null ? 0 : ActualStartDate.GetHashCode());
                hash = hash * 23 + (LocationId == default(short) ? 0 : LocationId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OperationSequence == default(short) ? 0 : OperationSequence.GetHashCode());
                hash = hash * 23 + (PlannedCost == default(decimal) ? 0 : PlannedCost.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (ScheduledEndDate == default(DateTime) ? 0 : ScheduledEndDate.GetHashCode());
                hash = hash * 23 + (ScheduledStartDate == default(DateTime) ? 0 : ScheduledStartDate.GetHashCode());
                hash = hash * 23 + (WorkOrderId == default(int) ? 0 : WorkOrderId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(WorkOrderRouting left, WorkOrderRouting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WorkOrderRouting left, WorkOrderRouting right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}
