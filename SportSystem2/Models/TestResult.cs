using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class TestResult
    {
        [Key]
        public int TestResultId { get; set; }

        [ForeignKey(nameof(Judge))]
        public int JudgeId { get; set; }
        [ValidateNever]
        public Judge Judge { get; set; } = null!;

        [ForeignKey(nameof(Test))]
        public int TestId { get; set; }
        [ValidateNever]
        public Test Test { get; set; } = null!;

        public double? Score { get; set; }

        public DateTime DateTaken { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Explanation { get; set; }
    }
}
