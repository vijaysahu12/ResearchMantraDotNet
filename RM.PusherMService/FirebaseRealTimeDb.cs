using Firebase.Database;
using Firebase.Database.Query;
using RM.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RM.NotificationService
{
    public class FirebaseRealTimeDb
    {
        readonly FirebaseClient firebaseClient;// new("https://mobileauthv3-c6f14-default-rtdb.firebaseio.com/"); 
        public IConfiguration _configuration { get; }
        public FirebaseRealTimeDb(IConfiguration configuration)
        {
            _configuration = configuration;
           // firebaseClient = new FirebaseClient(_configuration["AppSettings:firebaseClient_RealTimeDb"]);
            firebaseClient = new FirebaseClient(_configuration["AppSettings:firebaseClient_RealTimeDb_local"]);

//#if DEBUG

//            firebaseClient = new FirebaseClient(_configuration["AppSettings:firebaseClient_RealTimeDb_local"]);
//#endif
        }

        //Table = TopLooser or TopGainers Or Index

        //public async Task<CamrillaScanner> Read(string table)
        //{
        //    var firebaseResponse = await firebaseClient
        //      .Child(table)
        //      .OnceSingleAsync<CamrillaScanner>();

        //    return firebaseResponse ?? new CamrillaScanner();
        //}

        public async Task<CamrillaScanner> Read(string table)
        {
            var list = await firebaseClient
                .Child(table)
                .Child("Breakfast")
                .OnceSingleAsync<List<CamrillaR4Model>>();

            return new CamrillaScanner
            {
                CurrentDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Breakfast = list ?? new List<CamrillaR4Model>()
            };
        }


        public async Task<ScalpingStrategyScanner> ReadScalping(string table)
        {
            var list = await firebaseClient
                .Child(table)
                .Child("Scalping")
                .OnceSingleAsync<List<ScalpingStrategyStocks>>();

            return new ScalpingStrategyScanner
            {
                CurrentDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Stocks = list ?? new List<ScalpingStrategyStocks>()
            };
        }

        public async Task<bool> UpdateRealTimeMarket(string table, string data)
        {
            await firebaseClient.Child("RealTimeMarketValues").Child(table).PutAsync(data);
            return true;
        }
        public async Task<bool> UpdateRealTimeMarket(string table, object data)
        {
            //await firebaseClient.Child("RealTimeMarketValues").Child("TopGainers").PutAsync(JsonConvert.SerializeObject(topGainersTemp));
            await firebaseClient.Child("RealTimeMarketValues").Child(table).PutAsync(data);
            return true;
        }

        //public async Task<bool> UpdateBreakfastScanner(string data)
        //{
        //    // TODO: Currently saving BreakfastScanner data under "BreakfastScanner/Scalping". This is a temporary structure for grouping. In the future, split "Scalping" and "BreakfastScanner" into separate root-level nodes if needed.

        //    await firebaseClient.Child("BreakfastScanner").PutAsync(data);

        //    return true;
        //}

        public async Task<bool> UpdateBreakfastChildNode(string childKey, string jsonData)
        {
            // jsonData here should already be a JSON array: [ {}, {}, ... ]
            var parsedJson = JsonConvert.DeserializeObject<object>(jsonData);

            var updateData = new Dictionary<string, object>
            {
                { childKey, parsedJson }
            };

            await firebaseClient
                .Child("BreakfastScanner")
                .PatchAsync(updateData);

            return true;
        }

        public async Task<bool> UpdateBreakfastScanner(string childKey, object data)
        {
            var updateData = new Dictionary<string, object>
            {
                { childKey, data }
            };

            await firebaseClient
                .Child("BreakfastScanner")
                .PatchAsync(updateData);

            return true;
        }

        public async Task<bool> UpdateBreakfastScanner(string jsonData)
        {
            var parsedJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

            await firebaseClient
                .Child("BreakfastScanner")
                .PatchAsync(parsedJson); // Patch to preserve existing nodes like Scalping

            return true;
        }

        //public async Task<bool> ToggleIsBottomSheetAsync()
        //{
        //    try
        //    {
        //        var currentsValue = await firebaseClient.Child("IsBottomSheet").OnceSingleAsync<bool>();
        //        Console.WriteLine("Read after false: " + currentsValue); // should be false
        //        // Step 1: Set IsBottomSheet = false
        //        await firebaseClient.Child("IsBottomSheet").PutAsync(false);
        //        Console.WriteLine("Set IsBottomSheet to false");

        //        // Optional: Read to force update (flush)
        //        var currentValue = await firebaseClient.Child("IsBottomSheet").OnceSingleAsync<bool>();
        //        Console.WriteLine("Read after false: " + currentValue); // should be false

        //        // Step 2: Wait 2–3 seconds
        //        await Task.Delay(TimeSpan.FromSeconds(3));

        //        // Step 3: Set IsBottomSheet = true
        //        await firebaseClient.Child("IsBottomSheet").PutAsync(true);
        //        Console.WriteLine("Set IsBottomSheet to true");

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Firebase update failed: " + ex.Message);
        //        return false;
        //    }
        //}

        public async Task UpdatePromotionDataAsync(string key, object data)
        {
            await firebaseClient
                .Child(key)
                .PutAsync(data);
        }


        public async Task<bool> UpdateScalpingScanner(string data)
        {
            await firebaseClient
                            .Child("BreakfastScanner")    // e.g., "BreakfastScanner"
                            .Child("Scalping")      // e.g., "Scalping Scanner"
                            .PutAsync(data);

            return true;
        }

        public async Task UpdateCreatedDateTimeForRealTimeMarketValues(bool updateDate)
        {
            if (updateDate)
            {
                CurrentDateClass.CurrentDate = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
               // CurrentDateClass.CurrentDate = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");
                await firebaseClient.Child("RealTimeMarketValues").Child("CurrentDate").PutAsync(JsonConvert.SerializeObject(CurrentDateClass.CurrentDate));
            }
        }
        public async Task UpdateCreatedDateTimeForBreakfast(bool updateDate)
        {
            if (!updateDate)
                return;

            try
            {
                CurrentDateClass.CurrentDate = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss");

                await firebaseClient
                    .Child("BreakfastScanner")
                    .Child("CurrentDate")
                    .PutAsync(JsonConvert.SerializeObject(CurrentDateClass.CurrentDate));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateCreatedDateTimeForBreakfast: " + ex.Message);
                // Optional: rethrow or log it
                throw;
            }
        }

        public async Task UpdateCreatedDateTimeForScalping(bool updateDate)
        {
            if (updateDate)
            {
                CurrentDateClass.CurrentDate = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                await firebaseClient.Child("ScalpingScanner").Child("CurrentDate").PutAsync(JsonConvert.SerializeObject(CurrentDateClass.CurrentDate));
            }
        }

        public async Task<CamrillaScanner> GetBreakFastNotification()
        {
            // Retrieve the data
            var firebaseResponse = await firebaseClient
                .Child("BreakfastScanner")
                .OnceAsJsonAsync();
            // Parse the JSON directly into BreakfastResponse
            return JsonConvert.DeserializeObject<CamrillaScanner>(firebaseResponse);
        }

        public async Task<List<ScalpingStrategyStocks>> GetScalpingNotification()
        {
            var firebaseResponse = await firebaseClient
                .Child("BreakfastScanner")
                .Child("Scalping")
                .OnceAsJsonAsync();

            return JsonConvert.DeserializeObject<List<ScalpingStrategyStocks>>(firebaseResponse);
        }

        /// <summary>
        /// It will clear the BreakfastScanner data from firebase. 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearBreakfastData()
        {
            try
            {
                // ----------------->  !! THIS WILL REMOVE ALL THE DATA OF BREAKFASTSCANNER FROM FIREBASE REALTIME DB !! <----------------------------
                //CamrillaScanner camrillaScanner = new()
                //{
                //    Breakfast = new(),
                //    CurrentDate = DateTime.Now.ToString("yy-MM-dd HH:mm:ss")
                //};

                // This removes the entire 'BreakfastScanner' node and all its children
                await firebaseClient.Child("BreakfastScanner").DeleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateNotification(object data)
        {
            await firebaseClient.Child("Notification").PutAsync(data);
            return true;
        }

    }



}
