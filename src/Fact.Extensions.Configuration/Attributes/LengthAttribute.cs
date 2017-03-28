using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAttribute : Attribute //ValidatorAttribute, DAL.IDbAttribute
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public LengthAttribute()
        {
            Max = int.MaxValue;
        }

        public LengthAttribute(int max)
        {
            this.Max = max;
        }

        public LengthAttribute(int min, int max)
        {
            this.Min = min;
            this.Max = max;
        }

        /* Leftovers from Apprentice.  Really this never totally belonged in here anyway...
         * should instead be mapped
         * 
        // TODO: place in support for byte arrays/blobs
        public override void Validate()
        {
            string value = (string)Value;

            // required validator checks for this
            if (string.IsNullOrEmpty(value)) return;

            if (value.Length < Min)
                Error(ErrorMessages.Validation_StringLength_Min + Min);

            if (value.Length > Max)
                Error(ErrorMessages.Validation_StringLength_Max + Max);
        }

        void DAL.IDbAttribute.AttributeExtractor(DAL.PropertyAttributes pa)
        {
            // pa.MaxLength starts at maxlength default of 255 already, so leave this alone if
            // MaxValue is set, as MaxValue designates "no value" has been set
            if (Max != int.MaxValue)
                pa.MaxLength = Max;
        }*/
    }
}
