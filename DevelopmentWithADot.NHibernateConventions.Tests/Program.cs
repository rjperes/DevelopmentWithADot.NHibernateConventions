using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace DevelopmentWithADot.NHibernateConventions.Tests
{
	class Program
	{
		static void ExplicitMapping(ConventionModelMapper mapper)
		{
			mapper.Class<Master>(x =>
			{
				x.Id(y => y.Id, y => y.Generator(Generators.HighLow));
				x.Set(y => y.Options, y =>
				{
					y.Key(z => z.Column("master_id"));
					y.Table("master_option");
				}, y =>
				{
					y.ManyToMany(z =>
					{
						z.Column("option_id");
					});
				});
			});

			mapper.Class<Option>(x =>
			{
				x.Id(y => y.Id, y => y.Generator(Generators.HighLow));
				x.Set(y => y.Masters, y =>
				{
					y.Key(z => z.Column("option_id"));
					y.Table("master_option");
					y.Inverse(true);
				}, y =>
				{
					y.ManyToMany(z =>
					{
						z.Column("master_id");
					});
				});
			});
		}

		static void ConventionsMapping(ConventionModelMapper mapper)
		{
			mapper.IsOneToMany((MemberInfo member, Boolean isLikely) =>
			{
				Type sourceType = member.DeclaringType;
				Type destinationType = member.GetMemberFromDeclaringType().GetPropertyOrFieldType();

				//check if the property is of a generic collection type
				if ((destinationType.IsGenericCollection() == true) && (destinationType.GetGenericArguments().Length == 1))
				{
					Type destinationEntityType = destinationType.GetGenericArguments().Single();

					//check if the type of the generic collection property is an entity
					if (mapper.ModelInspector.IsEntity(destinationEntityType) == true)
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
			});

			mapper.IsManyToMany((MemberInfo member, Boolean isLikely) =>
			{
				//a relation is many to many if it isn't one to many
				Boolean isOneToMany = mapper.ModelInspector.IsOneToMany(member);
				return (!isOneToMany);
			});

			mapper.BeforeMapClass += (IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer) =>
			{
				classCustomizer.Id(x =>
				{
					//set the hilo generator
					x.Generator(Generators.HighLow);
				});
			};

			mapper.BeforeMapManyToMany += (IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer) =>
			{
				Type destinationEntityType = member.LocalMember.GetPropertyOrFieldType().GetGenericArguments().First();
				//set the mapping table column names from each source entity name plus the _Id sufix
				collectionRelationManyToManyCustomizer.Column(destinationEntityType.Name + "_Id");
			};

			mapper.BeforeMapSet += (IModelInspector modelInspector, PropertyPath member, ISetPropertiesMapper propertyCustomizer) =>
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
			};
		}

		static void Main(String[] args)
		{
			Configuration cfg = new Configuration()
			.DataBaseIntegration(x =>
			{
				x.LogSqlInConsole = true;
				x.LogFormattedSql = true;
				x.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
				x.SchemaAction = SchemaAutoAction.Update;
				x.ConnectionString = @"Data Source=.\sqlexpress;Integrated Security=SSPI;Initial Catalog=NHibernate";
				x.Dialect<MsSql2008Dialect>();
				x.Driver<Sql2008ClientDriver>();
			});

			ConventionModelMapper mapper = new ConventionModelMapper();

			//conventions mapper
			ConventionsMapping(mapper);

			//explicit mapping
			//ExplicitMapping(mapper);

			IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetExportedTypes();
			HbmMapping mapping = mapper.CompileMappingFor(types);

			cfg.AddMapping(mapping);

			using (ISessionFactory sessionFactory = cfg.BuildSessionFactory())
			{
				using (ISession session = sessionFactory.OpenSession())
				{
					var m = new Master { Name = "master" };
					var o1 = new Option { Name = "option 1" };
					var o2 = new Option { Name = "option 2" };

					session.Save(m);
					session.Save(o1);
					session.Save(o2);

					m.Options.Add(o1);
					m.Options.Add(o2);
					o1.Masters.Add(m);
					o2.Masters.Add(m);

					session.Flush();

					var ms = session.Query<Master>().ToList();
				}
			}
		}
	}
}
