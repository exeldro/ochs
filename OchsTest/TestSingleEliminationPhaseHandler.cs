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
            var fighters = Enumerable.Range(0, 8).Select(x => new Person{Id = Guid.NewGuid()}).ToList();
            Assert.AreEqual(8, fighters.Count);
            var matches = _singleEliminationPhaseHandler.GenerateMatches(fighters.Count, null, null);
            Assert.AreEqual(8, matches.Count);
            _singleEliminationPhaseHandler.AssignFightersToMatches(matches,fighters);
            var matchesPart = matches.Take(4).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreNotEqual(null, match.FighterBlue, match.Name + " blue is null");
                Assert.AreNotEqual(null, match.FighterRed, match.Name + " red is null");
                var indexBlue = fighters.IndexOf(match.FighterBlue);
                var indexRed = fighters.IndexOf(match.FighterRed);
                Assert.AreEqual(fighters.Count-1, indexBlue+indexRed, match.Name + " is not good pair");
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

            Assert.AreEqual(2, _singleEliminationPhaseHandler.GetRank(matches[4].FighterBlue, matches));
            Assert.AreEqual(4, _singleEliminationPhaseHandler.GetRank(matches[4].FighterRed, matches));
            Assert.AreEqual(5, _singleEliminationPhaseHandler.GetRank(matches[0].FighterBlue, matches));
            Assert.AreEqual(5, _singleEliminationPhaseHandler.GetRank(matches[1].FighterRed, matches));
        }


        [TestMethod]
        public void TestEliminationOf16()
        {
            var fighters = Enumerable.Range(0, 16).Select(x => new Person{Id = Guid.NewGuid()}).ToList();
            Assert.AreEqual(16, fighters.Count);
            var matches = _singleEliminationPhaseHandler.GenerateMatches(fighters.Count, null, null);
            Assert.AreEqual(16, matches.Count);
            _singleEliminationPhaseHandler.AssignFightersToMatches(matches,fighters);
            var matchesPart = matches.Take(8).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreNotEqual(null, match.FighterBlue, match.Name + " blue is null");
                Assert.AreNotEqual(null, match.FighterRed, match.Name + " red is null");
                var indexBlue = fighters.IndexOf(match.FighterBlue);
                var indexRed = fighters.IndexOf(match.FighterRed);
                Assert.AreEqual(fighters.Count-1, indexBlue+indexRed, match.Name + " is not good pair");
            }
            matchesPart = matches.Skip(8).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreEqual(null, match.FighterBlue, match.Name + " blue is not null");
                Assert.AreEqual(null, match.FighterRed, match.Name + " red is not null");
            }
            matches[0].Result = MatchResult.WinRed;
            var changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[0], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[0].FighterRed, matches[8].FighterBlue);
            
            matches[1].Result = MatchResult.WinBlue;
            changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[1], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[1].FighterBlue, matches[8].FighterRed);

            matches[8].Result = MatchResult.WinBlue;
            changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[8], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[8].FighterBlue, matches[12].FighterBlue);

            Assert.AreEqual(9, _singleEliminationPhaseHandler.GetRank(matches[0].FighterBlue, matches));
            Assert.AreEqual(9, _singleEliminationPhaseHandler.GetRank(matches[1].FighterRed, matches));
        }
        
        [TestMethod]
        public void TestEliminationOf32()
        {
            var fighters = Enumerable.Range(0, 32).Select(x => new Person{Id = Guid.NewGuid()}).ToList();
            Assert.AreEqual(32, fighters.Count);
            var matches = _singleEliminationPhaseHandler.GenerateMatches(fighters.Count, null, null);
            Assert.AreEqual(32, matches.Count);
            _singleEliminationPhaseHandler.AssignFightersToMatches(matches,fighters);
            var matchesPart = matches.Take(16).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreNotEqual(null, match.FighterBlue, match.Name + " blue is null");
                Assert.AreNotEqual(null, match.FighterRed, match.Name + " red is null");
                var indexBlue = fighters.IndexOf(match.FighterBlue);
                var indexRed = fighters.IndexOf(match.FighterRed);
                Assert.AreEqual(fighters.Count-1, indexBlue+indexRed, match.Name + " is not good pair");
            }
            matchesPart = matches.Skip(16).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreEqual(null, match.FighterBlue, match.Name + " blue is not null");
                Assert.AreEqual(null, match.FighterRed, match.Name + " red is not null");
            }
            matches[0].Result = MatchResult.WinRed;
            var changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[0], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[0].FighterRed, matches[16].FighterBlue);
            
            matches[1].Result = MatchResult.WinBlue;
            changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[1], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[1].FighterBlue, matches[16].FighterRed);

            matches[16].Result = MatchResult.WinBlue;
            changedMatches = _singleEliminationPhaseHandler.UpdateMatchesAfterFinishedMatch(matches[16], matches);
            Assert.AreEqual(1, changedMatches.Count);
            Assert.AreEqual(matches[16].FighterBlue, matches[24].FighterBlue);

            Assert.AreEqual(17, _singleEliminationPhaseHandler.GetRank(matches[0].FighterBlue, matches));
            Assert.AreEqual(17, _singleEliminationPhaseHandler.GetRank(matches[1].FighterRed, matches));
        }

        [TestMethod]
        public void TestEliminationOf17()
        {
            var fighters = Enumerable.Range(0, 17).Select(x => new Person{Id = Guid.NewGuid()}).ToList();
            Assert.AreEqual(17, fighters.Count);
            var matches = _singleEliminationPhaseHandler.GenerateMatches(fighters.Count, null, null);
            Assert.AreEqual(32, matches.Count);
            _singleEliminationPhaseHandler.AssignFightersToMatches(matches,fighters);
            var matchesPart = matches.Take(16).ToList();
            foreach (var match in matchesPart)
            {
                Assert.AreNotEqual(null, match.FighterBlue?? match.FighterRed, match.Name + " blue en red are null");
                if (match.FighterBlue != null && match.FighterRed != null)
                {
                    var indexBlue = fighters.IndexOf(match.FighterBlue);
                    var indexRed = fighters.IndexOf(match.FighterRed);
                    Assert.AreEqual(31, indexBlue + indexRed, match.Name + " is not good pair");
                }
            }
        }
    }
}
