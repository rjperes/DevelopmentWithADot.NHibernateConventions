using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentWithADot.NHibernateConventions.Tests
{
	public class Option
	{
		public Option()
		{
			this.Masters = new Iesi.Collections.Generic.HashedSet<Master>();
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

		public virtual Iesi.Collections.Generic.ISet<Master> Masters
		{
			get;
			protected set;
		}
	}
}
