using System;
using System.Collections.Generic;
using Recorder.MFCC;
using Recorder;

namespace Recorder.Testing
{
    struct UserSequence
    {
        public string userName;
        public Sequence sequence;
    }

    public static class TestingDTW
    {
        static public void TestCase(String TrainingFileListName, String TestingFileListName, int testCaseNumber)
        {

            int WrongAnswers = 0;
            List<User> TrainingData = new List<User>();
            List<User> TestingData = new List<User>();
            List<UserSequence> TrainingUserSequences = new List<UserSequence>();


            if (testCaseNumber == 1)
                TrainingData = TestcaseLoader.LoadTestcase1Training(TrainingFileListName);
            else if (testCaseNumber == 2)
                TrainingData = TestcaseLoader.LoadTestcase2Training(TrainingFileListName);

            Console.WriteLine("after loading Testing Data");
            for (int i = 0; i < TrainingData.Count; i++)
            {
                for (int j = 0; j < TrainingData[i].UserTemplates.Count; j++)
                {
                    UserSequence userSequence = new UserSequence();
                    userSequence.userName = TrainingData[i].UserName;
                    userSequence.sequence = AudioOperations.ExtractFeatures(TrainingData[i].UserTemplates[j]);
                    TrainingUserSequences.Add(userSequence);
                }
            }

            if (testCaseNumber == 1)
                TestingData = TestcaseLoader.LoadTestcase1Testing(TestingFileListName);
            else if (testCaseNumber == 2)
                TestingData = TestcaseLoader.LoadTestcase2Testing(TestingFileListName);



            for (int i = 0; i < TestingData.Count; i++)
            {
                for (int j = 0; j < TestingData[i].UserTemplates.Count; j++)
                {
                    UserSequence testedUser = new UserSequence();
                    testedUser.userName = TestingData[i].UserName;
                    testedUser.sequence = AudioOperations.ExtractFeatures(TestingData[i].UserTemplates[j]);


                    double minimumCost = double.PositiveInfinity;
                    String matchedUserName = null;
                    for (int z = 0; z < TrainingUserSequences.Count; z++)
                    {
                        int n = TrainingUserSequences[z].sequence.Frames.Length, m = testedUser.sequence.Frames.Length;

                        double[][] distanceMatrix = new double[n][];
                        for (int a = 0; a < n; a++)
                            distanceMatrix[a] = new double[m];

                        distanceMatrix = DTW.ConstructDistanceMatrix(n, m, TrainingUserSequences[z].sequence, testedUser.sequence);

                        double result = DTW.DTWDistance(TrainingUserSequences[z].sequence, testedUser.sequence, distanceMatrix);

                        if (minimumCost > result)
                        {
                            minimumCost = result;
                            matchedUserName = TrainingUserSequences[z].userName;
                        }
                    }

                    if (matchedUserName != null && matchedUserName != testedUser.userName)
                        WrongAnswers++;

                }
            }

            Console.WriteLine("Number of Wrong Answers for Testcase " + testCaseNumber + ": " + WrongAnswers);

        }
    }
}
