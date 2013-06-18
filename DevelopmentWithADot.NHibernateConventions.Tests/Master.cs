using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentWithADot.NHibernateConventions.Tests
{
	public class Master
	{
		public Master()
		{
			this.Options = new Iesi.Collections.Generic.HashedSet<Option>();
		}

		public virtual Int32 Id
		{
			get;
			protected set;
		}

		public virtual String Name
		{
			get;
			set;
		}

		public virtual Iesi.Collections.Generic.ISet<Option> Options
		{
			get;
			protected set;
		}
	}
}
