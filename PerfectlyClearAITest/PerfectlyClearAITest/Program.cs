using System;
using System.Collections.Generic;
using System.Drawing;
using PerfectlyClearAdapter;

namespace CSharp_AISample
{
	class Program
	{
		unsafe static void Main(string[] args)
		{
			string inname = "";
			string outname = "";
			string presetfile = "";
			bool bVerbose = false;

			int c, optind, idx;
			string optarg = null;
			optind = 0;
			idx = 0;
			string options = "v?";
			while ((c = getopt(args, options, ref optarg, ref optind)) != -1)
			{
				switch (c)
				{
					case -2:
						if (idx == 0)
							inname = optarg;
						else if (idx == 1)
							outname = optarg;
						else
							presetfile = optarg;
						idx++;
						break;

					case 'v':
						bVerbose = true;
						break;

					case '?':
						ShowHelp();
						return;
				}
			}

			if (inname == "")
			{
				Console.WriteLine("Path of input image is missing.");
				return;
			}
            //! [PFC_CSHARP_DETAILED_AI Instantiate]
            // Instantiate class object
            // if you have a license key, then call PFC_SetProtectionPath with the path
            // to your license files

            
            string execPath = AppDomain.CurrentDomain.BaseDirectory;
            string pathLicense = $"{execPath}sdk_license";
			PerfectlyClearAdapter.PerfectlyClear Pfc = new PerfectlyClearAdapter.PerfectlyClear(pathLicense);
			if (Pfc == null)
			{
				Console.WriteLine("Unable to create Pfc object.");
				return;
			}
			//! [PFC_CSHARP_DETAILED_AI Instantiate]

			//! [PFC_CSHARP_DETAILED_AI looks_setup]
			// Looks/LUTs require to load the .looks file, by default we will look into $PWD or equivalent
			// however, this might not be correct depending on where the binary is stored and
			// how it is called. For those cases you should set the correct UTF8-encoded path with the
			// following call.
			string addonPath = AppDomain.CurrentDomain.BaseDirectory;
			Pfc.SetAddonPath(addonPath);
			//! [PFC_CSHARP_DETAILED_AI looks_setup]

			//! [PFC_CSHARP_DETAILED_AI CreateAIEngine]
			// Use AI-enabled PFCENGINE to do scene detection and apply a scene preset
			// IMPORTANT: initialization of AI engine takes time.

			// Load AI engine (assuming all dlls and models are in exe folder):
			//string execPath = AppDomain.CurrentDomain.BaseDirectory;
			int aistatus = Pfc.LoadAiEngine(PFCAIFEATURE.AI_SCENE_DETECTION | PFCAIFEATURE.AI_CORRECTIONS, execPath);
			if (((PFCAIFEATURE)aistatus & PFCAIFEATURE.AI_SCENE_DETECTION) != PFCAIFEATURE.AI_SCENE_DETECTION) {
				Console.WriteLine("Cannot load AI SD engine. Make sure the pnn file and PFCAI libraries are in the executable directory or change binPath.");
				return;
			}

			if (((PFCAIFEATURE)aistatus & PFCAIFEATURE.AI_CORRECTIONS) != PFCAIFEATURE.AI_CORRECTIONS) {
				Console.WriteLine("Cannot load AI corrections engine. Make sure the pnn file and PFCAI libraries are in the executable directory or change binPath.");
				return;
			}
			//! [PFC_CSHARP_DETAILED_AI CreateAIEngine]

			//! [PFC_CSHARP_DETAILED_AI LoadScenePresets]
			// You can change the presets from a custom preset file with LoadScenePresets but it
			// must have same group and presets uids as embedded preset.
			//
			// By default when loading a AI_SCEE_DECTECTION we load the embeded presets immediately afterwards.
			// You can return back to default presets  embedded into the model (.pnn) with presetPath = null.
			
			if(presetfile != "") {
				int retLoadPresets = Pfc.LoadScenePresets(presetfile);
				if (0 != retLoadPresets) {
					 Console.WriteLine("Cannot load scene presets file '{0}' error '{1}'.", presetfile, retLoadPresets);
					 return;
				}
			}
			//! [PFC_CSHARP_DETAILED_AI LoadScenePresets]

			// Optionally you can read image file with class method ReadImage.
			//Bitmap bm = new Bitmap(inname);
			// Check if image opened successfully.
			//if (bm == null)
			//{
			//	Console.WriteLine("Unable to open image.");
			//	return;
			//}

            // Optionally you can read image file with class method ReadImage.
            //Bitmap bmpOriginalImage = new Bitmap(presentationOriginalImagePath);
            PerfectlyClearAdapter.PFCImageFile bm = new PerfectlyClearAdapter.PFCImageFile();
            bm.LoadImage(inname);
            //Common.ExifRotate(bmpOriginalImage);


            // Optionally you can read image file with class method ReadImage.
            //Bitmap bm = new Bitmap(aI_Test.ImagePath);
            // Check if image opened successfully.
            if (bm == null)
            {
                Console.WriteLine("Unable to open image.");
                return;
            }


			//! [PFC_CSHARP_DETAILED_AI Precalc]
            // Perform profile calculation on image
            PerfectlyClearAdapter.ADPTRRETURNCODE cret = Pfc.Calc(ref bm, PerfectlyClearAdapter.PFCFEATURE.CALC_ALL | PerfectlyClearAdapter.PFCFEATURE.CALC_SCENE_DETECTION, -1, null);

			// Check status of precalc analysis.
			if (bVerbose)
			{
				Console.WriteLine("Precalc return code: " + cret.ToString());
				Console.WriteLine("Noise removal return code: " + Pfc.LastStatus.NR_Status.ToString());
				Console.WriteLine("Perfectly Clear core return code: " + Pfc.LastStatus.CORE_Status.ToString());
				Console.WriteLine("Face beautification return code: " + Pfc.LastStatus.FB_Status.ToString());
				Console.WriteLine("Red eye removal return code: " + Pfc.LastStatus.RE_Status.ToString());
			}

			//! [PFC_CSHARP_DETAILED_AI Precalc]

			//! [PFC_CSHARP_DETAILED_AI GetScene]
			// After detectionn the scene id and model version can be optained through PFC_GetScene/
			// The specific meaning depends on the model loaded.
			int version = 0;
			int detectedScene = Pfc.GetScene(&version);
			Console.WriteLine("Model '{0}' assigned scene '{1}' to the image.\n", version, detectedScene);
			//! [PFC_CSHARP_DETAILED_AI GetScene]

			//! [PFC_CSHARP_DETAILED_AI GetSceneParams]
			// Apply scene preset for detected scene by getting the params to apply from the AI-enabled
			if (detectedScene >= 0) {
				PFCPARAM sceneParam = new PFCPARAM();
				if (0 == Pfc.ReadScenePreset(ref sceneParam, detectedScene)) {
					Pfc.m_Param = sceneParam;
				}
			}
			//! [PFC_CSHARP_DETAILED_AI GetSceneParams]

			// Optionally you can override the parameters further.
			// For examples:

			//   1. Use a different contrast mode.
			//Pfc.m_Param.core.eContrastMode = PerfectlyClearAdapter.CONTRASTMODE.HIGH_CONTRAST;

			//   2. Enable Abnormal Tint Removal.
			//Pfc.m_Param.core.bAbnormalTintRemoval = true;
			//Pfc.m_Param.core.eTintMode = PerfectlyClearAdapter.TINTCORRECTION.TINTCORRECT_CONSERVATIVE;

			//! [PFC_CSHARP_DETAILED_AI Apply]
			// Enhance image.
			PerfectlyClearAdapter.PFCAPPLYSTATUS aret = Pfc.Apply(ref bm, 100);

			// Optionally show return code of the process.
			if (bVerbose)
			{
				// Check if there's any warning
				if (aret == PerfectlyClearAdapter.PFCAPPLYSTATUS.APPLY_SUCCESS)
				{
					Console.WriteLine("Image processed successfully.");
				}
				else if (aret > 0)
				{
					// In case of error, query LastStatus for individual return code
					Console.WriteLine("Noise removal return code: " + Pfc.LastStatus.NR_Status.ToString());
					Console.WriteLine("Perfectly Clear core return code: " + Pfc.LastStatus.CORE_Status.ToString());
					Console.WriteLine("Face beautification return code: " + Pfc.LastStatus.FB_Status.ToString());
					Console.WriteLine("Red eye removal return code: " + Pfc.LastStatus.RE_Status.ToString());
				}
				else if (aret < 0)
				{
					// In case of general process error (negative return code)
					Console.WriteLine("Perfectly Clear return code: " + aret.ToString());
				}
			}
			//! [PFC_CSHARP_DETAILED_AI Apply]

			// That's it. The image in bm is now enhanced!
			if (outname == "")
			{
				outname = GetDefaultName(inname);
			}
			// Save processed image.
			//bm.Save(outname);
		}


		// Utility function to show usage of this command line tool.
		static void ShowHelp()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("> PerfectlyClear input [output] [presetfile] [-v]");
			Console.WriteLine("     input      - path of input jpeg file.");
			Console.WriteLine("     output     - path of output jpeg file.");
			Console.WriteLine("     presetfile - path to the preset file. (A .preset file can be exported from Athentech desktop softwares.)");
			Console.WriteLine("     -v       - verbose\n");
			Console.WriteLine("example: > PerfectlyClear input.jpg output.jpg my.preset -v\n\n");
		}
		// Utility function to generate a default file name.
		static string GetDefaultName(string path)
		{
			int idx = path.IndexOf('.');
			string name = path.Substring(0, idx);
			string ext = path.Substring(idx, path.Length - idx);
			return name + "_perfectlyclear" + ext;
		}
		static int getopt(string[] argv, string optstring, ref string optarg, ref int optind)
		{
			if (optind >= argv.Length)
			{
				return -1;
			}

			if (argv[optind][0] != '-')
			{
				optarg = argv[optind++];
				return -2;
			}

			char opt = argv[optind][1];
			int idx = optstring.IndexOf(opt);

			if (idx == -1)
			{
				return '?';
			}
			optind++;
			if (((idx + 1) < optstring.Length) && optstring[idx + 1] == ':')
			{
				if (optind >= argv.Length)
				{
					return '?';
				}
				optarg = argv[optind];
				optind++;
			}
			return opt;
		}
	}
}
