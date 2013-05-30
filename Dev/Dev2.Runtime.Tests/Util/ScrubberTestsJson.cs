﻿using System.Collections.Generic;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.String.Json;

namespace Dev2.Tests.Runtime.Util
{
    public partial class ScrubberTests
    {
        [TestMethod]
        public void ScrubberScrubJsonWithNonArrayDefinitionExpectedReturnsSameJson()
        {
            //------------------------Setup ----------------------------------------------------------------------
            const string JsonData = "{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
                "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
                "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"}";
            //------------------------Execute---------------------------------------------------------------
            var scrub = Scrubber.Scrub(JsonData, ScrubType.JSon);
            //------------------------Assert Results---------------------------------------------------
            Assert.AreEqual(JsonData,scrub);
        }

        [TestMethod]
        public void ScrubberScrubJsonWhereDataIsArrayWithNoNameExpectNamedArray()
        {
            //------------Setup for test--------------------------
            const string JsonData = "[{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
              "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
              "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"},{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
              "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
              "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"}]";
            const string ExpectedData = "{UnnamedArrayData:[{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
              "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
              "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"},{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
              "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
              "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"}]}";
            //------------Execute Test---------------------------
            var scrub = Scrubber.Scrub(JsonData, ScrubType.JSon);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedData, scrub);
        }      
        
        [TestMethod]
        public void ScrubberScrubJsonWhereDataIsArrayWithNameExpectSameJson()
        {
            //------------Setup for test--------------------------
            const string JsonData = "{Colours:[{color: \"red\",value: \"#f00\"},{color: \"green\",value: \"#0f0\"},{color: \"blue\",value: \"#00f\"},{color: \"cyan\",value: \"#0ff\"},{color: \"magenta\",value: \"#f0f\"},{color: \"yellow\",value: \"#ff0\"}," +
                "{color: \"black\",value: \"#000\"}]}";
            //------------Execute Test---------------------------
            var scrub = Scrubber.Scrub(JsonData, ScrubType.JSon);
            //------------Assert Results-------------------------
            Assert.AreEqual(JsonData, scrub);            
        }

        [TestMethod]
        public void VerifyScrubWhereRegularJsonExpectCorrectOutputDescription()
        {
            //------------Setup for test--------------------------
            const string JsonData = "{color: \"red\",value: \"#f00\"}";
            var paths = new List<IPath>
            {
                new JsonPath("color","color","","red"),
                new JsonPath("value","value","","#f00")
            };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            VerifyScrub(JsonData,paths);
        }

        [TestMethod]
        public void VerifyScrubJsonWhereDataIsArrayWithNameExpectCorrectOutputDescription()
        {
            //------------Setup for test--------------------------
            const string JsonData = "{Colours:[{color: \"red\",value: \"#f00\"},{color: \"green\",value: \"#0f0\"},{color: \"blue\",value: \"#00f\"},{color: \"cyan\",value: \"#0ff\"},{color: \"magenta\",value: \"#f0f\"},{color: \"yellow\",value: \"#ff0\"}," +
                "{color: \"black\",value: \"#000\"}]}";
            var paths = new List<IPath>
            {
                new JsonPath("Colours().color","Colours().color","","red,green,blue,cyan,magenta,yellow,black"),
                new JsonPath("Colours().value","Colours().value","","#f00,#0f0,#00f,#0ff,#f0f,#ff0,#000")
            };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            VerifyScrub(JsonData, paths);
        }

        [TestMethod]
        public void VerifyScrubJsonWhereComplexStructureExpectCorrectOutputDescription()
        {
            //------------Setup for test--------------------------
            const string JsonData = "{\"id\": \"0001\",\"type\": \"donut\",\"name\": \"Cake\",\"ppu\": 0.55,\"batters\":{\"batter\":[{ \"id\": \"1001\", \"type\": \"Regular\" },{ \"id\": \"1002\", \"type\": \"Chocolate\" },{ \"id\": \"1003\", \"type\": \"Blueberry\" }," +
                "{ \"id\": \"1004\", \"type\": \"Devil's Food\" }]},\"topping\":[{ \"id\": \"5001\", \"type\": \"None\" },{ \"id\": \"5002\", \"type\": \"Glazed\" },{ \"id\": \"5005\", \"type\": \"Sugar\" },{ \"id\": \"5007\", \"type\": \"Powdered Sugar\" }," +
                "{ \"id\": \"5006\", \"type\": \"Chocolate with Sprinkles\" },{ \"id\": \"5003\", \"type\": \"Chocolate\" },{ \"id\": \"5004\", \"type\": \"Maple\" }]}";

            var paths = new List<IPath>
            {
                new JsonPath("id","id","","0001"),
                new JsonPath("type","type","","donut"),
                new JsonPath("name","name","","Cake"),
                new JsonPath("ppu","ppu","","0.55"),
                new JsonPath("batters.batter().id","batters.batter().id","","1001,1002,1003,1004"),
                new JsonPath("batters.batter().type","batters.batter().type","","Regular,Chocolate,Blueberry,Devil's Food"),
                new JsonPath("topping().id","topping().id","","5001,5002,5005,5007,5006,5003,5004"),
                new JsonPath("topping().type","topping().type","","None,Glazed,Sugar,Powdered Sugar,Chocolate with Sprinkles,Chocolate,Maple")
            };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            VerifyScrub(JsonData, paths);
        }
    }
}
