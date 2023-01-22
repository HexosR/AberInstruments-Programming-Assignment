using CsvHelper;
using System.Globalization;
using System.Xml;

namespace AberInstruments_RobertMlynarczyk.Controller
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string pathXML = "";
			string pathCSV = "";

			for (int i = 1; i <= 5; i++)
			{
				pathXML = @"..\..\..\Data\OvenTest" + i + @"\OvenCyclingData" + i + ".xml";
				pathCSV = @"..\..\..\Data\OvenTest" + i + @"\processed_ovenreadings_full.csv";

				string serialNumber = getSerialNumberFromXML(pathXML);
				string completedTimeStamp = getCompletedTimeStampFromXML(pathXML);
				//to make program faster we can create a function that returns both temperatures
				float highestTemp = getHighestTemperatureFromCVS(pathCSV);
				float lowestTemp = getLowestTemperatureFromCVS(pathCSV);
				List<string> listHigh = getTimesWhenHighestTemperature(pathCSV, highestTemp);
				List<string> listLow = getTimesWhenLowestTemperature(pathCSV, lowestTemp);
				List<string> output = getAverageOutput(pathCSV, listHigh, listLow);
				saveDataInCSVFile(serialNumber, completedTimeStamp, output, listHigh, listLow, i);
			}
		}
		//Using csvHelper to save all data to .csv file
		public static void saveDataInCSVFile(string serialNumber, string completedTimeStamp, List<string> output, List<string> listHigh, List<string> listLow, int i)
		{
			int pointer = 1;
			//files are stored in bin/Debug/net6.0. Each file for each OvenTest
			var csvPath = Path.Combine(Environment.CurrentDirectory, $"output" + i + ".csv");
			using(var writer = new StreamWriter(csvPath))
			using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
			{
				csv.WriteField("Serial number: ");
				csv.NextRecord();
				csv.WriteField(serialNumber);
				csv.NextRecord();
				csv.WriteField("Completed Time Stamp: " );
				csv.NextRecord();
				csv.WriteField(completedTimeStamp);
				csv.NextRecord();
				foreach(string str in output)
				{
					csv.WriteField(str);
					csv.NextRecord();
				}
				csv.WriteField("Time periods where the oven is at its maximum temperature:");
				csv.NextRecord();
				foreach (string str in listHigh)
				{
					if(pointer == 1)
					{
						csv.WriteField(str);
						pointer--;
					}
					else
					{
						csv.WriteField(str);
						csv.NextRecord();
						pointer = 1;
					}
				}
				csv.WriteField("Time periods where the oven is at its minimum temperature:");
				csv.NextRecord();
				foreach (string str in listLow)
				{
					if (pointer == 1)
					{
						csv.WriteField(str);
						pointer--;
					}
					else
					{
						csv.WriteField(str);
						csv.NextRecord();
						pointer = 1;
					}
				}
			}
		}
		public static List<string> getAverageOutput(string path, List<string> highTimes, List<string> lowTimes)
		{
			using (var reader = new StreamReader(path))
			{
				var headerLine = reader.ReadLine();

				bool isHighTempReached = false;
				bool isLowTempReached = false;
				int high50kHzCounter = 0;
				int high64kHzCounter = 0;
				int low50kHzCounter = 0;
				int low64kHzCounter = 0;
				float highV1_fb50kHz = 0;
				float highG50kHz = 0;
				float highC50kHz = 0;
				float highV1_fb64kHz = 0;
				float highG64kHz = 0;
				float highC64kHz = 0;
				float lowV1_fb50kHz = 0;
				float lowG50kHz = 0;
				float lowC50kHz = 0;
				float lowV1_fb64kHz = 0;
				float lowG64kHz = 0;
				float lowC64kHz = 0;
				List<string> averageOutput = new List<string>();

				while (!reader.EndOfStream)
				{
					var values = reader.ReadLine().Split(',');
					if (highTimes[highTimes.Count - 2] == values[2])
					{
						isHighTempReached = true;
					}
					if (highTimes[highTimes.Count - 1] == values[2])
					{
						isHighTempReached = false;
					}
					if (isHighTempReached == true)
					{
						if (values[3] == "50")
						{
							high50kHzCounter++;
							highV1_fb50kHz += float.Parse(values[10], CultureInfo.InvariantCulture.NumberFormat);
							highG50kHz += float.Parse(values[7], CultureInfo.InvariantCulture.NumberFormat);
							highC50kHz += float.Parse(values[6], CultureInfo.InvariantCulture.NumberFormat);
						}
						if (values[3] == "64")
						{
							high64kHzCounter++;
							highV1_fb64kHz += float.Parse(values[10], CultureInfo.InvariantCulture.NumberFormat);
							highG64kHz += float.Parse(values[7], CultureInfo.InvariantCulture.NumberFormat);
							highC64kHz += float.Parse(values[6], CultureInfo.InvariantCulture.NumberFormat);
						}
					}
					if (lowTimes[lowTimes.Count - 2] == values[2])
					{
						isLowTempReached = true;
					}
					if (lowTimes[lowTimes.Count - 1] == values[2])
					{
						isLowTempReached = false;
					}
					if (isLowTempReached == true)
					{
						if (values[3] == "50")
						{
							low50kHzCounter++;
							lowV1_fb50kHz += float.Parse(values[10], CultureInfo.InvariantCulture.NumberFormat);
							lowG50kHz += float.Parse(values[7], CultureInfo.InvariantCulture.NumberFormat);
							lowC50kHz += float.Parse(values[6], CultureInfo.InvariantCulture.NumberFormat);
						}
						if (values[3] == "64")
						{
							low64kHzCounter++;
							lowV1_fb64kHz += float.Parse(values[10], CultureInfo.InvariantCulture.NumberFormat);
							lowG64kHz += float.Parse(values[7], CultureInfo.InvariantCulture.NumberFormat);
							lowC64kHz += float.Parse(values[6], CultureInfo.InvariantCulture.NumberFormat);
						}
					}
				}
				averageOutput.Add("Average V1_fb at 50kHz at a maximum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(highV1_fb50kHz / high50kHzCounter));
				averageOutput.Add("Average G at 50kHz at a maximum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(highG50kHz / high50kHzCounter));
				averageOutput.Add("Average C at 50kHz at a maximum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(highC50kHz / high50kHzCounter));

				averageOutput.Add("Average V1_fb at 64kHz at a maximum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(highV1_fb64kHz / high64kHzCounter));
				averageOutput.Add("Average G at 64kHz at a maximum temperature for the last oven full cycle:");
				averageOutput.Add(Convert.ToString(highG64kHz / high64kHzCounter));
				averageOutput.Add("Average C at 64kHz at a maximum temperature for the last oven full cycle:");
				averageOutput.Add(Convert.ToString(highC64kHz / high64kHzCounter));

				averageOutput.Add("Average V1_fb at 50kHz at a minimum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(lowV1_fb50kHz / low50kHzCounter));
				averageOutput.Add("Average G at 50kHz at a minimum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(lowG50kHz / low50kHzCounter));
				averageOutput.Add("Average C at 50kHz at a minimum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(lowC50kHz / low50kHzCounter));

				averageOutput.Add("Average V1_fb at 64kHz at a minimum temperature for the last full oven cycle:");
				averageOutput.Add(Convert.ToString(lowV1_fb64kHz / low64kHzCounter));
				averageOutput.Add("Average G at 64kHz at a minimum temperature for the last oven full cycle:");
				averageOutput.Add(Convert.ToString(lowG64kHz / low64kHzCounter));
				averageOutput.Add("Average C at 64kHz at a minimum temperature for the last oven full cycle:");
				averageOutput.Add(Convert.ToString(lowC64kHz / low64kHzCounter));
				
				return averageOutput;
			}
		}
		public static List<string> getTimesWhenHighestTemperature(string path, float highestTemp)
		{
			using (var reader = new StreamReader(path))
			{
				var headerLine = reader.ReadLine();

				List<string> times = new List<string>();
				bool isHighest = false;
				float temp = 0;
				int twoMinutes = 120;

				while (!reader.EndOfStream)
				{
					var values = reader.ReadLine().Split(',');
					//check if value is not blank
					if (values[5] != "" && (twoMinutes == 120 || twoMinutes < 0))
					{
						temp = float.Parse(values[5], CultureInfo.InvariantCulture.NumberFormat);
					}
					//the cycle is finished, tmeperature is decreasing
					if (temp < (highestTemp - 3) && isHighest)
					{
						times.Add(values[2]);
						isHighest = false;
						twoMinutes = 120;
					}
					//the cycle began, temperature is rising
					if (temp >= (highestTemp - 3) && !isHighest)
					{
						isHighest = true;
						times.Add(values[2]);
					}
					if(isHighest)
					{
						twoMinutes--;
					}
				}
				return times;
			}
		}
		public static List<string> getTimesWhenLowestTemperature(string path, float lowestTemp)
		{
			using (var reader = new StreamReader(path))
			{
				var headerLine = reader.ReadLine();

				List<string> times = new List<string>();
				bool isLowest = false;
				float temp = 0;
				int twoMinutes = 120;

				while (!reader.EndOfStream)
				{
					var values = reader.ReadLine().Split(',');

					if (values[5] != "" && (twoMinutes == 120 || twoMinutes < 0))
					{
						temp = float.Parse(values[5], CultureInfo.InvariantCulture.NumberFormat);
					}
					if (temp > (lowestTemp + 3) && isLowest)
					{
						times.Add(values[2]);
						isLowest = false;
						twoMinutes = 120;
					}
					if (temp <= (lowestTemp + 3) && !isLowest)
					{
						isLowest = true;
						times.Add(values[2]);
					}
					if (isLowest)
					{
						twoMinutes--;
					}
				}
				return times;
			}
		}
		public static float getHighestTemperatureFromCVS(string path)
		{
			using (var reader = new StreamReader(path))
			{
				//get rid of the first line in CSV file
				var headerLine = reader.ReadLine();

				float highestTemp = 0;
				float temp = 0;

				while (!reader.EndOfStream)
				{
					var values = reader.ReadLine().Split(',');
					if (values[5] != "")
					{
						temp = float.Parse(values[5], CultureInfo.InvariantCulture.NumberFormat);
					}
					if (temp > highestTemp)
					{
						highestTemp = temp;
					}
				}
				return highestTemp;
			}
		}
		public static float getLowestTemperatureFromCVS(string path)
		{
			using (var reader = new StreamReader(path))
			{
				//get rid of the first line in CSV file
				var headerLine = reader.ReadLine();

				float lowestTemp = float.MaxValue;
				float temp = float.MaxValue;

				while (!reader.EndOfStream)
				{
					var values = reader.ReadLine().Split(',');
					if (values[5] != "")
					{
						temp = float.Parse(values[5], CultureInfo.InvariantCulture.NumberFormat);
					}
					if (temp < lowestTemp)
					{
						lowestTemp = temp;
					}
				}
				return lowestTemp;
			}
		}
		public static string getSerialNumberFromXML(string path)
		{
			// Loading XML file
			XmlDocument MyDoc = new XmlDocument();
			MyDoc.Load(path);

			//Gathering and returning data data
			return MyDoc["OvenCyclingData"]["SerialNumber"].InnerText;
		}
		public static string getCompletedTimeStampFromXML(string path)
		{
			// Loading XML file
			XmlDocument MyDoc = new XmlDocument();
			MyDoc.Load(path);

			//Gathering and returning data data
			return MyDoc["OvenCyclingData"]["CompletedTimeStamp"].InnerText;
		}
	}
}