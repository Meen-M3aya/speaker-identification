using System;
using System.Collections.Generic;
using Recorder.MFCC;
using Recorder;
using Recorder.FileManager;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace Recorder.Testing
{
    struct UserSequence
    {
        public string userName;
        public Sequence sequence;
    }

    public static class TestingDTW
    {

        static public void sampling()
        {
            int WrongAnswers = 0;
            List<UserSequence> TrainingUserSequences = new List<UserSequence>();

            string basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\TEST CASES\[1] SAMPLE"));

            string trainingPath = Path.Combine(basePath, "Training set");
            string inputSamplePath = Path.Combine(basePath, "Input sample");
            List<KeyValuePair<string, string>> trainingFiles = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1", "conspiracy_Crystal_US_English.wav"),
                new KeyValuePair<string, string>("2", "conspiracy_Mike_US_English.wav"),
                new KeyValuePair<string, string>("3", "conspiracy_Rich_US_English.wav"),
                new KeyValuePair<string, string>("1", "plausible_Crystal_US_English.wav"),
                new KeyValuePair<string, string>("2", "plausible_Mike_US_English.wav"),
                new KeyValuePair<string, string>("3", "plausible_Rich_US_English.wav")
            };


            Console.WriteLine("Starting training feature extraction...");
            foreach (var pair in trainingFiles)
            {
                string userName = pair.Key;
                string fileName = pair.Value;

                var audioPath = Path.Combine(trainingPath, fileName);
                var sequence = AudioOperations.ExtractFeatures(
                                    AudioOperations.RemoveSilence(
                                        AudioOperations.OpenAudioFile(audioPath)));

                TrainingUserSequences.Add(new UserSequence
                {
                    userName = userName,
                    sequence = sequence
                });

                Console.WriteLine($"Processed training file: {fileName}");
            }


            Console.WriteLine("Finished training. Starting testing...");

            string testFileName = "ItIsPlausible_Rich_US_English.wav";
            string testUser = "3";
            string testFilePath = Path.Combine(inputSamplePath, testFileName);

            UserSequence testedUser = new UserSequence
            {
                userName = testUser,
                sequence = AudioOperations.ExtractFeatures(
                               AudioOperations.RemoveSilence(
                                   AudioOperations.OpenAudioFile(testFilePath)))
            };

            double minimumCost = double.PositiveInfinity;
            string matchedUserName = null;

            foreach (var train in TrainingUserSequences)
            {
                int n = train.sequence.Frames.Length;
                int m = testedUser.sequence.Frames.Length;

                double[][] distanceMatrix = DTW.ConstructDistanceMatrix(n, m, train.sequence, testedUser.sequence);
                double result = DTW.CalculateDTWDistanceWithWindow(train.sequence, testedUser.sequence, distanceMatrix, 5);

                if (result < minimumCost)
                {
                    minimumCost = result;
                    matchedUserName = train.userName;
                }
            }

            if (matchedUserName != null && matchedUserName != testedUser.userName)
                WrongAnswers++;

            Console.WriteLine("Number of Wrong Answers: " + WrongAnswers);
        }




        static public void TestCase(int testCaseNumber, int pruningWidth)
        {
            List<User> TrainingData = new List<User>();
            List<User> TestingData = new List<User>();
            List<UserSequence> TrainingUserSequences = new List<UserSequence>();
            List<UserSequence> TestingUserSequences = new List<UserSequence>();
            Stopwatch stopWatch = new Stopwatch();




            string datasetFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,$@"..\..\TEST CASES\[2] COMPLETE\Case{testCaseNumber}"));
            string trainingFilePath="";
            string testingFilePath="";
            if (testCaseNumber == 1)
            {
                datasetFolder = Path.Combine(datasetFolder, "Small samples");
                trainingFilePath = Path.Combine(datasetFolder, "TrainingList.txt");
                testingFilePath = Path.Combine(datasetFolder, "TestingList5Samples.txt");
            }
            if (testCaseNumber == 2)
            {
                datasetFolder = Path.Combine(datasetFolder, "Medium samples");
                trainingFilePath = Path.Combine(datasetFolder, "TrainingList5Samples.txt");
                testingFilePath = Path.Combine(datasetFolder, "TestingList1Sample.txt");
            }
            if (testCaseNumber == 3)
            {
                datasetFolder = Path.Combine(datasetFolder, "Large samples");
                trainingFilePath = Path.Combine(datasetFolder, "TrainingList1Sample.txt");
                testingFilePath = Path.Combine(datasetFolder, "TestingList.txt");
            }


            

            string templatesFolderPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\tests"));
            string basePath = Path.Combine(templatesFolderPath, testCaseNumber.ToString());
            string trainingPath = Path.Combine(basePath, "training.json");
            string testingPath = Path.Combine(basePath, "testing.json");
            if(!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            var trainingFileManager = new FileManager<UserSequence>(trainingPath);
            var testingFileManager = new FileManager<UserSequence>(testingPath);

            // --- Load or Extract Training Features ---
            if (File.Exists(trainingPath))
            {
                Console.WriteLine("Loading pre-extracted training features...");
                TrainingUserSequences = trainingFileManager.LoadFromFile();
            }
            else
            {
                Console.WriteLine("Extracting training features...");
                TrainingData = LoadTrainingData(1, trainingFilePath);

                ConcurrentBag<UserSequence> threadSafeSequences = new ConcurrentBag<UserSequence>();
                stopWatch.Start();
                Parallel.ForEach(TrainingData, user =>
                {
                    foreach (var template in user.UserTemplates)
                    {
                        var userSeq = new UserSequence
                        {
                            userName = user.UserName,
                            sequence = AudioOperations.ExtractFeatures(template)
                        };
                        threadSafeSequences.Add(userSeq);
                    }
                });
                stopWatch.Stop();
                TimeSpan extractdur = stopWatch.Elapsed;
                String trainextract = $"Training Features extracted in {extractdur:hh\\:mm\\:ss} (hours:minutes:seconds)";
                Console.WriteLine(trainextract);
                stopWatch.Reset();

                TrainingUserSequences.AddRange(threadSafeSequences);
                trainingFileManager.SaveToFile(TrainingUserSequences);
            }

            // --- Load or Extract Testing Features ---
            if (File.Exists(testingPath))
            {
                Console.WriteLine("Loading pre-extracted testing features...");
                TestingUserSequences = testingFileManager.LoadFromFile();
            }
            else
            {
                Console.WriteLine("Extracting testing features...");
                TestingData = LoadTestingData(1, testingFilePath);

                ConcurrentBag<UserSequence> threadSafeSequences = new ConcurrentBag<UserSequence>();
                stopWatch.Start();
                Parallel.ForEach(TestingData, user =>
                {
                    foreach (var template in user.UserTemplates)
                    {
                        var userSeq = new UserSequence
                        {
                            userName = user.UserName,
                            sequence = AudioOperations.ExtractFeatures(template)
                        };
                        threadSafeSequences.Add(userSeq);
                    }
                });
                stopWatch.Stop();
                TimeSpan extractdur = stopWatch.Elapsed;
                String testextract = $"Training Features extracted in {extractdur:hh\\:mm\\:ss} (hours:minutes:seconds)";
                Console.WriteLine(testextract);
                stopWatch.Reset();

                TestingUserSequences.AddRange(threadSafeSequences);
                testingFileManager.SaveToFile(TestingUserSequences);
            }


            Console.WriteLine("begin classification...");
            // --- Classification ---
            int totalTests = TestingUserSequences.Count;
            int correctMatches = 0;
            stopWatch.Start();

            Parallel.ForEach(TestingUserSequences, testedUser =>
            {
                double minCost = double.PositiveInfinity;
                string matchedUserName = null;

                foreach (var trainSeq in TrainingUserSequences)
                {
                   /* double[][] matrix = DTW.ConstructDistanceMatrix(
                        testedUser.sequence.Frames.Length,
                        trainSeq.sequence.Frames.Length,
                        testedUser.sequence, trainSeq.sequence);*/

                    double result= double.PositiveInfinity;

                    if (pruningWidth == 0)
                        result = DTW.DTWDistance(testedUser.sequence, trainSeq.sequence);
                    else
                        result = DTW.CalculateDTWDistanceWithWindow(testedUser.sequence, trainSeq.sequence, pruningWidth);

                    if (result < minCost)
                    {
                        minCost = result;
                        matchedUserName = trainSeq.userName;
                    }
                }

                if (matchedUserName == testedUser.userName)
                {
                    Interlocked.Increment(ref correctMatches);
                }
            });
            stopWatch.Stop();
            TimeSpan testDuration = stopWatch.Elapsed;
            String testDurationOutput = $"Test Case {testCaseNumber} executed in {testDuration:hh\\:mm\\:ss} (hours:minutes:seconds)";
            Console.WriteLine(testDurationOutput);


            double accuracy = (double)correctMatches / totalTests * 100;
            Console.WriteLine($"Accuracy for Testcase {testCaseNumber}: {accuracy:F2}%");

        }

        private static List<User> LoadTrainingData(int testCase, string path)
        {
            return testCase == 1 ? TestcaseLoader.LoadTestcase1Training(path) :
                   testCase == 2 ? TestcaseLoader.LoadTestcase2Training(path) :
                   throw new ArgumentException("Unsupported test case");
        }

        private static List<User> LoadTestingData(int testCase, string path)
        {
            return testCase == 1 ? TestcaseLoader.LoadTestcase1Testing(path) :
                   testCase == 2 ? TestcaseLoader.LoadTestcase2Testing(path) :
                   throw new ArgumentException("Unsupported test case");
        }

    }
}
