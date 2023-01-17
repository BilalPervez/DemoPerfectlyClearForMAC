using PerfectlyClearAdapter;
using PerfectlyClearAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PerfectlyClearAITest;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);

            processImageWithPerfectlyClearSDK();

        }
    }


    unsafe public void processImageWithPerfectlyClearSDK()
        {
        string presentationPath = "FlashopStudio";
        string inputPath = "FlashopStudio";
        string pcImageOutputPath = $"{presentationPath}/pc_output.jpg";
        Directory.CreateDirectory(presentationPath);
        if (Directory.Exists(presentationPath))
        {
            if (File.Exists(inputPath))
            {

                if (File.Exists(inputPath))
                {
                    // Load AI engine (assuming all dlls and models are in exe folder):
                    string execPath = AppDomain.CurrentDomain.BaseDirectory;
                    string pathLicense = $"{execPath}sdk_license";
                    PerfectlyClearAdapter.PerfectlyClear Pfc = new PerfectlyClearAdapter.PerfectlyClear(pathLicense);
                    if (Pfc == null)
                    {
                        _logger.LogInformation("Unable to create Pfc object. No license found.");

                        return;
                    }


                    int aistatus = Pfc.LoadAiEngine(PFCAIFEATURE.AI_SCENE_DETECTION | PFCAIFEATURE.AI_CORRECTIONS, execPath);
                    if (((PFCAIFEATURE)aistatus & PFCAIFEATURE.AI_SCENE_DETECTION) != PFCAIFEATURE.AI_SCENE_DETECTION)
                    {
                        Console.WriteLine("Cannot load AI SD engine. Make sure the pnn file and PFCAI libraries are in the executable directory or change binPath.");
                        return;
                    }

                    if (((PFCAIFEATURE)aistatus & PFCAIFEATURE.AI_CORRECTIONS) != PFCAIFEATURE.AI_CORRECTIONS)
                    {
                        Console.WriteLine("Cannot load AI corrections engine. Make sure the pnn file and PFCAI libraries are in the executable directory or change binPath.");
                        return;
                    }

                    if (inputPath != "")
                    {
                        //int retLoadPresets = Pfc.LoadScenePresets(inputPath);
                        //if (0 != retLoadPresets)
                        //{
                        //    Console.WriteLine("Cannot load scene presets file '{0}' error '{1}'.", inputPath, retLoadPresets);
                        //    return;
                        //}
                    }
                    //! [PFC_CSHARP_DETAILED_AI LoadScenePresets]

                    // Optionally you can read image file with class method ReadImage.
                    //Bitmap bm = new Bitmap(aI_Test.ImagePath);
                    // Check if image opened successfully.
                    //if (bm == null)
                    //{
                    //    Console.WriteLine("Unable to open image.");
                    //    return;
                    //}

                    //! [PFC_CSHARP_DETAILED_AI Precalc]
                    // Perform profile calculation on image
                    //PerfectlyClearAdapter.ADPTRRETURNCODE cret = Pfc.Calc(ref bm, PerfectlyClearAdapter.PFCFEATURE.CALC_ALL | PerfectlyClearAdapter.PFCFEATURE.CALC_SCENE_DETECTION, -1, null);

                }
                else
                {
                    _logger.LogInformation("No preset found in the following path {0}", inputPath);
                    //FileLog.WriteLine(2, "No presetfound in the following path {0}", aI_Test.PresetFilePath);
                    
                }



            }
            else
            {
                _logger.LogInformation("No images found in the following path {0}", inputPath);
                //FileLog.WriteLine(2, "No images found in the following path {0}", aI_Test.ImagePath);
            }
        }
        else
        {
            _logger.LogInformation("Flashop Directory C: drive path {0} found", presentationPath);
            //FileLog.WriteLine(2, "Flashop Directory C: drive path {0} found", presentationPath);
        }

    }

}

