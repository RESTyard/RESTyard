using System;
using System.Collections.Generic;

namespace CarShack.Util
{
    public class NameGenerator
    {
        private readonly List<string> maleNames = new List<string>()
        {
            "Harold",
            "Brock",
            "Franco",
            "Leroy",
            "Brycen",
            "Braylon",
            "Leonidas",
            "Brayden",
            "Jace",
            "Bryce",
            "Jamari",
            "Deon",
            "Jaxson",
            "Brooks",
            "Pierce",
            "Jamie",
            "Ronald",
            "Sebastian",
            "Taylor",
            "Sean",
            "Rogelio",
            "Malik",
            "Donovan",
            "Timothy",
            "Cayden",
            "Micheal",
            "Thomas",
            "Odin",
            "Gilbert",
            "Koen",
            "Adam",
            "Brodie",
            "Brady",
            "Dante",
            "Wyatt",
            "Duncan",
            "Preston",
            "Reece",
            "Elisha",
            "Amari",
            "Rodney",
            "Quincy",
            "Winston",
            "Hugo",
            "Rashad",
            "Ernest",
            "Phillip",
            "Antony",
            "Weston",
            "Sage",
        };

        private readonly List<string> femaleNames = new List<string>()
        {
            "Cheyenne",
            "Jaiden",
            "Aliyah",
            "Rubi",
            "Mariana",
            "Britney",
            "Karen",
            "Izabelle",
            "Katie",
            "Krystal",
            "Whitney",
            "Carla",
            "Sarai",
            "Daniella",
            "Cadence",
            "Jamiya",
            "Kaylah",
            "Bryanna",
            "Leslie",
            "Zaria",
            "Macey",
            "Jordan",
            "Kate",
            "Kali",
            "Catalina",
            "Lexi",
            "Justine",
            "Brianna",
            "Belinda",
            "Zion",
            "Grace",
            "Lillianna",
            "Heidy",
            "Sage",
            "Laila",
            "Alessandra",
            "Adriana",
            "Skye",
            "Arianna",
            "Kaylynn",
            "Arielle",
            "Nora",
            "Liliana",
            "Audrey",
            "Lilyana",
            "Macy",
            "Alicia",
            "Christine",
            "Laura",
            "Jayleen",
        };

        private readonly List<string> lastNames = new List<string>()
        {
             "Meyers",
             "Franco",
             "Miranda",
             "Bright",
             "Barrett",
             "Fischer",
             "Wong",
             "Ellison",
             "Alvarado",
             "Callahan",
             "Hogan",
             "Summers",
             "Jimenez",
             "Burch",
             "Dickson",
             "Skinner",
             "Moss",
             "Mullen",
             "Valentine",
             "Freeman",
             "Kelly",
             "Bruce",
             "Pittman",
             "Montes",
             "Manning",
             "Brewer",
             "Meza",
             "Bridges",
             "Fields",
             "Escobar",
             "Zavala",
             "Tapia",
             "Good",
             "Watkins",
             "Wells",
             "Ray",
             "Robinson",
             "Miller",
             "Payne",
             "Hensley",
             "Jordan",
             "Dennis",
             "Fritz",
             "Cain",
             "Lutz",
             "Torres",
             "Burke",
             "Duke",
             "Berry",
             "Bra",
        };

        private readonly Random rand;

        public NameGenerator(long seed = long.MinValue)
        {
            if (seed == long.MinValue)
            {
                seed = DateTime.Now.Ticks;
            }

            this.rand = new Random((int)seed);
        }

        public string GenerateNext()
        {
            var name = string.Empty;
            var useMale = rand.NextDouble() > 0.5;

            if (useMale)
            {
                name += maleNames[rand.Next(0, maleNames.Count - 1)];
            }
            else
            {
                name += femaleNames[rand.Next(0, femaleNames.Count - 1)];
            }


            name += " " + lastNames[rand.Next(0, lastNames.Count - 1)];

            return name;
        }



    }
}
