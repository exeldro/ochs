using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ochs;

namespace OchsTest
{
    [TestClass]
    public class TestSingleEliminationPhaseHandler
    {
        private SingleEliminationPhaseHandler _singleEliminationPhaseHandler;

        public TestSingleEliminationPhaseHandler()
        {
            _singleEliminationPhaseHandler = new SingleEliminationPhaseHandler();
        }

        [TestMethod]
        public void TestEliminationOf8()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(8, fighters.Count);
            var matches = _singleEliminationPhaseHandler.GenerateMatches(fighters.Count, null, null);
            Assert.AreEqual(8, matches.Count);
            _singleEliminationPhaseHandler.AssignFightersToMatches(matches,fighters);
            var matchesPart = matches.Take(4).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreNotEqual(null, match.FighterBlue, match.Name + " blue is null");
                Assert.AreNotEqual(null, match.FighterRed, match.Name + " red is null");
            }
            matchesPart = matches.Skip(4).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreEqual(null, match.FighterBlue, match.Name + " blue is not null");
                Assert.AreEqual(null, match.FighterRed, match.Name + " red is not null");
            }
            matches[0].Result = MatchResult.WinRed;
            var changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[0], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[0].FighterRed, matches[4].FighterBlue);
            
            matches[1].Result = MatchResult.WinBlue;
            changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[1], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[1].FighterBlue, matches[4].FighterRed);

            matches[4].Result = MatchResult.WinBlue;
            changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[4], matches);
            Assert.AreEqual(2, changedMatches.Count);
            Assert.AreEqual(matches[4].FighterBlue, matches[7].FighterBlue);
            Assert.AreEqual(matches[4].FighterRed, matches[6].FighterBlue);
        }
    }
}
