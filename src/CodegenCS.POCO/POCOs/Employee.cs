using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Employee", Schema = "HumanResources")]
    public partial class Employee : INotifyPropertyChanged
    {
        #region Members
        private int _businessEntityId;
        [Key]
        public int BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private DateTime _birthDate;
        public DateTime BirthDate 
        { 
            get { return _birthDate; } 
            set { SetField(ref _birthDate, value, nameof(BirthDate)); } 
        }
        private bool _currentFlag;
        public bool CurrentFlag 
        { 
            get { return _currentFlag; } 
            set { SetField(ref _currentFlag, value, nameof(CurrentFlag)); } 
        }
        private string _gender;
        public string Gender 
        { 
            get { return _gender; } 
            set { SetField(ref _gender, value, nameof(Gender)); } 
        }
        private DateTime _hireDate;
        public DateTime HireDate 
        { 
            get { return _hireDate; } 
            set { SetField(ref _hireDate, value, nameof(HireDate)); } 
        }
        private string _jobTitle;
        public string JobTitle 
        { 
            get { return _jobTitle; } 
            set { SetField(ref _jobTitle, value, nameof(JobTitle)); } 
        }
        private string _loginId;
        public string LoginId 
        { 
            get { return _loginId; } 
            set { SetField(ref _loginId, value, nameof(LoginId)); } 
        }
        private string _maritalStatus;
        public string MaritalStatus 
        { 
            get { return _maritalStatus; } 
            set { SetField(ref _maritalStatus, value, nameof(MaritalStatus)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private string _nationalIdNumber;
        public string NationalIdNumber 
        { 
            get { return _nationalIdNumber; } 
            set { SetField(ref _nationalIdNumber, value, nameof(NationalIdNumber)); } 
        }
        private short? _organizationLevel;
        public short? OrganizationLevel 
        { 
            get { return _organizationLevel; } 
            set { SetField(ref _organizationLevel, value, nameof(OrganizationLevel)); } 
        }
        private Guid _rowguid;
        public Guid Rowguid 
        { 
            get { return _rowguid; } 
            set { SetField(ref _rowguid, value, nameof(Rowguid)); } 
        }
        private bool _salariedFlag;
        public bool SalariedFlag 
        { 
            get { return _salariedFlag; } 
            set { SetField(ref _salariedFlag, value, nameof(SalariedFlag)); } 
        }
        private short _sickLeaveHours;
        public short SickLeaveHours 
        { 
            get { return _sickLeaveHours; } 
            set { SetField(ref _sickLeaveHours, value, nameof(SickLeaveHours)); } 
        }
        private short _vacationHours;
        public short VacationHours 
        { 
            get { return _vacationHours; } 
            set { SetField(ref _vacationHours, value, nameof(VacationHours)); } 
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
                INSERT INTO [HumanResources].[Employee]
                (
                    [BirthDate],
                    [BusinessEntityID],
                    [CurrentFlag],
                    [Gender],
                    [HireDate],
                    [JobTitle],
                    [LoginID],
                    [MaritalStatus],
                    [ModifiedDate],
                    [NationalIDNumber],
                    [SalariedFlag],
                    [SickLeaveHours],
                    [VacationHours]
                )
                VALUES
                (
                    @BirthDate,
                    @BusinessEntityId,
                    @CurrentFlag,
                    @Gender,
                    @HireDate,
                    @JobTitle,
                    @LoginId,
                    @MaritalStatus,
                    @ModifiedDate,
                    @NationalIdNumber,
                    @SalariedFlag,
                    @SickLeaveHours,
                    @VacationHours
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [HumanResources].[Employee] SET
                    [BirthDate] = @BirthDate,
                    [BusinessEntityID] = @BusinessEntityId,
                    [CurrentFlag] = @CurrentFlag,
                    [Gender] = @Gender,
                    [HireDate] = @HireDate,
                    [JobTitle] = @JobTitle,
                    [LoginID] = @LoginId,
                    [MaritalStatus] = @MaritalStatus,
                    [ModifiedDate] = @ModifiedDate,
                    [NationalIDNumber] = @NationalIdNumber,
                    [SalariedFlag] = @SalariedFlag,
                    [SickLeaveHours] = @SickLeaveHours,
                    [VacationHours] = @VacationHours
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
            Employee other = obj as Employee;
            if (other == null) return false;

            if (BirthDate != other.BirthDate)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (CurrentFlag != other.CurrentFlag)
                return false;
            if (Gender != other.Gender)
                return false;
            if (HireDate != other.HireDate)
                return false;
            if (JobTitle != other.JobTitle)
                return false;
            if (LoginId != other.LoginId)
                return false;
            if (MaritalStatus != other.MaritalStatus)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (NationalIdNumber != other.NationalIdNumber)
                return false;
            if (OrganizationLevel != other.OrganizationLevel)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalariedFlag != other.SalariedFlag)
                return false;
            if (SickLeaveHours != other.SickLeaveHours)
                return false;
            if (VacationHours != other.VacationHours)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BirthDate == default(DateTime) ? 0 : BirthDate.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (CurrentFlag == default(bool) ? 0 : CurrentFlag.GetHashCode());
                hash = hash * 23 + (Gender == null ? 0 : Gender.GetHashCode());
                hash = hash * 23 + (HireDate == default(DateTime) ? 0 : HireDate.GetHashCode());
                hash = hash * 23 + (JobTitle == null ? 0 : JobTitle.GetHashCode());
                hash = hash * 23 + (LoginId == null ? 0 : LoginId.GetHashCode());
                hash = hash * 23 + (MaritalStatus == null ? 0 : MaritalStatus.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (NationalIdNumber == null ? 0 : NationalIdNumber.GetHashCode());
                hash = hash * 23 + (OrganizationLevel == null ? 0 : OrganizationLevel.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalariedFlag == default(bool) ? 0 : SalariedFlag.GetHashCode());
                hash = hash * 23 + (SickLeaveHours == default(short) ? 0 : SickLeaveHours.GetHashCode());
                hash = hash * 23 + (VacationHours == default(short) ? 0 : VacationHours.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Employee left, Employee right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Employee left, Employee right)
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
