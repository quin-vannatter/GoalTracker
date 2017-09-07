/* Program.cs
 * A program that takes dates and creates waiting goals.
 * 
 * Revision History
 *      Quinlan Vannatter, 2016.08.02: Created
 * 
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace GoalTracker
{
    class Program
    {
        // Location of dates file.
        const string FILE_LOCATION = "dates.txt";

        // Music.
        const string FILE_MUSIC = "music.mp3";

        // Formats.
        const string DATE_FORMAT_YEARS = "{0} years";
        const string DATE_FORMAT_MONTHS = "{0} months";
        const string DATE_FORMAT_WEEKS = "{0} weeks";
        const string DATE_FORMAT_DAYS = "{0} days";
        const string DATE_FORMAT_TIME = "{0}:{1}:{2}";
        const string LINE_FORMAT = "[{0}] - {1}";

        // Delimiter for time elements.
        const string DATE_DELIMITER = ", ";

        const string CURRENT = "Current:               {0}";
        const string NEXT_GOAL = "Next Goal:             {0}";
        const string NEXT_GOAL_DATE = "Next Goal Date:        {0}, {1}";
        const string TIME_UNTIL_GOAL = "Time Unit Next Goal:   {0}";
        const string GOAL_COUNT = "Number of Goals:       {0}";

        // Messages.
        const string MSG_ERR_BAD_DATE = "The date could not be parsed.";
        const string MSG_ERR_BAD_INDEX = "The index chosen is in the incorrect format.";
        const string MSG_ERR_INDEX_BAD_RANGE = "The index is out of range.";

        // Number of milliseconds in a second.
        const int ONE_SECOND = 1000;

        // Number of seconds in an minute.
        const int MINUTE = 60;

        // Number of seconds in an hour.
        const int HOUR = 60 * MINUTE;

        // Number of seconds in a day.
        const int DAY = 24 * HOUR;

        // Number of seconds in a week, month, year.
        const int WEEK = 7 * DAY;
        const int MONTH = 30 * DAY;
        const int YEAR = 365 * DAY;

        // Format for 2 digit numbers.
        const string TWO_DIGITS = "00";

        // Delimiter for date elements.
        const char DELIMITER = '|';

        // Holds the dates from the file location.
        static DateTime programDate;

        // Whether or not if the program is running.
        static bool running;
        static bool firstCheck;

        /// <summary>
        /// The start point of the program.
        /// </summary>
        /// <param name="args">These aren't used. Program arguments.</param>
        static void Main(string[] args)
        {
            // On load, we don't want the music to play right away if goals have been made.
            firstCheck = true;

            // Program is running.
            running = true;

            // Create the file if it doesn't exist.
            if (!File.Exists(FILE_LOCATION)) File.Create(FILE_LOCATION).Close();

            // Get the lines from the file.
            string [] lines = File.ReadAllLines(FILE_LOCATION);
            int index = -1;

            // Wait until the user chooses a line index.
            while (index == -1)
            {
                Console.Clear();

                // Loop through the lines, output each one.
                for (int i = 0; i < lines.Length; i++)
                {
                    DateTime currentDate;
                    if(TryGetDate(lines[i],out currentDate))
                        Console.WriteLine(string.Format(LINE_FORMAT, i, currentDate.ToString()));
                    else
                        Console.WriteLine(MSG_ERR_BAD_DATE);
                }

                // Insert an empty line.
                Console.WriteLine();

                // Attempt to get the user's choice for the index, if out of range change back, show message.
                string input = Console.ReadLine();
                if(int.TryParse(input,out index))
                {
                    if(index >= lines.Length || index < 0)
                    {
                        index = -1;
                        Console.WriteLine(MSG_ERR_INDEX_BAD_RANGE);
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine(MSG_ERR_BAD_INDEX);
                    Console.ReadKey();
                }
            }
            // Set the current date.
            programDate = new DateTime();
            TryGetDate(lines[index], out programDate);

            // While the program is running.
            new Thread(new ThreadStart(RunProgram)).Start();

            // Wait for the user to press something.
            Console.ReadKey();
            running = false;
        }

        /// <summary>
        /// Thread where the program runs.
        /// </summary>
        static void RunProgram()
        {
            int lastGoal = 0;

            // While the program is running.
            while(running)
            {
                float goal = DAY;
                int goalCount = 0;
                int seconds = GetSeconds(programDate);
                while(goal<seconds)
                {
                    goal *= 2f;
                    goalCount++;
                }

                if (goalCount > lastGoal)
                {
                    lastGoal = goalCount;
                    if (!firstCheck) Process.Start(FILE_MUSIC);
                }

                Console.Clear();
                Console.WriteLine(string.Format(CURRENT, GetDateFormat(seconds)));
                Console.WriteLine(string.Format(NEXT_GOAL, GetDateFormat(goal)));
                Console.WriteLine(string.Format(NEXT_GOAL_DATE, programDate.AddSeconds(goal).ToLongDateString(), 
                    programDate.AddSeconds(goal).ToLongTimeString()));
                Console.WriteLine(string.Format(TIME_UNTIL_GOAL, GetDateFormat(goal - seconds)));
                Console.WriteLine(string.Format(GOAL_COUNT, goalCount));
                Thread.Sleep(ONE_SECOND);
                firstCheck = false;
            }
        }

        /// <summary>
        /// Gets the current number of seconds in a range of dates.
        /// </summary>
        /// <param name="date">Date being used in the calculation.</param>
        /// <returns>Return the number of seconds.</returns>
        static int GetSeconds(DateTime date)
        {
            // Return the number of seconds.
            return (int)(DateTime.Now - date).TotalSeconds;
        }

        /// <summary>
        /// Gets the date from a line in the file.
        /// </summary>
        /// <param name="line">Line being parsed.</param>
        /// <returns>Resulting date.</returns>
        static bool TryGetDate(string line, out DateTime output)
        {
            try
            {
                // Get the date elements.
                string[] dateElements = line.Split(DELIMITER);
                int year = int.Parse(dateElements[0]);
                int month = int.Parse(dateElements[1]);
                int day = int.Parse(dateElements[2]);
                int hour = int.Parse(dateElements[3]);
                int minute = int.Parse(dateElements[4]);
                int second = int.Parse(dateElements[5]);

                // Return the number of seconds.
                output = new DateTime(year, month, day, hour, minute, second);
                return true;
            }
            catch
            {
                // Return empty date and false.
                output = new DateTime();
                return false;
            }
        }

        /// <summary>
        /// Formats a sum of seconds into a nice readable string.
        /// </summary>
        /// <param name="totalSeconds">Total number of seconds being parsed.</param>
        /// <returns>A string containing the total seconds in a nice readable string.</returns>
        static string GetDateFormat(float totalSeconds)
        {
            // Holds formated values.
            List<string> elements = new List<string>();

            // Values the total seconds will be divided into.
            int years = 0;
            int months = 0;
            int weeks = 0;
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            // Take out values until total seconds is 0.
            while (totalSeconds > 0)
            {
                if(totalSeconds >= YEAR)
                {
                    totalSeconds -= YEAR;
                    years++;
                }
                else if (totalSeconds >= MONTH)
                {
                    totalSeconds -= MONTH;
                    months++;
                }
                else if (totalSeconds >= WEEK)
                {
                    totalSeconds -= WEEK;
                    weeks++;
                }
                else if(totalSeconds >= DAY)
                {
                    totalSeconds -= DAY;
                    days++;
                }
                else if(totalSeconds >= HOUR)
                {
                    totalSeconds -= HOUR;
                    hours++;
                }
                else if(totalSeconds >= MINUTE)
                {
                    totalSeconds -= MINUTE;
                    minutes++;
                }
                else
                {
                    seconds = (int)totalSeconds;
                    totalSeconds = 0;
                }
            }

            // Get elements.
            if (years > 0) elements.Add(string.Format(DATE_FORMAT_YEARS, years));
            if (months > 0) elements.Add(string.Format(DATE_FORMAT_MONTHS, months));
            if (weeks > 0) elements.Add(string.Format(DATE_FORMAT_WEEKS, weeks));
            if (days > 0) elements.Add(string.Format(DATE_FORMAT_DAYS, days));

            if (hours > 0 || minutes > 0 || seconds > 0) 
                elements.Add(string.Format(DATE_FORMAT_TIME, hours,minutes.ToString(TWO_DIGITS),seconds.ToString(TWO_DIGITS)));

            return string.Join(DATE_DELIMITER, elements);
        }
    }
}
