using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ochs;

namespace OchsTest
{
    [TestClass]
    public class TestSwissPhaseHandler
    {
        private readonly SwissPhaseHandler _swissPhaseHandler;

        public TestSwissPhaseHandler()
        {
            _swissPhaseHandler = new SwissPhaseHandler();
        }
        [TestMethod]
        public void TestGenerate2Rounds4Persons()
        {
            var fighters = new List<Person>
                {new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}, new Person{Id = Guid.NewGuid()}};
            Assert.AreEqual(4, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(2, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, matchesRound1);
            Assert.AreEqual(2, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, matchesRound1);
            var allMatches = matchesRound1.ToList();
            allMatches.AddRange(matchesRound2);
            Assert.AreEqual(4, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x!= match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
        }

        [TestMethod]
        public void TestGenerate3Rounds5Persons()
        {
            var fighters = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FullName = "1" }, new Person { Id = Guid.NewGuid(), FullName = "2" },
                new Person { Id = Guid.NewGuid(), FullName = "3" }, new Person { Id = Guid.NewGuid(), FullName = "4" },
                new Person { Id = Guid.NewGuid(), FullName = "5" }
            };
            Assert.AreEqual(5, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(2, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            var allMatches = matchesRound1.ToList();
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(2, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, allMatches);
            allMatches.AddRange(matchesRound2);
            var matchesRound3 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(2, matchesRound3.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound3, fighters, allMatches);
            allMatches.AddRange(matchesRound3);

            Assert.AreEqual(6, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x != match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
            foreach (var fighter in fighters)
            {
                Assert.IsTrue(allMatches.Count(x => x.FighterBlue == fighter || x.FighterRed == fighter) >= 2);
            }
        }

        [TestMethod]
        public void TestGenerate3Rounds6Persons()
        {
            var fighters = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FullName = "1" }, new Person { Id = Guid.NewGuid(), FullName = "2" },
                new Person { Id = Guid.NewGuid(), FullName = "3" }, new Person { Id = Guid.NewGuid(), FullName = "4" },
                new Person { Id = Guid.NewGuid(), FullName = "5" }, new Person { Id = Guid.NewGuid(), FullName = "6" }
            };
            Assert.AreEqual(6, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(3, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            var allMatches = matchesRound1.ToList();
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(3, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, allMatches);
            allMatches.AddRange(matchesRound2);
            var matchesRound3 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(3, matchesRound3.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound3, fighters, allMatches);
            allMatches.AddRange(matchesRound3);

            Assert.AreEqual(9, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x != match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
            foreach (var fighter in fighters)
            {
                Assert.IsTrue(allMatches.Count(x => x.FighterBlue == fighter || x.FighterRed == fighter) == 3);
            }
        }

        [TestMethod]
        public void TestGenerate3Rounds7Persons()
        {
            var fighters = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FullName = "1" }, new Person { Id = Guid.NewGuid(), FullName = "2" },
                new Person { Id = Guid.NewGuid(), FullName = "3" }, new Person { Id = Guid.NewGuid(), FullName = "4" },
                new Person { Id = Guid.NewGuid(), FullName = "5" }, new Person { Id = Guid.NewGuid(), FullName = "6" },
                new Person { Id = Guid.NewGuid(), FullName = "7" }
            };
            Assert.AreEqual(7, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(3, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            var allMatches = matchesRound1.ToList();
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(3, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, allMatches);
            allMatches.AddRange(matchesRound2);
            var matchesRound3 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(3, matchesRound3.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound3, fighters, allMatches);
            allMatches.AddRange(matchesRound3);

            Assert.AreEqual(9, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x != match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
            foreach (var fighter in fighters)
            {
                Assert.IsTrue(allMatches.Count(x => x.FighterBlue == fighter || x.FighterRed == fighter) >= 2);
            }
        }

        [TestMethod]
        public void TestGenerate4Rounds8Persons()
        {
            var fighters = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FullName = "1" }, new Person { Id = Guid.NewGuid(), FullName = "2" },
                new Person { Id = Guid.NewGuid(), FullName = "3" }, new Person { Id = Guid.NewGuid(), FullName = "4" },
                new Person { Id = Guid.NewGuid(), FullName = "5" }, new Person { Id = Guid.NewGuid(), FullName = "6" },
                new Person { Id = Guid.NewGuid(), FullName = "7" }, new Person { Id = Guid.NewGuid(), FullName = "8" }
            };
            Assert.AreEqual(8, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(4, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            var allMatches = matchesRound1.ToList();
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, allMatches);
            allMatches.AddRange(matchesRound2);
            var matchesRound3 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound3.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound3, fighters, allMatches);
            allMatches.AddRange(matchesRound3);
            var matchesRound4 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound4.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound4, fighters, allMatches);
            allMatches.AddRange(matchesRound4);

            Assert.AreEqual(16, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x != match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
            foreach (var fighter in fighters)
            {
                Assert.IsTrue(allMatches.Count(x => x.FighterBlue == fighter || x.FighterRed == fighter) == 4);
            }
        }

        [TestMethod]
        public void TestGenerate4Rounds9Persons()
        {
            var fighters = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FullName = "1" }, new Person { Id = Guid.NewGuid(), FullName = "2" },
                new Person { Id = Guid.NewGuid(), FullName = "3" }, new Person { Id = Guid.NewGuid(), FullName = "4" },
                new Person { Id = Guid.NewGuid(), FullName = "5" }, new Person { Id = Guid.NewGuid(), FullName = "6" },
                new Person { Id = Guid.NewGuid(), FullName = "7" }, new Person { Id = Guid.NewGuid(), FullName = "8" },
                new Person { Id = Guid.NewGuid(), FullName = "9" }
            };
            Assert.AreEqual(9, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(4, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            AssignRandomResults(matchesRound1);
            var allMatches = matchesRound1.ToList();
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, allMatches);
            AssignRandomResults(matchesRound2);
            allMatches.AddRange(matchesRound2);
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound3 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound3.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound3, fighters, allMatches);
            AssignRandomResults(matchesRound3);
            allMatches.AddRange(matchesRound3);
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound4 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound4.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound4, fighters, allMatches);
            AssignRandomResults(matchesRound4);
            allMatches.AddRange(matchesRound4);
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));

            Assert.AreEqual(16, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x != match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
            foreach (var fighter in fighters)
            {
                var matchCount = allMatches.Count(x => x.FighterBlue == fighter || x.FighterRed == fighter);
                Assert.IsTrue(matchCount >= 3);
            }
        }

        [TestMethod]
        public void TestGenerate5Rounds9Persons()
        {
            var fighters = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FullName = "1" }, new Person { Id = Guid.NewGuid(), FullName = "2" },
                new Person { Id = Guid.NewGuid(), FullName = "3" }, new Person { Id = Guid.NewGuid(), FullName = "4" },
                new Person { Id = Guid.NewGuid(), FullName = "5" }, new Person { Id = Guid.NewGuid(), FullName = "6" },
                new Person { Id = Guid.NewGuid(), FullName = "7" }, new Person { Id = Guid.NewGuid(), FullName = "8" },
                new Person { Id = Guid.NewGuid(), FullName = "9" }
            };
            Assert.AreEqual(9, fighters.Count);
            var matchesRound1 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, null);
            Assert.AreEqual(4, matchesRound1.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound1, fighters, null);
            AssignRandomResults(matchesRound1);
            var allMatches = matchesRound1.ToList();
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound2 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound2.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound2, fighters, allMatches);
            AssignRandomResults(matchesRound2);
            allMatches.AddRange(matchesRound2);
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound3 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound3.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound3, fighters, allMatches);
            AssignRandomResults(matchesRound3);
            allMatches.AddRange(matchesRound3);
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound4 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound4.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound4, fighters, allMatches);
            AssignRandomResults(matchesRound4);
            allMatches.AddRange(matchesRound4);
            Assert.IsTrue(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));
            var matchesRound5 = _swissPhaseHandler.GenerateMatches(fighters.Count, null, null, allMatches);
            Assert.AreEqual(4, matchesRound5.Count);
            _swissPhaseHandler.AssignFightersToMatches(matchesRound5, fighters, allMatches);
            AssignRandomResults(matchesRound5);
            allMatches.AddRange(matchesRound5);
            Assert.IsFalse(_swissPhaseHandler.AllowedToGenerateMatches(allMatches, fighters.Count, null, null));

            Assert.AreEqual(20, allMatches.Count);
            foreach (var match in allMatches)
            {
                Assert.AreNotEqual(null, match.FighterBlue);
                Assert.AreNotEqual(null, match.FighterRed);
                Assert.IsFalse(allMatches.Any(x => x != match && ((x.FighterBlue == match.FighterBlue && x.FighterRed == match.FighterRed) || (x.FighterRed == match.FighterBlue && x.FighterBlue == match.FighterRed))));
            }
            foreach (var fighter in fighters)
            {
                var matchCount = allMatches.Count(x => x.FighterBlue == fighter || x.FighterRed == fighter);
                Assert.IsTrue(matchCount >= 4);
            }
        }

        private void AssignRandomResults(IList<Match> matches)
        {
            var random = new Random();
            foreach (var match in matches)
            {
                match.StartedDateTime = DateTime.Now;
                match.Result = (MatchResult)(random.Next(5) + 1);
                match.FinishedDateTime = DateTime.Now;
            }
        }
    }
}