using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fp4me.Web.Data.Models;

namespace fp4me.Web.Data
{

    public static class DbInitializer
    {
        public static void Initialize(DataContext context)
        {
            context.Database.EnsureCreated();

            if (!context.UserPlans.Any())
            {
                context.UserPlans.Add(new UserPlan {
                    Name = "Free",
                    CheckFrequencyInMinutes = 60,
                    PauseFrequencyInMinutes = 60 * 4,
                    MaxAllowedActiveAttractionFastPassRequests = 1,
                    IsPublic = true});
                context.UserPlans.Add(new UserPlan
                {
                    Name = "Beta",
                    CheckFrequencyInMinutes = 5,
                    PauseFrequencyInMinutes = 60,
                    MaxAllowedActiveAttractionFastPassRequests = 3,
                    IsPublic = true
                });
                context.UserPlans.Add(new UserPlan {
                    Name = "Pro",
                    CheckFrequencyInMinutes = 5,
                    PauseFrequencyInMinutes = 60,
                    MaxAllowedActiveAttractionFastPassRequests = 10,
                    IsPublic = true
                });
                context.UserPlans.Add(new UserPlan {
                    Name = "Ultra",
                    CheckFrequencyInMinutes = 0,
                    PauseFrequencyInMinutes = 0,
                    MaxAllowedActiveAttractionFastPassRequests = 10,
                    IsPublic = true
                });
            }

            // Look for any students.
            if (!context.Parks.Any())
            {
                context.Parks.Add(new Park { Abbreviation = "MK", Name = "Magic Kingdom", DisneyApiParkID = "80007944" });
                context.Parks.Add(new Park { Abbreviation = "AK", Name = "Animal Kingdom", DisneyApiParkID = "80007823" });
                context.Parks.Add(new Park { Abbreviation = "HS", Name = "Hollywood Studios", DisneyApiParkID = "80007998" });
                context.Parks.Add(new Park { Abbreviation = "EP", Name = "Epcot", DisneyApiParkID = "80007838" });
                context.SaveChanges();

                var park = context.Parks.Single(p => p.Abbreviation == "MK");
                context.Attractions.Add(new Attraction { Name = "The Barnstormer", Park = park });
                context.Attractions.Add(new Attraction { Name = "Big Thunder Mountain Railroad", Park = park });
                context.Attractions.Add(new Attraction { Name = "Buzz Lightyear's Space Ranger Spin", Park = park });
                context.Attractions.Add(new Attraction { Name = "Dumbo the Flying Elephant", Park = park });
                context.Attractions.Add(new Attraction { Name = "Enchanted Tales with Belle", Park = park });
                context.Attractions.Add(new Attraction { Name = "Haunted Mansion", Park = park });
                context.Attractions.Add(new Attraction { Name = @"""it's a small world""", Park = park });
                context.Attractions.Add(new Attraction { Name = "Jungle Cruise", Park = park });
                context.Attractions.Add(new Attraction { Name = "Peter Pan's Flight", Park = park });
                context.Attractions.Add(new Attraction { Name = "Pirates of the Caribbean", Park = park });
                context.Attractions.Add(new Attraction { Name = "Seven Dwarfs Mine Train", Park = park, Priority = 10 });
                context.Attractions.Add(new Attraction { Name = "Space Mountain", Park = park, Priority = 13 });
                context.Attractions.Add(new Attraction { Name = "Splash Mountain", Park = park });
                context.Attractions.Add(new Attraction { Name = "Tomorrowland Speedway", Park = park });

                park = context.Parks.Single(p => p.Abbreviation == "AK");
                context.Attractions.Add(new Attraction { Name = "Na'vi River Journey", Park = park, Priority = 2 });
                context.Attractions.Add(new Attraction { Name = "DINOSAUR", Park = park });
                context.Attractions.Add(new Attraction { Name = "Expedition Everest - Legend of the Forbidden Mountain", Park = park });
                context.Attractions.Add(new Attraction { Name = "Festival of the Lion King", Park = park });
                context.Attractions.Add(new Attraction { Name = "It's Tough to be a Bug!", Park = park });
                context.Attractions.Add(new Attraction { Name = "Kilimanjaro Safaris", Park = park });
                context.Attractions.Add(new Attraction { Name = "Meet Favorite Disney Pals at Adventurers Outpost", Park = park });
                context.Attractions.Add(new Attraction { Name = "Primeval Whirl", Park = park });
                context.Attractions.Add(new Attraction { Name = "Rivers of Light", Park = park, Priority = 3 });
                context.Attractions.Add(new Attraction { Name = "Avatar Flight of Passage", Park = park, Priority = 1 });
                context.Attractions.Add(new Attraction { Name = "Finding Nemo - The Musical", Park = park });
                context.Attractions.Add(new Attraction { Name = "Kali River Rapids", Park = park });

                park = context.Parks.Single(p => p.Abbreviation == "HS");
                context.Attractions.Add(new Attraction { Name = "Beauty and the Beast-Live on Stage", Park = park });
                context.Attractions.Add(new Attraction { Name = "Fantasmic!", Park = park });
                context.Attractions.Add(new Attraction { Name = "Rock 'n' Roller Coaster Starring Aerosmith", Park = park, Priority = 40 });
                context.Attractions.Add(new Attraction { Name = "Toy Story Mania!", Park = park, Priority = 50 });
                context.Attractions.Add(new Attraction { Name = "Disney Junior - Live on Stage!", Park = park });
                context.Attractions.Add(new Attraction { Name = "For the First Time in Forever: A Frozen Sing-Along Celebration", Park = park });
                context.Attractions.Add(new Attraction { Name = "Indiana Jones™ Epic Stunt Spectacular!", Park = park });
                context.Attractions.Add(new Attraction { Name = "Muppet*Vision 3D", Park = park });
                context.Attractions.Add(new Attraction { Name = "Star Tours – The Adventures Continue", Park = park });
                context.Attractions.Add(new Attraction { Name = "Voyage of The Little Mermaid", Park = park });
                context.Attractions.Add(new Attraction { Name = "The Twilight Zone Tower of Terror™", Park = park, Priority = 55 });
                
                park = context.Parks.Single(p => p.Abbreviation == "EP");
                context.Attractions.Add(new Attraction { Name = "Frozen Ever After", Park = park, Priority = 60 });
                context.Attractions.Add(new Attraction { Name = "IllumiNations: Reflections of Earth", Park = park });
                context.Attractions.Add(new Attraction { Name = "Soarin'", Park = park, Priority = 30 });
                context.Attractions.Add(new Attraction { Name = "Test Track", Park = park, Priority = 35 });
                context.Attractions.Add(new Attraction { Name = "Disney & Pixar Short Film Festival", Park = park });
                context.Attractions.Add(new Attraction { Name = "Living with the Land", Park = park });
                context.Attractions.Add(new Attraction { Name = "RELAUNCHED! Mission: SPACE", Park = park });
                context.Attractions.Add(new Attraction { Name = "Spaceship Earth", Park = park });
                context.Attractions.Add(new Attraction { Name = "Turtle Talk With Crush", Park = park });

            }

            context.SaveChanges();

        }
    }
}


