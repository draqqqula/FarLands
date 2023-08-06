using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace VideoGame.Construct.Families
{
    public class Entity : Family
    {
        private static ImmutableDictionary<Team, List<Sprite>> CreateTeamList() => Enum.GetValues(typeof(Team))
            .As<IEnumerable<Team>>()
            .ToDictionary(it => it, it => new List<Sprite>())
            .ToImmutableDictionary();

        private readonly ImmutableDictionary<Team, List<Sprite>> Teams
            = CreateTeamList();

        private readonly ImmutableDictionary<Team, List<Sprite>> Enemies
            = CreateTeamList();


        private Dictionary<Sprite, Team> ObjectTeamPairs = new Dictionary<Sprite, Team>();

        public Sprite[] GetComposition(Team team)
        {
            return Teams[team].ToArray();
        }

        public IEnumerable<Sprite> GetFoes(Team team)
        {
            return Enemies[team];
        }

        public IEnumerable<Sprite> GetFoes(Sprite member)
        {
            return GetFoes(ObjectTeamPairs[member]);
        }

        public Team GetTeam(Sprite member)
        {
            Team team;
            if (ObjectTeamPairs.TryGetValue(member, out team))
            {
                return team;
            }
            return Team.empty;
        }

        public override void OnReplenishment(Sprite member)
        {
            var dummy = member.GetBehavior<Dummy>();
            Teams[dummy.Team].Add(member);

            foreach (var enemyTeam in Enemies)
            {
                if (enemyTeam.Key != dummy.Team)
                    enemyTeam.Value.Add(member);
            }

            ObjectTeamPairs.Add(member, dummy.Team);
        }

        public override void OnAbandonment(Sprite member)
        {
            foreach (var enemyTeam in Enemies)
            {
                if (enemyTeam.Key != ObjectTeamPairs[member])
                    enemyTeam.Value.Add(member);
            }

            Teams[ObjectTeamPairs[member]].Remove(member);
            ObjectTeamPairs.Remove(member);
        }
    }
}
