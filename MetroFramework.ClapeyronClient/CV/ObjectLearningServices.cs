using System.Collections.Generic;
using System.Drawing;

namespace ObjectRecognition
{
    public class ObjectLearningServices
    {
        public static void LearnObject(Bitmap learningImage, Bitmap backgroundImage, string objectName)
        {
            //get the object pallete
            Dictionary<Color, ColorInformation> objectPalleteData = PaletteAnalyzer.AnalyzeBitmaps(learningImage, backgroundImage);

            //convert into ColorData
            List<ObjectColorData> objectColorData = new List<ObjectColorData>();
            foreach (Color color in objectPalleteData.Keys)
            {
                objectColorData.Add(new ObjectColorData(color));
            }
            ObjectSignatureData objectSignature = new ObjectSignatureData(objectColorData, objectName);

            //save the learned object
            ObjectMemoryService.AddSignature(objectSignature);

        }

    }
}
