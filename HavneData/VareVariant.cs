using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public class VareVariant
    {
        public enum Length
        {
            Undefined,
            Small,
            Medium,
            Big
        }

        public Length Size { get; set; }

        public static VareVariant Create(double length)
        {
            if (length < 7.3)
            {
                return new VareVariant { Size = Length.Small };
            }

            if (length < 9.2)
            {
                return new VareVariant { Size = Length.Medium };
            }

            return new VareVariant { Size = Length.Big };
        }

        public static VareVariant Create(string variantText)
        {
            switch (variantText)
            {
                case "> 7,2 m båtlengde":
                    return new VareVariant { Size = Length.Small };
                case "7,3m-9,1m båtlengde":
                    return new VareVariant { Size = Length.Medium };
                case "9,2 < båtlengde":
                    return new VareVariant { Size = Length.Big };
                default:
                    return new VareVariant { Size = Length.Undefined };
            }
        }
    }
}
