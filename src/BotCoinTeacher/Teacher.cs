using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro;
using Accord.Statistics;

namespace BotCoinTeacher
{

    public class Teacher
    {

        private void Log(string message, bool breakLine = true)
        {
            if (breakLine)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now}: {message}");
                Console.WriteLine($"{message}");
            }
            else
            {
                System.Diagnostics.Debug.Write($"{DateTime.Now}: {message}");
                Console.Write($"{message}");
            }
        }

        public void Test()
        {
            
            Log("Start");

            var windowSize = 10;
            var predictionSize = 2;

            var series = Enumerable.Range(0, 2000).Select(e => (decimal)(e % 39 + 1)).ToList();
            
            var normalizer = new BotCoinShared.Normalizer(series);

            var normalizedSeries = normalizer.Normalize(series).ToList();
                        
            var network = new ActivationNetwork(
                new BipolarSigmoidFunction(2.0),
                windowSize,
                windowSize * 2,
                predictionSize
            );
            
            var teacher = new Accord.Neuro.Learning.LevenbergMarquardtLearning(network);

            //teacher.Reset(0.0125);

            var sampleCount = normalizedSeries.Count() - predictionSize - windowSize;

            var input = new double[sampleCount][];
            var output = new double[sampleCount][];

            for(var pos = 0; pos < sampleCount; pos++)
            {
                if (pos > 0 && pos % 10000 == 0) Log($"Loaded {pos} of {sampleCount} samples");
                input[pos] = normalizedSeries.Skip(pos).Take(windowSize).Select(e => (double)e).ToArray();
                output[pos] = normalizedSeries.Skip(pos + windowSize).Take(predictionSize).Select(e => (double)e).ToArray();
            }

            var lastBatch = 100000m;
            var batchSize = 30;
            var absoluteMax = int.MaxValue;

            var ema = 100m;
            var lastEma = 200m;

            for (var iteration = 1; iteration <= absoluteMax; iteration++)
            {
                Log($"{iteration:00000} - ", false);

                teacher.RunEpoch(input, output);

                var learningError = 0m;
                var predictionError = 0m;

                var predicted = "";
                var actual = "";

                for (var pos = 0; pos < sampleCount; pos++)
                {

                    var inputs = input[pos];
                    var expected = series.Skip(pos + windowSize).Take(predictionSize).ToArray();

                    var calculatedRaw = network.Compute(inputs);

                    var calculated = normalizer.Denormalize(calculatedRaw.Select(e => (decimal)e)).ToArray();

                    var error = Enumerable.Range(0, predictionSize).Select(e => Math.Abs(1 - (calculated[e] / expected[e]))).Average()*100;

                    if(pos >= sampleCount - 1)
                    {
                        predictionError += error;
                        predicted = string.Join(",", calculated.Select(e => e.ToString("#0")));
                        actual = string.Join(",", expected.Select(e => e.ToString("#0"))); ;
                    }
                    else
                    {
                        learningError += error;
                    }

                }

                learningError /= (sampleCount - 1);

                ema = (ema * (batchSize-1) + predictionError) / batchSize;

                Log($"LE {learningError:#0.0000}%, Prediction {predicted:#0} ({actual:#0}) {predictionError:#0.0000}%, EMA {ema:#0.0000}%");
                
                if(iteration > batchSize && ema > lastEma)
                {
                    Log("Prediction quality not improving, breaking...");
                    break;
                }

                lastEma = ema;

            }

            var o = network;

        }

    }

}
