using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HackathonSabreBot.RootLuisDialog
{
    [LuisModel("d5349ddb-9855-4e6f-9311-b192a7545f69", "466e21bf4ba440239482b3d6f75ff4b6")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string EntityBookFrom = "Location::FromLocation";
        private const string EntityBookTo = "Location::ToLocation";
        private const string EntityDate = "builtin.datetime.date";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Please type in this format 'Book a flight from Singapore to Hong Kong from 20th October to 27 October'";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("BookFlight")]
        public async Task BookFlight(IDialogContext context, LuisResult result)
        {
            string toLocation = "";
            string fromLocation = "";
            string startDate = "";
            string endDate = "";
            string now = DateTime.Now.Year.ToString();

            //var message = await activity;
            await context.PostAsync($"Sure!");

            //var fligthsQuery = new FligthsQuery();
            
            EntityRecommendation bookFlightEntityRecommendation;

            if (result.TryFindEntity(EntityBookTo, out bookFlightEntityRecommendation))
            {
                toLocation = bookFlightEntityRecommendation.Entity;
                await context.PostAsync(toLocation);
            }

            if (result.TryFindEntity(EntityBookFrom, out bookFlightEntityRecommendation))
            {
                fromLocation = bookFlightEntityRecommendation.Entity;
                await context.PostAsync(fromLocation);
            }

            var dateStartDict = result.Entities[2].Resolution;

            var dateEndDict =  result.Entities[3].Resolution;

            await context.PostAsync(dateEndDict.ToString());
            foreach (KeyValuePair<string, string> entry in dateStartDict)
            {
                // do something with entry.Value or entry.Key
                startDate = entry.Value;
                startDate = startDate.Replace("XXXX", now);

            }

            foreach (KeyValuePair<string, string> entry in dateEndDict)
            {
                // do something with entry.Value or entry.Key
                endDate = entry.Value;
                endDate = endDate.Replace("XXXX", now);

            }

            if(startDate == "" || endDate == "") {
                await context.PostAsync("Please type in this format 'Book a flight from Singapore to Hong Kong from 20th October to 27 October'");
            }

            await context.PostAsync(fromLocation + " to "+ toLocation + " on "+ startDate + " to " + endDate);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.test.sabre.com/v1/shop/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer T1RLAQIS5gCkjBB1EosqTm8QiBBMXRj6FRBC/Hqi9elCoT351J7dgcgRAACgGomLgr8tWgyVO8aRgKumR6zu7/GNb6EqmWybUgTrPRP7yoMwiqX2horRipgVVhB4/VHBSDcgWNGWNjZDDgWWRFyw2/njGvhWL4dRpa2GlJDANm8lJtRZ0ehrRBXabPXT0eZ4xdwDtqbQIyqCzCu2uB73bEo/S82294Evx5vKl7fyD8ahPferTNump9Ydjub14yvVXjboT1vFtPN8y8MlQQ**");
                var response = await client.GetAsync("flights?origin=JFK&destination=LAX&departuredate=" + startDate + "&returndate=" + endDate + "&limit=1&enabletagging=true");

                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    JToken token = JObject.Parse(responseString);

                    dynamic bunchOfObj = token.SelectToken("PricedItineraries");

                    for (int i = 0; i < bunchOfObj.length(); i++) {
                        dynamic secondObj = bunchOfObj[i].AirItinerary.OriginDestinationOptions;
                        for (int x = 0; x < secondObj.length(); x++) {
                            dynamic thirdObj = secondObj[x].LocationCode;
                            await context.PostAsync(thirdObj);
                        }
                    }

                    
                }
                else {
                    await context.PostAsync("THIS DID NOT WORK!!!");
                    await context.PostAsync(response.Content.ToString());
                    await context.PostAsync(response.RequestMessage.ToString());

                }
            }

            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Add("Authorization", "Bearer T1RLAQIS5gCkjBB1EosqTm8QiBBMXRj6FRBC/Hqi9elCoT351J7dgcgRAACgGomLgr8tWgyVO8aRgKumR6zu7/GNb6EqmWybUgTrPRP7yoMwiqX2horRipgVVhB4/VHBSDcgWNGWNjZDDgWWRFyw2/njGvhWL4dRpa2GlJDANm8lJtRZ0ehrRBXabPXT0eZ4xdwDtqbQIyqCzCu2uB73bEo/S82294Evx5vKl7fyD8ahPferTNump9Ydjub14yvVXjboT1vFtPN8y8MlQQ**");
            //    var responseString = await client.GetStringAsync("https://api.sabre.com/v1/shop/flights?origin=JFK&destination=LAX&departuredate=" + startDate + "&returndate=" + endDate + "&limit=1&enabletagging=true");
            //   await context.PostAsync(responseString.ToString());
            //}

            //var request = (HttpWebRequest)WebRequest.Create("https://api.sabre.com/v1/shop/flights?origin=JFK&destination=LAX&departuredate=" + startDate+ "&returndate=" + endDate+ "&limit=1&enabletagging=true");

            //request.ContentType = "application/json";
            //request.ContentLength = data.Length;
            //request.Headers.Add("Authorization", "Bearer T1RLAQIS5gCkjBB1EosqTm8QiBBMXRj6FRBC/Hqi9elCoT351J7dgcgRAACgGomLgr8tWgyVO8aRgKumR6zu7/GNb6EqmWybUgTrPRP7yoMwiqX2horRipgVVhB4/VHBSDcgWNGWNjZDDgWWRFyw2/njGvhWL4dRpa2GlJDANm8lJtRZ0ehrRBXabPXT0eZ4xdwDtqbQIyqCzCu2uB73bEo/S82294Evx5vKl7fyD8ahPferTNump9Ydjub14yvVXjboT1vFtPN8y8MlQQ**");

           // var response = (HttpWebResponse)request.GetResponse();

            //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

          
            context.Wait(this.MessageReceived);
        }

    }
}                       