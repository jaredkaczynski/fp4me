using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Disney
{
    public class Helpers
    {
        public List<ExperienceFastpassAvailability> ExtractFastpassStatus(string fastpassAvailabilityJson)
        {
            return ExtractFastpassStatus(JObject.Parse(fastpassAvailabilityJson));
        }

        public List<ExperienceFastpassAvailability> ExtractFastpassStatus(JObject parsedFastpassAvailabilityJson)
        {
            List<ExperienceFastpassAvailability> results = new List<ExperienceFastpassAvailability>();

            foreach (var experienceGroup in parsedFastpassAvailabilityJson.SelectTokens("experienceGroups").Children())
            {
                foreach (var experience in experienceGroup.SelectTokens("experiences").Children())
                {
                    JToken assetName = parsedFastpassAvailabilityJson.SelectToken("$.assets." + experience["id"] + ".name");
                    results.Add(new ExperienceFastpassAvailability
                    {
                        ExperienceID = (string)experience["id"],
                        ExperienceName = (string)assetName,
                        Availability = (string)experience["status"]
                    });
                }
            }
            return results;
        }
    }
}
