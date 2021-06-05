using Jaxx.VideoDb.Data.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    public class DiskId
    {        
        public DiskId(string value)
        {
            Value = value;
            if (!IsValid) throw new FormatException($"Invalid diskid {Value}");
            SplitDiskId();
        }

        private void SplitDiskId()
        {
            Regex regex = new Regex(@"(R)(\d{2})(F)([1-8])(D)(\d{2})");
            var match = regex.Match(Value);
            Shelter = match.Groups[1].Value + match.Groups[2].Value;
            Compartment = match.Groups[3].Value + match.Groups[4].Value;
            ShelterAndCompartment = Shelter + Compartment;
            CompartmentPosition = match.Groups[5].Value + match.Groups[6].Value;
            CompartmentPositionIntValue = int.Parse(match.Groups[6].Value);
        }

        public string Value { get; private set; }
        public string Shelter { get; private set; }
        public string Compartment { get; private set; }
        public string ShelterAndCompartment { get; private set; }
        public string CompartmentPosition { get; private set; }
        public int CompartmentPositionIntValue { get; private set; }
        public bool IsValid
        {
            get
            {
                return DiskIdValidation.IsValidDiskID(Value);
            }
        }
    }
}
