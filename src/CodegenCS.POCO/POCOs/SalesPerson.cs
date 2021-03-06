﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("SalesPerson", Schema = "Sales")]
    public partial class SalesPerson : INotifyPropertyChanged
    {
        #region Members
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private decimal _bonus;
        public decimal Bonus 
        { 
            get { return _bonus; } 
            set { SetField(ref _bonus, value, nameof(Bonus)); } 
        }
        private decimal _commissionPct;
        public decimal CommissionPct 
        { 
            get { return _commissionPct; } 
            set { SetField(ref _commissionPct, value, nameof(CommissionPct)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private decimal _salesLastYear;
        public decimal SalesLastYear 
        { 
            get { return _salesLastYear; } 
            set { SetField(ref _salesLastYear, value, nameof(SalesLastYear)); } 
        }
        private decimal? _salesQuota;
        public decimal? SalesQuota 
        { 
            get { return _salesQuota; } 
            set { SetField(ref _salesQuota, value, nameof(SalesQuota)); } 
        }
        private decimal _salesYtd;
        public decimal SalesYtd 
        { 
            get { return _salesYtd; } 
            set { SetField(ref _salesYtd, value, nameof(SalesYtd)); } 
        }
        private int? _territoryId;
        public int? TerritoryId 
        { 
            get { return _territoryId; } 
            set { SetField(ref _territoryId, value, nameof(TerritoryId)); } 
        }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Sales].[SalesPerson]
                (
                    [Bonus],
                    [BusinessEntityID],
                    [CommissionPct],
                    [ModifiedDate],
                    [SalesLastYear],
                    [SalesQuota],
                    [SalesYTD],
                    [TerritoryID]
                )
                VALUES
                (
                    @Bonus,
                    @BusinessEntityId,
                    @CommissionPct,
                    @ModifiedDate,
                    @SalesLastYear,
                    @SalesQuota,
                    @SalesYtd,
                    @TerritoryId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Sales].[SalesPerson] SET
                    [Bonus] = @Bonus,
                    [BusinessEntityID] = @BusinessEntityId,
                    [CommissionPct] = @CommissionPct,
                    [ModifiedDate] = @ModifiedDate,
                    [SalesLastYear] = @SalesLastYear,
                    [SalesQuota] = @SalesQuota,
                    [SalesYTD] = @SalesYtd,
                    [TerritoryID] = @TerritoryId
                WHERE
                    [BusinessEntityID] = @BusinessEntityId";
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
            SalesPerson other = obj as SalesPerson;
            if (other == null) return false;

            if (Bonus != other.Bonus)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (CommissionPct != other.CommissionPct)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalesLastYear != other.SalesLastYear)
                return false;
            if (SalesQuota != other.SalesQuota)
                return false;
            if (SalesYtd != other.SalesYtd)
                return false;
            if (TerritoryId != other.TerritoryId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Bonus == default(decimal) ? 0 : Bonus.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (CommissionPct == default(decimal) ? 0 : CommissionPct.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalesLastYear == default(decimal) ? 0 : SalesLastYear.GetHashCode());
                hash = hash * 23 + (SalesQuota == null ? 0 : SalesQuota.GetHashCode());
                hash = hash * 23 + (SalesYtd == default(decimal) ? 0 : SalesYtd.GetHashCode());
                hash = hash * 23 + (TerritoryId == null ? 0 : TerritoryId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SalesPerson left, SalesPerson right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SalesPerson left, SalesPerson right)
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
