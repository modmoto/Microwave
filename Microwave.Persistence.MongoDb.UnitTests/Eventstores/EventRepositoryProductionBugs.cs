using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Eventstores.Persistence.MongoDb;

namespace Microwave.Persistence.MongoDb.UnitTests.Eventstores
{
    [TestClass]
    public class EventRepositoryProductionBugs : IntegrationTests
    {
        [TestMethod]
        public async Task AddEvents_PlayerConfigSeedBug()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<PlayerConfigCreated>();
            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));

            await eventRepository.AppendAsync(DomainEventsInSeed, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(PlayerConfigCreated).Name);

            Assert.AreEqual(3, result.Value.Count());
        }

        private static IEnumerable<IDomainEvent> DomainEventsInSeed => new List<IDomainEvent>
        {
            new PlayerConfigCreated(StringIdentity.Create("DE_LineMan"),
                new List<StringIdentity>(),
                new List<SkillType>
                {
                    SkillType.General
                },
                new List<SkillType>
                {
                    SkillType.General
                }
            ),
            new PlayerConfigCreated(StringIdentity.Create("DE_Blitzer"),
                new List<StringIdentity>
                {
                    Skills.Block
                },
                new List<SkillType>
                {
                    SkillType.General
                },
                new List<SkillType>
                {
                    SkillType.General
                }
            ),
            new PlayerConfigCreated(StringIdentity.Create("DE_WitchElve"),
                new List<StringIdentity>
                {
                    Skills.Dodge
                },
                new List<SkillType>
                {
                    SkillType.General
                },
                new List<SkillType>
                {
                    SkillType.General
                }
            )
        };

    }

    internal static class Skills
    {
        public static StringIdentity Block => StringIdentity.Create("Block");
        public static StringIdentity Dodge => StringIdentity.Create("Dodge");
    }

    public class PlayerConfigCreated : IDomainEvent
    {
        public Identity EntityId { get; }
        public IEnumerable<StringIdentity> StartingSkills { get; }
        public IEnumerable<SkillType> SkillsOnDefault { get; }
        public IEnumerable<SkillType> SkillsOnDouble { get; }

        public PlayerConfigCreated(
            StringIdentity entityId,
            IEnumerable<StringIdentity> startingSkills,
            IEnumerable<SkillType> skillsOnDefault,
            IEnumerable<SkillType> skillsOnDouble)
        {
            EntityId = entityId;
            StartingSkills = startingSkills;
            SkillsOnDefault = skillsOnDefault;
            SkillsOnDouble = skillsOnDouble;
        }


    }
    public enum SkillType
    {
        General, Agility, Strength, Passing, Mutation, Extraordinary,
        PlusOneAgility,
        PlusOneArmorOrMovement,
        PlusOneStrength
    }
}