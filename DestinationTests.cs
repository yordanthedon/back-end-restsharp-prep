using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class DestinationTests : IDisposable
    {
        private RestClient client;
        private string token;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_GetAllDestinations()
        {
            var request = new RestRequest("destination", Method.Get);

            var response = client.Execute(request);


            Assert.Multiple(() =>

            {

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

                Assert.That(response.Content, Is.Not.Empty, "Response content should not be empty");


                var destinations = JArray.Parse(response.Content);

                Assert.That(destinations.Type, Is.EqualTo(JTokenType.Array), "Expected response content to be a JSON array");

                Assert.That(destinations.Count, Is.GreaterThan(0), "Expected at least one destination in the response");


                foreach (var destination in destinations)

                {

                    Assert.That(destination["name"]?.ToString(), Is.Not.Null.And.Not.Empty, "Destination name should not be null or empty");

                    Assert.That(destination["location"]?.ToString(), Is.Not.Null.And.Not.Empty, "Destination location should not be null or empty");

                    Assert.That(destination["description"]?.ToString(), Is.Not.Null.And.Not.Empty, "Destination description should not be null or empty");

                    Assert.That(destination["category"]?.ToString(), Is.Not.Null.And.Not.Empty, "Destination category should not be null or empty");

                    Assert.That(destination["bestTimeToVisit"]?.ToString(), Is.Not.Null.And.Not.Empty, "Destination bestTimeToVisit should not be null or empty");

                    Assert.That(destination["attractions"]?.Type, Is.EqualTo(JTokenType.Array), "Expected Destination attractions content to be a JSON array");

                }

            });
        }

        [Test]
        public void Test_GetDestinationByName()
        {
            var request = new RestRequest("destination", Method.Get);

            var response = client.Execute(request);


            Assert.Multiple(() =>

            {

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

                Assert.That(response.Content, Is.Not.Empty, "Response content should not be empty");


                var destinations = JArray.Parse(response.Content);

                var destination = destinations.FirstOrDefault(d => d["name"]?.ToString() == "New York City");


                Assert.That(destination, Is.Not.Null, "Expected to find destination 'New York City'");

                Assert.That(destination["location"]?.ToString(), Is.EqualTo("New York, USA"), "Location should match");

                Assert.That(destination["description"]?.ToString(), Is.EqualTo("The largest city in the USA, known for its skyscrapers, culture, and entertainment."), "Description should match");

            });
        }

        [Test]
        public void Test_AddDestination()
        {
            var getCategoriesRequest = new RestRequest("category", Method.Get);

            var getCategoriesResponse = client.Execute(getCategoriesRequest);

            var categories = JArray.Parse(getCategoriesResponse.Content);

            var categoryId = categories.First["_id"].ToString();


            var request = new RestRequest("destination", Method.Post);

            request.AddHeader("Authorization", $"Bearer {token}");

            var newDestination = new

            {

                name = "Summer in Machu Picchu",

                location = "Hawaii, USA",

                description = "A beautiful beach with crystal clear waters and white sands.",

                bestTimeToVisit = "April to October",

                attractions = new[] { "Surfing", "Sunbathing", "Snorkeling" },

                category = categoryId

            };

            request.AddJsonBody(newDestination);


            var response = client.Execute(request);


            Assert.Multiple(() =>

            {

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

                Assert.That(response.Content, Is.Not.Empty, "Response content should not be empty");


                var createdDestination = JObject.Parse(response.Content);

                var createdId = createdDestination["_id"]?.ToString();

                Assert.That(createdId, Is.Not.Empty, "Created destination should have an ID");


                var getDestinationRequest = new RestRequest($"destination/{createdId}", Method.Get);

                var getDestinationResponse = client.Execute(getDestinationRequest);


                Assert.That(getDestinationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK ( 200)");

                Assert.That(getDestinationResponse.Content, Is.Not.Empty, "Response content should not be empty");


                var content = JObject.Parse(getDestinationResponse.Content);

                Assert.That(content["name"]?.ToString(), Is.EqualTo(newDestination.name), "Destination name should match the input.");

                Assert.That(content["location"]?.ToString(), Is.EqualTo(newDestination.location), "Destination location should match the input.");

                Assert.That(content["description"]?.ToString(), Is.EqualTo(newDestination.description), "Destination description should match the input.");

                Assert.That(content["bestTimeToVisit"]?.ToString(), Is.EqualTo(newDestination.bestTimeToVisit), "Destination bestTimeToVisit should match the input.");

                Assert.That(content["category"]["_id"]?.ToString(), Is.EqualTo(categoryId), "Destination category ID should match the input.");

                Assert.That(content["attractions"]?.Type, Is.EqualTo(JTokenType.Array), "Expected Destination attractions content to be a JSON array");

                Assert.That(content["attractions"].Count, Is.EqualTo(newDestination.attractions.Length), "Destination attractions should have the correct number of elements.");

            });
        }

        [Test]
        public void Test_UpdateDestination()
        {
            // Step 1: Get all destinations

            var getRequest = new RestRequest("destination", Method.Get);

            var getResponse = client.Execute(getRequest);


            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Failed to retrieve destinations");

            Assert.That(getResponse.Content, Is.Not.Empty, "Get destinations response content is empty");


            var destinations = JArray.Parse(getResponse.Content);

            var destinationToUpdate = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Maui Beach");


            Assert.That(destinationToUpdate, Is.Not.Null, "Destination with name 'Machu Picchu' not found");


            var destinationId = destinationToUpdate["_id"].ToString();


            // Step 2: Update the destination

            var updateRequest = new RestRequest($"destination/{destinationId}", Method.Put);

            updateRequest.AddHeader("Authorization", $"Bearer {token}");

            updateRequest.AddJsonBody(new

            {

                name = "Summer in Machu Picchu",

                bestTimeToVisit = "Summer!",

            });


            var updateResponse = client.Execute(updateRequest);


            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

            Assert.That(updateResponse.Content, Is.Not.Empty, "Update response content should not be empty");


            // Step 3: Verify the update

            var verifyRequest = new RestRequest($"destination/{destinationId}", Method.Get);

            var verifyResponse = client.Execute(verifyRequest);

            var content = JObject.Parse(verifyResponse.Content);


            Assert.Multiple(() =>

            {

                Assert.That(content["name"]?.ToString(), Is.EqualTo("Summer in Machu Picchu"), "Destination name should match the updated value");

                Assert.That(content["bestTimeToVisit"]?.ToString(), Is.EqualTo("Summer!"), "Destination best time to visit should match the updated value");

            });
        }

        [Test]
        public void Test_DeleteDestination()
        {
            var getRequest = new RestRequest("destination", Method.Get);

            var getResponse = client.Execute(getRequest);


            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Failed to retrieve destinations");

            Assert.That(getResponse.Content, Is.Not.Empty, "Get destinations response content is empty");


            var destinations = JArray.Parse(getResponse.Content);

            var destinationToDelete = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Summer in Machu Picchu");


            Assert.That(destinationToDelete, Is.Not.Null, "Destination with name 'Summer in Machu Picchu' not found");


            var destinationId = destinationToDelete["_id"].ToString();

            var deleteRequest = new RestRequest($"destination/{destinationId}", Method.Delete);

            deleteRequest.AddHeader("Authorization", $"Bearer {token}");


            var deleteResponse = client.Execute(deleteRequest);


            Assert.Multiple(() =>

            {

                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");


                var verifyGetRequest = new RestRequest($"destination/{destinationId}", Method.Get);

                var verifyGetResponse = client.Execute(verifyGetRequest);


                Assert.That(verifyGetResponse.Content, Is.Null.Or.EqualTo("null"), "Verify get response content should be empty");

            });
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
