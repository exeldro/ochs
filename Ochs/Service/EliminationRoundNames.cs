using Microsoft.VisualBasic;

namespace Ochs
{
    public static class EliminationRoundNames
    {
        private static readonly string[] roundNames = {"Final","Semifinals","Quarterfinals","Eighth-finals","16th-finals","32nd-finals", "64th-finals"};
        private static readonly string thirdPlaceMatchName = "Third place match";
        public static string GetMatchName(int round, int matchNumber)
        {
            if (round == 0)
            {
                return matchNumber == 2 ? " "+thirdPlaceMatchName  : roundNames[0];
            }
            return Strings.Space(round) + roundNames[round] + " match " + matchNumber.ToString().PadLeft(3);
        }

        public static int GetRound(string matchName)
        {
            if (matchName.Trim() == thirdPlaceMatchName)
                return 0;
            for (var round = 0; round < roundNames.Length; round++)
            {
                if (matchName.TrimStart().StartsWith(roundNames[round]))
                    return round;
            }
            return -1;
        }
        public static int GetMatchNumber(string matchName, int round)
        {
            if (round == 0)
            {
                if (matchName == roundNames[round])
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            return int.Parse(matchName.Replace(roundNames[round] + " match ", ""));

        }
    }
}