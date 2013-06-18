using System;
using NHibernate.Cfg;

namespace ConsoleApplication1
{
	public sealed class CustomNamingStrategy : INamingStrategy
	{
		#region Private readonly fields
		private readonly INamingStrategy namingStrategy;
		#endregion

		#region Public constructor
		public CustomNamingStrategy(INamingStrategy namingStrategy)
		{
			this.namingStrategy = namingStrategy ?? DefaultNamingStrategy.Instance;
		}

		public CustomNamingStrategy() : this(DefaultNamingStrategy.Instance)
		{
		}
		#endregion

		#region Public events
		public event Func<String, String> ClassToTableName;
		public event Func<String, String> ColumnName;
		public event Func<String, String, String> LogicalColumnName;
		public event Func<String, String> PropertyToColumnName;
		public event Func<String, String, String> PropertyToTableName;
		public event Func<String, String> TableName;
		#endregion

		#region INamingStrategy Members

		String INamingStrategy.ClassToTableName(String className)
		{
			if (this.ClassToTableName != null)
			{
				return (this.ClassToTableName(className));
			}
			else
			{
				return (this.namingStrategy.ClassToTableName(className));
			}
		}

		String INamingStrategy.ColumnName(String columnName)
		{
			if (this.ColumnName != null)
			{
				return (this.ColumnName(columnName));
			}
			else
			{
				return (this.namingStrategy.ColumnName(columnName));
			}
		}

		String INamingStrategy.LogicalColumnName(String columnName, String propertyName)
		{
			if (this.LogicalColumnName != null)
			{
				return (this.LogicalColumnName(columnName, propertyName));
			}
			else
			{
				return (this.namingStrategy.LogicalColumnName(columnName, propertyName));
			}
		}

		String INamingStrategy.PropertyToColumnName(String propertyName)
		{
			if (this.PropertyToColumnName != null)
			{
				return (this.PropertyToColumnName(propertyName));
			}
			else
			{
				return (this.namingStrategy.PropertyToColumnName(propertyName));
			}
		}

		String INamingStrategy.PropertyToTableName(String className, String propertyName)
		{
			if (this.PropertyToTableName != null)
			{
				return (this.PropertyToTableName(className, propertyName));
			}
			else
			{
				return (this.namingStrategy.PropertyToTableName(className, propertyName));
			}
		}

		String INamingStrategy.TableName(String tableName)
		{
			if (this.TableName != null)
			{
				return (this.TableName(tableName));
			}
			else
			{
				return (this.namingStrategy.TableName(tableName));
			}
		}

		#endregion
	}
}
