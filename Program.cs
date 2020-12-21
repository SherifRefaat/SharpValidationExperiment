using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace SharpValidationExperiment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ValidatorFactory>();
        }
    }

    public class ValidatorFactory
    {
        Model
            m1 = null,
            m2 = new Model { Numbers = new List<int>(0) },
            m3 = new Model { Numbers = new List<int>(0), Name = "" },
            m4 = new Model { Numbers = new List<int>(0), Name = "", Dob = new DateTime(2001, 1, 1) };


        [Benchmark]
        public void ValidateUsingCSharpValidator()
        {

            CSharpPatternValidator.Validate(m1);
            CSharpPatternValidator.Validate(m2);
            CSharpPatternValidator.Validate(m3);
            CSharpPatternValidator.Validate(m4);
        }

        [Benchmark]
        public void ValidateUsingFluentValidator()
        {
            var v = new FluentValidator();
            //v.Validate(m1); // Not applicable here, will throw exception.
            v.Validate(m2);
            v.Validate(m3);
            v.Validate(m4);
        }

        [Benchmark]
        public void ValidateUsingBareValidator()
        {
            BareValidator.Validate(m1);
            BareValidator.Validate(m2);
            BareValidator.Validate(m3);
            BareValidator.Validate(m4);
        }
    }

    public static class CSharpPatternValidator
    {
        public static List<string> Validate(Model m)
        {
            var errors = new List<string>();

            if (m is null)
            {
                errors.Add("Null model.");
                return errors;
            }

            if (m is { Numbers: null or { Count: 0 } })
                errors.Add("Numbers are empty.");

            if (m.Name is null or { Length: 0 })
                errors.Add("Name is empty.");

            //if (m.Dob == default || m.Dob is { Day: 1, Month: 1, Year: 1 } /*or { Year :>= DateTime.Now.Year }*/ or { Year :<= 2000 }) // Commented expression is not possible.
            if (m.Dob == default || m.Dob is { Day: 1, Month: 1, Year: 1 } or { Year :<= 2000 } || m.Dob.Year >= DateTime.Now.Year)
                errors.Add("Dob is invalid.");

            return errors;
        }
    }

    class FluentValidator : AbstractValidator<Model>
    {
        public FluentValidator()
        {
            RuleFor(m => m)
                .Null()
                .WithMessage("Null model.");

            RuleFor(m => m.Numbers)
                .Null().WithMessage("Numbers are empty.")
                .Empty().WithMessage("Numbers are empty.");

            RuleFor(m => m.Name)
                .Null().WithMessage("Name is empty.")
                .Empty().WithMessage("Name is empty.");

            RuleFor(m => m.Dob)
                .Empty().WithMessage("Date is invalid.")
                .Must(m => 
                {
                    bool result = true;

                    if (m.Day == 1 || m.Month == 1 || m.Year == 1)
                        result = false;

                    if (m.Year >= DateTime.Now.Year)
                        result = false;

                    if (m.Year <= 2000)
                        result = false;

                    return result;
                }).WithMessage("Date is invalid.");
        }
    }

    public static class BareValidator
    {
        public static List<string> Validate(Model m)
        {
            var errors = new List<string>();

            if (m == null)
            { 
                errors.Add("Null model.");
                return errors;
            }

            if (m.Numbers == null || m.Numbers.Count == 0)
                errors.Add("Numbers are empty.");

            if (m.Name == null || m.Name.Length == 0)
                errors.Add("Name is empty.");

            if (m.Dob == default)
                errors.Add("Dob is invalid.");

            if (m.Dob.Day == 1 || m.Dob.Month == 1 || m.Dob.Year == 1)
                errors.Add("Dob is invalid.");

            if (m.Dob.Year >= DateTime.Now.Year || m.Dob.Year <= 2000)
                errors.Add("Dob is invalid.");

            return errors;
        }
    }

    public class Model
    {
        public int Age { get; set; }

        public DateTime Dob { get; set; }

        public string Name { get; set; }

        public List<int> Numbers { get; set; }
    }
}
