using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using CoreFtp;
using CoreFtp.Enum;
using OpenCvSharp;

namespace ImageProfessorImageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load("./ServerInformation.env");

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.Port = 5672;
            connectionFactory.HostName = "localhost";
            connectionFactory.UserName = "professor";
            connectionFactory.Password = "rlawnsdn";
            connectionFactory.VirtualHost = "image_professor";

            IConnection connection = connectionFactory.CreateConnection();
            IModel channel = connection.CreateModel();
            channel.BasicQos(0, 1, false);
            EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);

            eventingBasicConsumer.Received += async (sender, basicDeliveryEventArgs) =>
            {
                IBasicProperties basicProperties = basicDeliveryEventArgs.BasicProperties;
                Console.WriteLine("Message received by the event based consumer. Check the debug window for details.");

                var message = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);

                string[] _args = message.Split(',');

                if (_args.Count() == 4)
                {
                    string username = _args[0];
                    string imageID = _args[1];
                    string filename = _args[2];
                    string effectType = _args[3];

                    using (var ftpClient = new FtpClient(new FtpClientConfiguration
                    {
                        Host = System.Environment.GetEnvironmentVariable("hostname"),
                        Username = System.Environment.GetEnvironmentVariable("username"),
                        Password = System.Environment.GetEnvironmentVariable("password"),
                        Port = 1024,
                        EncryptionType = FtpEncryption.Implicit,
                        IgnoreCertificateErrors = true
                    }))
                    {
                        // File Downloading
                        var tempFile = new FileInfo(filename);
                        await ftpClient.LoginAsync();
                        using (var ftpReadStream = await ftpClient.OpenFileReadStreamAsync(filename))
                        {
                            using (var fileWriteStream = tempFile.OpenWrite())
                            {
                                await ftpReadStream.CopyToAsync(fileWriteStream);
                            }
                        }

                        // Image Processing
                        Mat dstImage = processImage(effectType, tempFile.FullName);

                        dstImage.SaveImage(tempFile.FullName);
                    }

                }

                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
            };

            channel.BasicConsume("kimjunu.queue.ip.srcs", false, eventingBasicConsumer);
        }

        static Mat processImage(string effectType, string filename)
        {
            Mat dstImage;
            if (effectType == "grayscale")
            {
                dstImage = new Mat(filename, ImreadModes.GrayScale);
            }
            else if (effectType == "canny")
            {
                Mat srcImage = new Mat(filename, ImreadModes.GrayScale);
                dstImage = new Mat();

                Cv2.Canny(srcImage, dstImage, 50, 200);
            }
            else
            {
                dstImage = new Mat(filename, ImreadModes.AnyColor);
            }

            return dstImage;
        }
    }
}