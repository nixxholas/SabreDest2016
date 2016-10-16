using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string EntityDate = "builtin.datetime.date";
        private const string EntityBookHotel = "Location";
        private const string EntityWeather = "Location";

        string now = DateTime.Now.Year.ToString();

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Please type in this format 'Book a flight from Singapore to Hong Kong from 20th October to 27 October'";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [LuisIntent("GetBoardingTicket")]
        public async Task GetBoardingTicket(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Sure!");
            List<Attachment> a = new List<Attachment>();

            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            string rand = RandomString(5);

            a.Add(GetHeroCard(
                  "Here you go!",
                  "",
                  "",
                  new CardImage(url: "https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=" + rand),
                  new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=" + rand)));

            resultMessage.Attachments = a;

            await context.PostAsync(resultMessage);

         context.Wait(this.MessageReceived);
        
    }

    [LuisIntent("GetHotel")]
    public async Task GetHotel(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"Sure!");

        EntityRecommendation bookHotelEntityRecommendation;

        /*for (int i = 0; i < result.Entities.Count; i++) {
            await context.PostAsync(result.Entities[i].Type);
        }*/
        string toLocation = "";
        string startDate = "";
        string endDate = "";
        List<Attachment> a = new List<Attachment>();


        if (result.TryFindEntity(EntityBookHotel, out bookHotelEntityRecommendation))
        {
            toLocation = bookHotelEntityRecommendation.Entity;
            //await context.PostAsync(toLocation);
        }

        //string r = JsonConvert.SerializeObject(result);
        //await context.PostAsync(r);

        var dateStartDict = result.Entities[2].Resolution;

        var dateEndDict = result.Entities[3].Resolution;

        //await context.PostAsync(dateEndDict.ToString());
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

        await context.PostAsync("Finding hotels around " + toLocation + " from " + startDate + " to " + endDate);


        var client = new RestClient("http://terminal2.expedia.com/x/mhotels/search?city=SIN&checkInDate=2016-10-20&checkOutDate=2016-10-30&room1=2&apikey=48RGOAbNOn84uIQS94ppK9uEBRtNdzYL");
        var request = new RestRequest(Method.GET);
        request.AddHeader("postman-token", "8e3cd118-4c4b-6f23-c61c-72bf383c0939");
        request.AddHeader("cache-control", "no-cache");
        IRestResponse response = client.Execute(request);

        var reply = context.MakeMessage();

        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;



        if (response.StatusCode == HttpStatusCode.OK)
        {
            string responseString = response.Content.ToString();
            JObject jsonObject = JObject.Parse(responseString);

            string link = (string)jsonObject["deepLinkUrl"];

            for (int i = 0; i < 25; i++)
            {
                JObject hotelsFound = (JObject)jsonObject["hotelList"][i];

                string name = (string)hotelsFound["name"];
                string shortDescription = (string)hotelsFound["shortDescription"];
                string lowRate = (string)hotelsFound["lowRate"];

                a.Add(GetHeroCard(
               name,
               "Price Per Night : USD $" + lowRate,
               shortDescription,
               new CardImage(url: "http://www.secretflying.com/wp-content/uploads/2015/02/expedia-logo.jpg"),
               new CardAction(ActionTypes.OpenUrl, "Learn more", value: link)));

            }
            reply.Attachments = a;

            await context.PostAsync(reply);


        }
        else
        {
            await context.PostAsync(response.Headers.ToString());
            await context.PostAsync(response.Content.ToString());
            await context.PostAsync(response.ResponseStatus.ToString());
        }

        context.Wait(this.MessageReceived);
    }

    [LuisIntent("BookFlight")]
    public async Task BookFlight(IDialogContext context, LuisResult result)
    {
        string toLocation = "";
        string fromLocation = "";
        string startDate = "";
        string endDate = "";

       // string baba = (string) JsonConvert.SerializeObject(result);

            //await context.PostAsync(baba);
            await context.PostAsync($"Sure");

        //var fligthsQuery = new FligthsQuery();

        EntityRecommendation bookFlightEntityRecommendation;

            /*for (int i = 0; i < result.Entities.Count; i++) {
                await context.PostAsync(result.Entities[i].Type);
            }*/

            fromLocation = "New York";
            toLocation = "Los Angeles";
                //await context.PostAsync(fromLocation);

            // do something with entry.Value or entry.Key
            startDate = "2016-10-18";
            startDate = startDate.Replace("XXXX", now);


            // do something with entry.Value or entry.Key
            endDate = "2016-10-28";
            endDate = endDate.Replace("XXXX", now);
       
        if (startDate == "" || endDate == "")
        {
            await context.PostAsync("Please type in this format 'Book a flight from Singapore to Hong Kong from 20th October to 27 October'");
        }

        await context.PostAsync(fromLocation + " to " + toLocation + " on " + startDate + " to " + endDate);

            List<Attachment> a = new List<Attachment>();

            var resultMessage = context.MakeMessage();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://api.test.sabre.com/v1/shop/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer T1RLAQIS5gCkjBB1EosqTm8QiBBMXRj6FRBC/Hqi9elCoT351J7dgcgRAACgGomLgr8tWgyVO8aRgKumR6zu7/GNb6EqmWybUgTrPRP7yoMwiqX2horRipgVVhB4/VHBSDcgWNGWNjZDDgWWRFyw2/njGvhWL4dRpa2GlJDANm8lJtRZ0ehrRBXabPXT0eZ4xdwDtqbQIyqCzCu2uB73bEo/S82294Evx5vKl7fyD8ahPferTNump9Ydjub14yvVXjboT1vFtPN8y8MlQQ**");
            var response = await client.GetAsync("flights?origin=JFK&destination=LAX&departuredate=2016-10-18&returndate=2016-10-28&limit=1&enabletagging=true");

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                JObject jsonObject = JObject.Parse(responseString);

                    int x = 0;

                //await context.PostAsync(ArrivalAirport);

                string MarketingAirline = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
                    ["OriginDestinationOption"][0]["FlightSegment"][0]["MarketingAirline"]["Code"];

                //await context.PostAsync(MarketingAirline);

                string DepartureDateTime = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
                    ["OriginDestinationOption"][0]["FlightSegment"][0]["DepartureDateTime"];

                //await context.PostAsync(DepartureDateTime);

                string ArrivalDateTime = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
                    ["OriginDestinationOption"][0]["FlightSegment"][0]["ArrivalDateTime"];

                //await context.PostAsync(ArrivalDateTime);

                string DepartureDateTimeSecond = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
                ["OriginDestinationOption"][1]["FlightSegment"][0]["DepartureDateTime"];

                //await context.PostAsync(DepartureDateTime);

                string ArrivalDateTimeSecond = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
                    ["OriginDestinationOption"][1]["FlightSegment"][0]["ArrivalDateTime"];

                //await context.PostAsync(ArrivalDateTime);

                string FlightNumber = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
                            ["OriginDestinationOption"][0]["FlightSegment"][0]["FlightNumber"];

                string FlightNumberSecond = (string)jsonObject["PricedItineraries"][x]["AirItinerary"]["OriginDestinationOptions"]
        ["OriginDestinationOption"][1]["FlightSegment"][0]["FlightNumber"];

                //await context.PostAsync(FlightNumber);

                string CurrencyCode = (string)jsonObject["PricedItineraries"][x]["AirItineraryPricingInfo"]["PTC_FareBreakdowns"]
                            ["PTC_FareBreakdown"]["PassengerFare"]["TotalFare"]["CurrencyCode"];

                //await context.PostAsync(CurrencyCode);

                string Amount = (string)jsonObject["PricedItineraries"][x]["AirItineraryPricingInfo"]["PTC_FareBreakdowns"]
                    ["PTC_FareBreakdown"]["PassengerFare"]["TotalFare"]["Amount"];

                    string DepartureAirport = "JFK";
                    string ArrivalAirport = "LAX";

                    a.Add(GetHeroCard(
              "2 way trip",
              "Price: " + CurrencyCode + " $" + Amount,
              DepartureAirport + " ------> " + ArrivalAirport + " Departure: " + DepartureDateTime + " Arrival: " + ArrivalDateTime +" "+ ArrivalAirport + " ------> " + DepartureAirport + " "+ "Departure: " + DepartureDateTimeSecond+ " Arrival: " + ArrivalDateTimeSecond,
              new CardImage(url: "https://www.sabre.com/brand/assets/img/logo_sabre_white.jpg"),
              new CardAction(ActionTypes.OpenUrl, "Purchase now", value: "http://testprogress.azurewebsites.net/")));


                    resultMessage.Attachments = a;

                    await context.PostAsync(resultMessage);
                }

            else
            {
                await context.PostAsync("THIS DID NOT WORK!!!");
                await context.PostAsync(response.Content.ToString());
                await context.PostAsync(response.RequestMessage.ToString());

            }
        }

        context.Wait(this.MessageReceived);
    }

        [LuisIntent("GetWeather")]
        public async Task GetWeather(IDialogContext context, LuisResult result)
        {   string location = "";
            try
            {
                EntityRecommendation getWeatherEntityRecommendation;

                if (result.TryFindEntity(EntityWeather, out getWeatherEntityRecommendation))
                {
                    location = getWeatherEntityRecommendation.Entity;
                    await context.PostAsync("The weather at " + location + " now is 30 Degrees Celsius.");
                }
            }
            catch (Exception e)
            {
                await context.PostAsync(e.ToString());
            }

            context.Wait(this.MessageReceived);
        }


        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
    {
        var heroCard = new HeroCard
        {
            Title = title,
            Subtitle = subtitle,
            Text = text,
            Images = new List<CardImage>() { cardImage },
            Buttons = new List<CardAction>() { cardAction },
        };

        return heroCard.ToAttachment();
    }

    private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
    {
        var heroCard = new ThumbnailCard
        {
            Title = title,
            Subtitle = subtitle,
            Text = text,
            Images = new List<CardImage>() { cardImage },
            Buttons = new List<CardAction>() { cardAction },
        };

        return heroCard.ToAttachment();
    }

}

}                       