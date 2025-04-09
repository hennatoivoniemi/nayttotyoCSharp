using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using nayttotehtava.Models;

public class ImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

    //Kuvien ja tietojen haku tiedostoista
    public List<ImageWithInfo> GetImagesWithInfo()
    {
        string imagesPath = Path.Combine(_env.WebRootPath, "images");
        string infoPath = Path.Combine(_env.WebRootPath, "info");

        if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);
        if (!Directory.Exists(infoPath)) Directory.CreateDirectory(infoPath);

        var imageFiles = Directory
            .GetFiles(imagesPath)
            .Where(f => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }
            .Contains(Path.GetExtension(f).ToLower()));

        var result = new List<ImageWithInfo>();

        foreach (var img in imageFiles)
        {
            var imageName = Path.GetFileName(img);
            var baseName = Path.GetFileNameWithoutExtension(img);
            var txtPath = Path.Combine(infoPath, baseName + ".txt");

            var imageInfo = new ImageWithInfo
            {
                ImageUrl = "/images/" + imageName
            };

            if (File.Exists(txtPath))
            {
                var lines = File.ReadAllLines(txtPath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Nimi:"))
                        imageInfo.Species = line.Replace("Nimi:", "").Trim();
                    else if (line.StartsWith("P‰iv‰m‰‰r‰:"))
                        imageInfo.Date = line.Replace("P‰iv‰m‰‰r‰:", "").Trim();
                    else if (line.StartsWith("Paikka:"))
                        imageInfo.Location = line.Replace("Paikka:", "").Trim();
                }
            }

            result.Add(imageInfo);
        }

        return result;
    }

    //Kuvien ja tietojen lis‰ys
    public async Task SaveImageWithInfoAsync(string fileName, Stream fileStream, string species, string date, string location)
        {
            string imagesPath = Path.Combine(_env.WebRootPath, "images");
            string infoPath = Path.Combine(_env.WebRootPath, "info");

            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }
            if (!Directory.Exists(infoPath))
            {
                Directory.CreateDirectory(infoPath);
            }
            //T‰ss‰ tallennetaan kuva
            string imageFilePath = Path.Combine(imagesPath, Path.GetFileName(fileName));

            await using var stream = new FileStream(imageFilePath, FileMode.Create);
            await fileStream.CopyToAsync(stream);

            //T‰ss‰ tallennetaan kuvaan liittyv‰t tiedot
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string txtPath = Path.Combine(infoPath, baseName + ".txt");

            var lines = new List<string>
            {
                $"Nimi: {species}",
                $"P‰iv‰m‰‰r‰: {date}",
                $"Paikka: {location}"
            };

            await File.WriteAllLinesAsync(txtPath, lines);
        }


    }
