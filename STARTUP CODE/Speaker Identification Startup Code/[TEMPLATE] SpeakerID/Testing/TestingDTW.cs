using System;
using System.Collections.Generic;
using Recorder.MFCC;
using Recorder;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

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
            List<User> TrainingData = new List<User>();
            List<User> TestingData = new List<User>();
            List<UserSequence> TrainingUserSequences = new List<UserSequence>();
            UserSequence u = new UserSequence();
            Console.WriteLine("before user 1");

            u.userName = "1";
            u.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\conspiracy_Crystal_US_English.wav")));
            TrainingUserSequences.Add(u);
            Console.WriteLine("user 1");

            u.userName = "2";
            u.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\conspiracy_Mike_US_English.wav")));
            TrainingUserSequences.Add(u);
            Console.WriteLine("user 2");

            u.userName = "3";
            u.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\conspiracy_Rich_US_English.wav")));
            TrainingUserSequences.Add(u);
            Console.WriteLine("user 3");

            u.userName = "1";
            u.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\plausible_Crystal_US_English.wav")));
            TrainingUserSequences.Add(u);
            u.userName = "2";
            u.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\plausible_Mike_US_English.wav")));
            TrainingUserSequences.Add(u);
            u.userName = "3";
            u.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\plausible_Rich_US_English.wav")));
            TrainingUserSequences.Add(u);




            UserSequence testedUser = new UserSequence();
            testedUser.userName = "3";
            testedUser.sequence = AudioOperations.ExtractFeatures(AudioOperations.RemoveSilence(AudioOperations.OpenAudioFile(@"E:\[2] SPEAKER IDENTIFICATION-20250504T141248Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Input sample\ItIsPlausible_Rich_US_English.wav")));


            double minimumCost = double.PositiveInfinity;
            String matchedUserName = null;
            for (int z = 0; z < TrainingUserSequences.Count; z++)
            {
                int n = TrainingUserSequences[z].sequence.Frames.Length, m = testedUser.sequence.Frames.Length;

                double[][] distanceMatrix = new double[n][];
                for (int a = 0; a < n; a++)
                    distanceMatrix[a] = new double[m];

                distanceMatrix = DTW.ConstructDistanceMatrix(n, m, TrainingUserSequences[z].sequence, testedUser.sequence);

                double result = DTW.CalculateDTWDistanceWithWindow(TrainingUserSequences[z].sequence, testedUser.sequence, distanceMatrix, 5);

                if (minimumCost > result)
                {
                    minimumCost = result;
                    matchedUserName = TrainingUserSequences[z].userName;
                }
            }

            if (matchedUserName != null && matchedUserName != testedUser.userName)
                WrongAnswers++;

             
            

            Console.WriteLine("Number of Wrong Answers" + ": " + WrongAnswers);

        }



        static public void TestCase(String TrainingFileListName, String TestingFileListName, int testCaseNumber)
        {

            int WrongAnswers = 0;
            List<User> TrainingData = new List<User>();
            List<User> TestingData = new List<User>();
            List<UserSequence> TrainingUserSequences = new List<UserSequence>();

            Console.WriteLine("before loading training Data");
            if (testCaseNumber == 1)
                TrainingData = TestcaseLoader.LoadTestcase1Training(TrainingFileListName);
            else if (testCaseNumber == 2)
                TrainingData = TestcaseLoader.LoadTestcase2Training(TrainingFileListName);

            Console.WriteLine("after loading Training Data");
            ConcurrentBag<UserSequence> threadSafeSequences = new ConcurrentBag<UserSequence>();

            Parallel.For(0, TrainingData.Count, i =>
            {
                for (int j = 0; j < TrainingData[i].UserTemplates.Count; j++)
                {
                    UserSequence userSequence = new UserSequence
                    {
                        userName = TrainingData[i].UserName
                    };

                    Console.WriteLine($"extracting i: {i}, j: {j}");

                    userSequence.sequence = AudioOperations.ExtractFeatures(TrainingData[i].UserTemplates[j]);
                    threadSafeSequences.Add(userSequence);  // Thread-safe add
                }
            });

            // Now add all the results to the original list (outside the parallel block)
            TrainingUserSequences.AddRange(threadSafeSequences);

            Console.WriteLine("before loading testing Data");

            if (testCaseNumber == 1)
                TestingData = TestcaseLoader.LoadTestcase1Testing(TestingFileListName);
            else if (testCaseNumber == 2)
                TestingData = TestcaseLoader.LoadTestcase2Testing(TestingFileListName);
            Console.WriteLine("after loading testing Data");



            Parallel.For(0, TestingData.Count, i =>
            {
                for (int j = 0; j < TestingData[i].UserTemplates.Count; j++)
                {
                    UserSequence testedUser = new UserSequence
                    {
                        userName = TestingData[i].UserName,
                        sequence = AudioOperations.ExtractFeatures(TestingData[i].UserTemplates[j])
                    };

                    double minimumCost = double.PositiveInfinity;
                    string matchedUserName = null;

                    for (int z = 0; z < TrainingUserSequences.Count; z++)
                    {
                        int n = TrainingUserSequences[z].sequence.Frames.Length;
                        int m = testedUser.sequence.Frames.Length;

                        double[][] distanceMatrix = DTW.ConstructDistanceMatrix(n, m, TrainingUserSequences[z].sequence, testedUser.sequence);
                        double result = DTW.DTWDistance(TrainingUserSequences[z].sequence, testedUser.sequence, distanceMatrix);

                        Console.WriteLine($"measuring i: {i}, j: {j}, result= {result}");

                        if (minimumCost > result)
                        {
                            minimumCost = result;
                            matchedUserName = TrainingUserSequences[z].userName;
                        }
                    }

                    if (matchedUserName != null && matchedUserName != testedUser.userName)
                    {
                        Interlocked.Increment(ref WrongAnswers);  // Safe increment for shared variable
                    }
                }
            });


            Console.WriteLine("Number of Wrong Answers for Testcase " + testCaseNumber + ": " + WrongAnswers);

        }
    }
}
