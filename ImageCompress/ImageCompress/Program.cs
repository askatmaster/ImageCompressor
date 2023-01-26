using ImageCompress;
Console.WriteLine("Start process!");

//Compress image
var compressedImage = ImageProcessor.Compress(@"C:\TestImage.jpg", 1024);
using var compressFileStream = new FileStream(@"C:\CompressedImage.jpg", FileMode.Create);
compressFileStream.Write(compressedImage);

//Resize image
var resizedImage = ImageProcessor.ResizeImage(@"C:\TestImage.jpg", 1920, 1080);
using var resizedFileStream = new FileStream(@"C:\ResizedImage.jpg", FileMode.Create);
resizedFileStream.Write(resizedImage);

Console.WriteLine("Finish process!");