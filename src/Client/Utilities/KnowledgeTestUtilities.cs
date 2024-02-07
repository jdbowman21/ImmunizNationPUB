using ImmunizNation.Client.Models;
using System;

namespace ImmunizNation.Client.Utilities
{
    public static class KnowledgeTestUtilities
    {
        public static string GetScorePercentage(double score)
        {
            return Math.Ceiling(score * 100) + "%";
        }
    }
}
