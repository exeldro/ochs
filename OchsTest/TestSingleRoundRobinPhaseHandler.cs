using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ochs;

namespace OchsTest
{
    [TestClass]
    public class TestSingleRoundRobinPhaseHandler
    {
        private SingleRoundRobinPhaseHandler _singleRoundRobinPhaseHandler;

        public TestSingleRoundRobinPhaseHandler()
        {
            _singleRoundRobinPhaseHandler = new SingleRoundRobinPhaseHandler();
        }
        [TestMethod]
        public void TestGeneratePoolsOf4()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(4, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(6, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);

            foreach (var match in matches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
            }
        }

        [TestMethod]
        public void TestGeneratePoolsOf5()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(5, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(10, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);
            AssertFighterTwiceInARow(matches);
            AssertFightersMatchUpOnce(matches);
        }

        [TestMethod]
        public void TestGeneratePoolsOf6()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(6, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(15, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);
            AssertFighterTwiceInARow(matches);
            AssertFightersMatchUpOnce(matches);
        }

        [TestMethod]
        public void TestGeneratePoolsOf7()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(7, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(21, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);
            AssertFighterTwiceInARow(matches);
            AssertFightersMatchUpOnce(matches);
        }

        [TestMethod]
        public void TestGeneratePoolsOf8()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(8, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(28, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);
            AssertFighterTwiceInARow(matches);
            AssertFightersMatchUpOnce(matches);
        }

        [TestMethod]
        public void TestGeneratePoolsOf9()
        {
            var fighters = Enumerable.Range(0, 9).Select(x => new Person{Id = Guid.NewGuid()}).ToList();
            Assert.AreEqual(9, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(36, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);
            AssertFighterTwiceInARow(matches);
            AssertFightersMatchUpOnce(matches);
        }

        [TestMethod]
        public void TestGeneratePoolsOf10()
        {
            var fighters = Enumerable.Range(0, 10).Select(x => new Person{Id = Guid.NewGuid()}).ToList();
            Assert.AreEqual(10, fighters.Count);
            var matches = _singleRoundRobinPhaseHandler.GenerateMatches(fighters.Count, null);
            Assert.AreEqual(45, matches.Count);
            _singleRoundRobinPhaseHandler.AssignFightersToMatches(matches,fighters);
            AssertFighterTwiceInARow(matches);
            AssertFightersMatchUpOnce(matches);
        }

        private void AssertFightersMatchUpOnce(IList<Match> matches)
        {
            Assert.IsFalse(matches.Any(x => matches.Any(y =>
                y != x && ((x.FighterBlue == y.FighterBlue && x.FighterRed == y.FighterRed) ||
                           (x.FighterRed == y.FighterBlue && x.FighterBlue == y.FighterRed)))));
        }

        private static void AssertFighterTwiceInARow(IList<Match> matches)
        {
            Person blue = null;
            Person red = null;
            foreach (var match in matches)
            {
                Assert.AreNotEqual(null, match.FighterBlue, match.Name + " blue is null");
                Assert.AreNotEqual(null, match.FighterRed, match.Name + " red is null");
                Assert.AreNotSame(match.FighterBlue, blue, match.Name + " blue is same fighter as previous fight blue");
                Assert.AreNotSame(match.FighterBlue, red, match.Name + " blue is same fighter as previous fight red");
                Assert.AreNotSame(match.FighterRed, blue, match.Name + " red is same fighter as previous fight blue");
                Assert.AreNotSame(match.FighterRed, red, match.Name + " red is same fighter as previous fight red");
                blue = match.FighterBlue;
                red = match.FighterRed;
            }
        }
    }
}
