using FaceRecognitionDotNet;

namespace WebApiDemo.Util
{

    public static class FaceRecognitionFactory
    {
        public static object o = new object();
        public static FaceRecognition FaceRecognition = FaceRecognition.Create(Path.Combine(Environment.CurrentDirectory, "models"));
        //static string _modelsFolder = Path.Combine(Environment.CurrentDirectory, "models");
        //static FaceRecognition Instance
        //{
        //    get
        //    {
        //        if (FaceRecognition == null)
        //        {
        //            lock (o)
        //            {
        //                if (FaceRecognition == null)
        //                {
        //                    FaceRecognition = FaceRecognition.Create(_modelsFolder);
        //                }

        //            }
        //        }

        //        return FaceRecognition;
        //    }
        //}

    }
}
