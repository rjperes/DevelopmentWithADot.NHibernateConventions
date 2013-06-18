using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;

namespace ConsoleApplication1
{
	public class ManyToManyConventionModelMapper : ConventionModelMapper
	{
		public ManyToManyConventionModelMapper()
		{
			base.IsOneToMany((MemberInfo member, Boolean isLikely) =>
			{
				return (this.IsOneToMany(member, isLikely));
			});

			base.IsManyToMany((MemberInfo member, Boolean isLikely) =>
			{
				return (this.IsManyToMany(member, isLikely));
			});

			base.BeforeMapManyToMany += this.BeforeMapManyToMany;
			base.BeforeMapSet += this.BeforeMapSet;
		}

		protected virtual Boolean IsManyToMany(MemberInfo member, Boolean isLikely)
		{
			//a relation is many to many if it isn't one to many
			Boolean isOneToMany = this.ModelInspector.IsOneToMany(member);
			return (!isOneToMany);
		}

		protected virtual Boolean IsOneToMany(MemberInfo member, Boolean isLikely)
		{
			Type sourceType = member.DeclaringType;
			Type destinationType = member.GetMemberFromDeclaringType().GetPropertyOrFieldType();

			//check if the property is of a generic collection type
			if ((destinationType.IsGenericCollection() == true) && (destinationType.GetGenericArguments().Length == 1))
			{
				Type destinationEntityType = destinationType.GetGenericArguments().Single();

				//check if the type of the generic collection property is an entity
				if (this.ModelInspector.IsEntity(destinationEntityType) == true)
				{
					//check if there is an equivalent property on the target type that is also a generic collection and points to this entity
					PropertyInfo collectionInDestinationType = destinationEntityType.GetProperties().Where(x => (x.PropertyType.IsGenericCollection() == true) && (x.PropertyType.GetGenericArguments().Length == 1) && (x.PropertyType.GetGenericArguments().Single() == sourceType)).SingleOrDefault();

					if (collectionInDestinationType != null)
					{
						return (false);
					}
				}
			}

			return (true);
		}

		protected virtual new void BeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
		{
			Type destinationEntityType = member.LocalMember.GetPropertyOrFieldType().GetGenericArguments().First();
			//set the mapping table column names from each source entity name plus the _Id sufix
			collectionRelationManyToManyCustomizer.Column(destinationEntityType.Name + "_Id");
		}

		protected virtual new void BeforeMapSet(IModelInspector modelInspector, PropertyPath member, ISetPropertiesMapper propertyCustomizer)
		{
			if (modelInspector.IsManyToMany(member.LocalMember) == true)
			{
				propertyCustomizer.Key(x => x.Column(member.LocalMember.DeclaringType.Name + "_Id"));

				Type sourceType = member.LocalMember.DeclaringType;
				Type destinationType = member.LocalMember.GetPropertyOrFieldType().GetGenericArguments().First();
				String [] names = new Type[] { sourceType, destinationType }.Select(x => x.Name).OrderBy(x => x).ToArray();

				//set inverse on the relation of the alphabetically first entity name
				propertyCustomizer.Inverse(sourceType.Name == names.First());
				//set mapping table name from the entity names in alphabetical order
				propertyCustomizer.Table(String.Join("_", names));
			}
		}
	}
}
