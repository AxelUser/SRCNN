using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSuperResolution.SRCNN.Handler.Upscalling
{
    public class SRCNNModelLayer
    {
        public class ModelConfig
        {
            public string ModelName { get; set; }
            public int ScaleFactor { get; set; }
            public int Channels { get; set; }
            public int Offset { get; set; }
        }

        [JsonProperty("weight")]
        public double[][][][] Weights { get; internal set; }

        public int StrideHeight { get; set; }
        public int StrideWidth { get; set; }

        [JsonProperty("nInputPlane")]
        public int InputPlanesCount { get; set; }

        [JsonProperty("nOutputPlane")]
        public int OutputPlanesCount { get; set; }
        public int KernelWidth { get; set; }
        public int KernelHeight { get; set; }
        public ModelConfig ModelInfo { get; set; }

        [JsonProperty("bias")]
        public double[] Bias { get; set; }


        public SRCNNModelLayer()
        {

            //Weights = new double[5][][][];
        }

        public double[] GetAllWeights()
        {
            LinkedList<double> allWeights = new LinkedList<double>();
            foreach(double[][][] weightForOutputPlane in Weights)
            {
                foreach(double[][] weightMatrix in weightForOutputPlane)
                {
                    foreach (double[] weightVector in weightMatrix)
                    {
                        foreach(double omgForGodsSakeFinalWeight in weightVector)
                        {
                            allWeights.AddLast(omgForGodsSakeFinalWeight);
                        }                        
                    }
                }
            }
            return allWeights.ToArray();            
        }

        public static SRCNNModelLayer[] ReadModel(string json)
        {
            JArray jsonModel = JArray.Parse(json);
            List<SRCNNModelLayer> layers = new List<SRCNNModelLayer>();
            foreach (JToken jsonLayer in jsonModel)
            {
                var layer = JsonConvert.DeserializeObject<SRCNNModelLayer>(jsonLayer.ToString());
                layers.Add(layer);
            }

            return layers.ToArray();
        }
    }
}
