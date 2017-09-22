using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotCoinShared
{

    public class Normalizer
    {

        private decimal min { get; set; }
        private decimal range { get; set; }

        public Normalizer(IEnumerable<decimal> numbers, decimal margin = 0.005m)
        {

            var max = numbers.Max();
            min = numbers.Min();
            min *= 1 - margin;
            max *= 1 + margin;

            //max = (decimal)Math.Log((double)max);
            //min = (decimal)Math.Log((double)min);

            range = max - min;
            if (range == 0) range = 1;

        }

        public decimal Normalize(decimal number)
        {
            //return ((((decimal)Math.Log((double)number) - min) / range) * 2) - 1;
            return ((((decimal)((double)number) - min) / range) * 2) - 1;
        }

        public IEnumerable<decimal> Normalize(IEnumerable<decimal> numbers) => numbers.Select(Normalize);

        public decimal Denormalize(decimal number)
        {
            //return (decimal)Math.Exp((double)((((number + 1) / 2) * range) + min));
            return (decimal)((double)((((number + 1) / 2) * range) + min));
        }

        public IEnumerable<decimal> Denormalize(IEnumerable<decimal> numbers) => numbers.Select(Denormalize);

    }

}
