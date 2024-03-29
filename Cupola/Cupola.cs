﻿using ComputeSharp;

namespace Cupola
{
    public partial class Cupola

    {
        public static void RunSingle(string imgDir, string outDir)
        {
            ReadWriteTexture2D<Bgra32, float4>[] images = Load(imgDir);

            ReadWriteTexture2D<Bgra32, float4> finalImage = RunSingle(images);

            finalImage.Save(outDir + ".jpg");
        }

        public static ReadWriteTexture2D<Bgra32, float4> RunSingle(ReadWriteTexture2D<Bgra32, float4>[] images)
        {
            ReadWriteTexture2D<Bgra32, Float4> brightest = images[0];
            ReadWriteTexture2D<Bgra32, Float4> output = images[0];

            for (int i = 1; i < images.Length; i++)
            {
                Console.WriteLine(i.ToString());

                ReadWriteTexture2D<Bgra32, Float4> temp = images[i];

                GraphicsDevice.GetDefault().For
                (
                    temp.Width,
                    temp.Height,
                    new Brightest(brightest, temp)
                );

                GraphicsDevice.GetDefault().For
                (
                    temp.Width,
                    temp.Height,
                    new Average(temp, output, 0.5f)
                );

                GraphicsDevice.GetDefault().For
                (
                    temp.Width,
                    temp.Height,
                    new Average(temp, brightest, 0.6f)
                );

                output = temp;
            }

            return output;
        }

        public static void RunMulti(string imgDir, string outDir)
        {
            ReadWriteTexture2D<Bgra32, float4>[] images = Load(imgDir);

            ReadWriteTexture2D<Bgra32, float4>[] imagesOut = RunMulti(images);

            for (int i = 0; i < imagesOut.Length; i++)
            {
                imagesOut[i].Save(outDir + "-" + i.ToString() + ".jpg");
            }
        }

        public static ReadWriteTexture2D<Bgra32, float4>[] RunMulti(ReadWriteTexture2D<Bgra32, float4>[] images)
        {
            ReadWriteTexture2D<Bgra32, float4>[] outputImages = new ReadWriteTexture2D<Bgra32, Float4>[images.Length];

            ReadWriteTexture2D<Bgra32, Float4> brightest = images[0];
            ReadWriteTexture2D<Bgra32, Float4> output = images[0];

            outputImages[0] = images[0];

            for (int i = 1; i < images.Length; i++)
            {
                Console.WriteLine(i.ToString());

                ReadWriteTexture2D<Bgra32, Float4> temp = images[i];

                GraphicsDevice.GetDefault().For
                (
                    temp.Width,
                    temp.Height,
                    new Brightest(brightest, temp)
                );

                GraphicsDevice.GetDefault().For
                (
                    temp.Width,
                    temp.Height,
                    new Average(temp, output, 0.5f)
                );

                GraphicsDevice.GetDefault().For
                (
                    temp.Width,
                    temp.Height,
                    new Average(temp, brightest, 0.6f)
                );

                output = temp;

                outputImages[i] = output;
            }

            return outputImages;
        }

        public static ReadWriteTexture2D<Bgra32, float4>[] Load(string dir)
        {
            string[] files = Directory.GetFiles(dir);

            return Load(files);
        }

        public static ReadWriteTexture2D<Bgra32, float4>[] Load(string[] files)
        {
            ReadWriteTexture2D<Bgra32, float4>[] images = new ReadWriteTexture2D<Bgra32, Float4>[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(files[i]);
                images[i] = GraphicsDevice.GetDefault().LoadReadWriteTexture2D<Bgra32, float4>(files[i]);
            }

            return images;
        }

        [AutoConstructor]
        public readonly partial struct Brightest : IComputeShader
        {
            public readonly IReadWriteNormalizedTexture2D<float4> input1;
            public readonly IReadWriteNormalizedTexture2D<float4> input2;

            // Other captured resources or values here...

            public void Execute()
            {
                float3 i1 = input1[ThreadIds.XY].RGB;
                float3 i2 = input2[ThreadIds.XY].RGB;

                float i1Intensity = (i1.X * i1.X) + (i1.Y * i1.Y) + (i1.Z * i1.Z);
                float i2Intensity = (i2.X * i2.X) + (i2.Y * i2.Y) + (i2.Z * i2.Z);

                if (i1Intensity > i2Intensity)
                {
                    input1[ThreadIds.XY].RGB = i1;
                }
                else
                {
                    input1[ThreadIds.XY].RGB = i2;
                }
            }
        }

        [AutoConstructor]
        public readonly partial struct Average : IComputeShader
        {
            public readonly IReadWriteNormalizedTexture2D<float4> input1;
            public readonly IReadWriteNormalizedTexture2D<float4> input2;
            public readonly float weight;

            // Other captured resources or values here...

            public void Execute()
            {
                float w1 = weight;
                float w2 = 1 - weight;

                float3 i1 = input1[ThreadIds.XY].RGB;
                float3 i2 = input2[ThreadIds.XY].RGB;

                float3 o1 = 0;

                o1.X = (i1.X * w1) + (i2.X * w2);
                o1.Y = (i1.Y * w1) + (i2.Y * w2);
                o1.Z = (i1.Z * w1) + (i2.Z * w2);

                input1[ThreadIds.XY].RGB = o1;
            }
        }
    }
}