using ImmunizNation.Client.Models;
using ImmunizNation.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.ViewModels.KnowledgeTest
{
    public class KnowledgetTestResultsViewModel
    {
        public double MinResult { get; set; } = 0.7;

        public double Result { get; set; }

        public AccountTypes AccountType { get; set; }

        public string ResultText
        {
            get { return KnowledgeTestUtilities.GetScorePercentage(Result); }
        }

        public bool Passed
        {
            get
            {
                return Result >= MinResult;
            }
        }

        public KnowledgetTestResultsViewModel()
        {

        }

        public KnowledgetTestResultsViewModel(double result)
        {
            Result = result;
        }
    }
}
