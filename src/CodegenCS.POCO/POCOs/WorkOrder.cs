﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("WorkOrder", Schema = "Production")]
    public partial class WorkOrder : INotifyPropertyChanged
    {
        #region Members
        private int _workOrderId;
        [Key]
        public int WorkOrderId 
        { 
            get { return _workOrderId; } 
            set { SetField(ref _workOrderId, value, nameof(WorkOrderId)); } 
        }
        private DateTime _dueDate;
        public DateTime DueDate 
        { 
            get { return _dueDate; } 
            set { SetField(ref _dueDate, value, nameof(DueDate)); } 
        }
        private DateTime? _endDate;
        public DateTime? EndDate 
        { 
            get { return _endDate; } 
            set { SetField(ref _endDate, value, nameof(EndDate)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private int _orderQty;
        public int OrderQty 
        { 
            get { return _orderQty; } 
            set { SetField(ref _orderQty, value, nameof(OrderQty)); } 
        }
        private int _productId;
        public int ProductId 
        { 
            get { return _productId; } 
            set { SetField(ref _productId, value, nameof(ProductId)); } 
        }
        private short _scrappedQty;
        public short ScrappedQty 
        { 
            get { return _scrappedQty; } 
            set { SetField(ref _scrappedQty, value, nameof(ScrappedQty)); } 
        }
        private short? _scrapReasonId;
        public short? ScrapReasonId 
        { 
            get { return _scrapReasonId; } 
            set { SetField(ref _scrapReasonId, value, nameof(ScrapReasonId)); } 
        }
        private DateTime _startDate;
        public DateTime StartDate 
        { 
            get { return _startDate; } 
            set { SetField(ref _startDate, value, nameof(StartDate)); } 
        }
        private int _stockedQty;
        public int StockedQty 
        { 
            get { return _stockedQty; } 
            set { SetField(ref _stockedQty, value, nameof(StockedQty)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (WorkOrderId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[WorkOrder]
                (
                    [DueDate],
                    [EndDate],
                    [ModifiedDate],
                    [OrderQty],
                    [ProductID],
                    [ScrappedQty],
                    [ScrapReasonID],
                    [StartDate]
                )
                VALUES
                (
                    @DueDate,
                    @EndDate,
                    @ModifiedDate,
                    @OrderQty,
                    @ProductId,
                    @ScrappedQty,
                    @ScrapReasonId,
                    @StartDate
                )";

                this.WorkOrderId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[WorkOrder] SET
                    [DueDate] = @DueDate,
                    [EndDate] = @EndDate,
                    [ModifiedDate] = @ModifiedDate,
                    [OrderQty] = @OrderQty,
                    [ProductID] = @ProductId,
                    [ScrappedQty] = @ScrappedQty,
                    [ScrapReasonID] = @ScrapReasonId,
                    [StartDate] = @StartDate
                WHERE
                    [WorkOrderID] = @WorkOrderId";
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
            WorkOrder other = obj as WorkOrder;
            if (other == null) return false;

            if (DueDate != other.DueDate)
                return false;
            if (EndDate != other.EndDate)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (OrderQty != other.OrderQty)
                return false;
            if (ProductId != other.ProductId)
                return false;
            if (ScrappedQty != other.ScrappedQty)
                return false;
            if (ScrapReasonId != other.ScrapReasonId)
                return false;
            if (StartDate != other.StartDate)
                return false;
            if (StockedQty != other.StockedQty)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (DueDate == default(DateTime) ? 0 : DueDate.GetHashCode());
                hash = hash * 23 + (EndDate == null ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (OrderQty == default(int) ? 0 : OrderQty.GetHashCode());
                hash = hash * 23 + (ProductId == default(int) ? 0 : ProductId.GetHashCode());
                hash = hash * 23 + (ScrappedQty == default(short) ? 0 : ScrappedQty.GetHashCode());
                hash = hash * 23 + (ScrapReasonId == null ? 0 : ScrapReasonId.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                hash = hash * 23 + (StockedQty == default(int) ? 0 : StockedQty.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(WorkOrder left, WorkOrder right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WorkOrder left, WorkOrder right)
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
