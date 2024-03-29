﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tunney.Common.Statistics
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="Automation_Statistics")]
	public partial class ActionDurationDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertActionDuration(ActionDuration instance);
    partial void UpdateActionDuration(ActionDuration instance);
    partial void DeleteActionDuration(ActionDuration instance);
    partial void InsertAction(Action instance);
    partial void UpdateAction(Action instance);
    partial void DeleteAction(Action instance);
    #endregion
		
		public ActionDurationDataContext() : 
				base(string.Empty, mappingSource) //TODO:  TUNNEY!!!!
		{
			OnCreated();
		}
		
		public ActionDurationDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ActionDurationDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ActionDurationDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ActionDurationDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<ActionDuration> ActionDurations
		{
			get
			{
				return this.GetTable<ActionDuration>();
			}
		}
		
		public System.Data.Linq.Table<Action> Actions
		{
			get
			{
				return this.GetTable<Action>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ActionDuration")]
	public partial class ActionDuration : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _ID;
		
		private System.DateTime _Stamp;
		
		private string _Source;
		
		private long _Duration;
		
		private string _Machine;
		
		private int _ActionID;
		
		private EntityRef<Action> _Action;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(long value);
    partial void OnIDChanged();
    partial void OnStampChanging(System.DateTime value);
    partial void OnStampChanged();
    partial void OnSourceChanging(string value);
    partial void OnSourceChanged();
    partial void OnDurationChanging(long value);
    partial void OnDurationChanged();
    partial void OnMachineChanging(string value);
    partial void OnMachineChanged();
    partial void OnActionIDChanging(int value);
    partial void OnActionIDChanged();
    #endregion
		
		public ActionDuration()
		{
			this._Action = default(EntityRef<Action>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public long ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Stamp", DbType="DateTime NOT NULL")]
		public System.DateTime Stamp
		{
			get
			{
				return this._Stamp;
			}
			set
			{
				if ((this._Stamp != value))
				{
					this.OnStampChanging(value);
					this.SendPropertyChanging();
					this._Stamp = value;
					this.SendPropertyChanged("Stamp");
					this.OnStampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Source", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Source
		{
			get
			{
				return this._Source;
			}
			set
			{
				if ((this._Source != value))
				{
					this.OnSourceChanging(value);
					this.SendPropertyChanging();
					this._Source = value;
					this.SendPropertyChanged("Source");
					this.OnSourceChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Duration", DbType="bigint NOT NULL")]
		public long Duration
		{
			get
			{
				return this._Duration;
			}
			set
			{
				if ((this._Duration != value))
				{
					this.OnDurationChanging(value);
					this.SendPropertyChanging();
					this._Duration = value;
					this.SendPropertyChanged("Duration");
					this.OnDurationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Machine", DbType="NVarChar(255) NOT NULL", CanBeNull=false)]
		public string Machine
		{
			get
			{
				return this._Machine;
			}
			set
			{
				if ((this._Machine != value))
				{
					this.OnMachineChanging(value);
					this.SendPropertyChanging();
					this._Machine = value;
					this.SendPropertyChanged("Machine");
					this.OnMachineChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ActionID", DbType="Int NOT NULL")]
		public int ActionID
		{
			get
			{
				return this._ActionID;
			}
			set
			{
				if ((this._ActionID != value))
				{
					if (this._Action.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnActionIDChanging(value);
					this.SendPropertyChanging();
					this._ActionID = value;
					this.SendPropertyChanged("ActionID");
					this.OnActionIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Action_ActionDuration", Storage="_Action", ThisKey="ActionID", OtherKey="ID", IsForeignKey=true)]
		public Action Action
		{
			get
			{
				return this._Action.Entity;
			}
			set
			{
				Action previousValue = this._Action.Entity;
				if (((previousValue != value) 
							|| (this._Action.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Action.Entity = null;
						previousValue.ActionDurations.Remove(this);
					}
					this._Action.Entity = value;
					if ((value != null))
					{
						value.ActionDurations.Add(this);
						this._ActionID = value.ID;
					}
					else
					{
						this._ActionID = default(int);
					}
					this.SendPropertyChanged("Action");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Actions")]
	public partial class Action : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private string _Action1;
		
		private EntitySet<ActionDuration> _ActionDurations;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnAction1Changing(string value);
    partial void OnAction1Changed();
    #endregion
		
		public Action()
		{
			this._ActionDurations = new EntitySet<ActionDuration>(new Action<ActionDuration>(this.attach_ActionDurations), new Action<ActionDuration>(this.detach_ActionDurations));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="Action", Storage="_Action1", DbType="NVarChar(100) NOT NULL", CanBeNull=false)]
		public string Action1
		{
			get
			{
				return this._Action1;
			}
			set
			{
				if ((this._Action1 != value))
				{
					this.OnAction1Changing(value);
					this.SendPropertyChanging();
					this._Action1 = value;
					this.SendPropertyChanged("Action1");
					this.OnAction1Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Action_ActionDuration", Storage="_ActionDurations", ThisKey="ID", OtherKey="ActionID")]
		public EntitySet<ActionDuration> ActionDurations
		{
			get
			{
				return this._ActionDurations;
			}
			set
			{
				this._ActionDurations.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_ActionDurations(ActionDuration entity)
		{
			this.SendPropertyChanging();
			entity.Action = this;
		}
		
		private void detach_ActionDurations(ActionDuration entity)
		{
			this.SendPropertyChanging();
			entity.Action = null;
		}
	}
}
#pragma warning restore 1591
